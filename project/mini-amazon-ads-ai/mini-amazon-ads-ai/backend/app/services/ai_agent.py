"""Claude Agent — 自動分析 Amazon 廣告數據並給建議"""
import json
import uuid
from anthropic import Anthropic
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.config import settings
from app.core.clickhouse import get_clickhouse_client
from app.models import Recommendation


# ============ Tools 定義 ============

TOOLS = [
    {
        "name": "query_metrics",
        "description": "查詢 campaign 過去 N 天的廣告指標(每日聚合)",
        "input_schema": {
            "type": "object",
            "properties": {
                "campaign_id": {"type": "string"},
                "days": {"type": "integer", "minimum": 1, "maximum": 90},
            },
            "required": ["campaign_id", "days"],
        },
    },
    {
        "name": "compare_with_target",
        "description": "比較 campaign 實際 ACOS 與目標 ACOS 的差距",
        "input_schema": {
            "type": "object",
            "properties": {
                "campaign_id": {"type": "string"},
            },
            "required": ["campaign_id"],
        },
    },
    {
        "name": "create_recommendation",
        "description": "把分析結論寫成一筆建議,儲存到資料庫供使用者審核",
        "input_schema": {
            "type": "object",
            "properties": {
                "type": {
                    "type": "string",
                    "enum": ["bid_down", "bid_up", "pause", "budget_up", "budget_down"],
                },
                "reasoning": {"type": "string", "description": "為什麼建議這個動作"},
                "action_detail": {"type": "object", "description": "具體動作參數,例如 {percent: -20}"},
            },
            "required": ["type", "reasoning", "action_detail"],
        },
    },
]


# ============ Tool 實作 ============

async def execute_tool(
    tool_name: str,
    tool_input: dict,
    target_acos: float,
    tenant_id: str,
    campaign_id: str,
    db: AsyncSession,
) -> str:
    """根據 LLM 要求的工具執行對應動作"""

    if tool_name == "query_metrics":
        ch = get_clickhouse_client()
        rows = ch.query(f"""
            SELECT
                toDate(hour) AS day,
                sum(impressions),
                sum(clicks),
                sum(spend),
                sum(sales),
                if(sum(sales) > 0, sum(spend) / sum(sales) * 100, 0) AS acos
            FROM ad_metrics_hourly
            WHERE campaign_id = '{tool_input['campaign_id']}'
              AND hour >= now() - INTERVAL {tool_input['days']} DAY
            GROUP BY day
            ORDER BY day
        """).result_rows
        return json.dumps([
            {
                "date": str(r[0]),
                "impressions": int(r[1]),
                "clicks": int(r[2]),
                "spend": round(float(r[3]), 2),
                "sales": round(float(r[4]), 2),
                "acos": round(float(r[5]), 2),
            }
            for r in rows
        ])

    elif tool_name == "compare_with_target":
        ch = get_clickhouse_client()
        rows = ch.query(f"""
            SELECT
                if(sum(sales) > 0, sum(spend) / sum(sales) * 100, 0) AS actual_acos,
                sum(spend) AS total_spend,
                sum(sales) AS total_sales
            FROM ad_metrics_hourly
            WHERE campaign_id = '{tool_input['campaign_id']}'
              AND hour >= now() - INTERVAL 7 DAY
        """).result_rows
        if not rows:
            return json.dumps({"error": "no data"})
        actual_acos, spend, sales = rows[0]
        return json.dumps({
            "actual_acos_7d": round(float(actual_acos), 2),
            "target_acos": target_acos,
            "gap": round(float(actual_acos) - target_acos, 2),
            "total_spend_7d": round(float(spend), 2),
            "total_sales_7d": round(float(sales), 2),
        })

    elif tool_name == "create_recommendation":
        rec = Recommendation(
            tenant_id=uuid.UUID(tenant_id),
            campaign_id=uuid.UUID(campaign_id),
            type=tool_input["type"],
            reasoning=tool_input["reasoning"],
            suggested_action=tool_input["action_detail"],
            status="pending",
        )
        db.add(rec)
        await db.commit()
        await db.refresh(rec)
        return json.dumps({"id": str(rec.id), "status": "saved"})

    return json.dumps({"error": f"unknown tool: {tool_name}"})


# ============ Agent Loop ============

async def run_ads_agent(
    campaign_id: str,
    campaign_name: str,
    target_acos: float,
    user_question: str | None,
    tenant_id: str,
    db: AsyncSession,
) -> dict:
    """
    Agent 主迴圈:
    1. 給 LLM 任務
    2. LLM 決定用哪個工具
    3. 執行工具 → 結果回給 LLM
    4. 重複直到 LLM 說「完成」
    """
    client = Anthropic(api_key=settings.ANTHROPIC_API_KEY)

    system_prompt = f"""你是一個 Amazon 廣告分析專家。

當前任務:分析 campaign「{campaign_name}」(ID: {campaign_id})
目標 ACOS:{target_acos}%

請按照這個流程:
1. 先用 compare_with_target 看整體狀況
2. 用 query_metrics 看趨勢(建議 14 天)
3. 找出問題後,用 create_recommendation 給出具體建議
4. 最後用繁體中文總結你的分析

請直接開始,不要問使用者任何問題。"""

    user_prompt = user_question or f"請分析這個 campaign 並給優化建議"

    messages = [{"role": "user", "content": user_prompt}]
    trace = []  # 紀錄推理過程

    for step in range(8):  # 最多 8 輪
        response = client.messages.create(
            model="claude-sonnet-4-5",
            max_tokens=2048,
            system=system_prompt,
            tools=TOOLS,
            messages=messages,
        )

        # 紀錄這一步
        step_log = {"step": step + 1, "thoughts": [], "tool_calls": []}

        for block in response.content:
            if block.type == "text":
                step_log["thoughts"].append(block.text)
            elif block.type == "tool_use":
                step_log["tool_calls"].append({"name": block.name, "input": block.input})

        trace.append(step_log)

        # 結束條件
        if response.stop_reason == "end_turn":
            return {
                "final_answer": "\n".join(step_log["thoughts"]),
                "trace": trace,
            }

        # 執行工具
        tool_results = []
        for block in response.content:
            if block.type == "tool_use":
                result = await execute_tool(
                    block.name,
                    block.input,
                    target_acos=target_acos,
                    tenant_id=tenant_id,
                    campaign_id=campaign_id,
                    db=db,
                )
                tool_results.append({
                    "type": "tool_result",
                    "tool_use_id": block.id,
                    "content": result,
                })

        # 把這輪對話塞回去,繼續下一輪
        messages.append({"role": "assistant", "content": response.content})
        messages.append({"role": "user", "content": tool_results})

    return {
        "final_answer": "(達到最大步數,未完成)",
        "trace": trace,
    }
