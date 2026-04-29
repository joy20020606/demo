from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from pydantic import BaseModel
import uuid

from app.core.database import get_db
from app.core.deps import get_current_user
from app.models import Campaign, Recommendation
from app.services.ai_agent import run_ads_agent

router = APIRouter(prefix="/api/ai", tags=["ai"])


class AnalyzeRequest(BaseModel):
    campaign_id: uuid.UUID
    question: str | None = None


@router.post("/analyze")
async def analyze_campaign(
    req: AnalyzeRequest,
    current=Depends(get_current_user),
    db: AsyncSession = Depends(get_db),
):
    """讓 Claude Agent 分析指定 campaign,回傳推理過程 + 建議"""
    # 確認 campaign 屬於當前 tenant
    result = await db.execute(
        select(Campaign).where(
            Campaign.id == req.campaign_id,
            Campaign.tenant_id == uuid.UUID(current["tenant_id"]),
        )
    )
    campaign = result.scalar_one_or_none()
    if not campaign:
        raise HTTPException(404, "Campaign not found")

    # 執行 Agent loop
    result = await run_ads_agent(
        campaign_id=str(campaign.id),
        campaign_name=campaign.name,
        target_acos=float(campaign.target_acos),
        user_question=req.question,
        tenant_id=current["tenant_id"],
        db=db,
    )

    return result


@router.get("/recommendations")
async def list_recommendations(
    current=Depends(get_current_user),
    db: AsyncSession = Depends(get_db),
):
    """列出當前 tenant 的所有 AI 建議"""
    result = await db.execute(
        select(Recommendation)
        .where(Recommendation.tenant_id == uuid.UUID(current["tenant_id"]))
        .order_by(Recommendation.created_at.desc())
    )
    recs = result.scalars().all()
    return [
        {
            "id": str(r.id),
            "campaign_id": str(r.campaign_id),
            "type": r.type,
            "reasoning": r.reasoning,
            "suggested_action": r.suggested_action,
            "status": r.status,
            "created_at": r.created_at.isoformat(),
        }
        for r in recs
    ]
