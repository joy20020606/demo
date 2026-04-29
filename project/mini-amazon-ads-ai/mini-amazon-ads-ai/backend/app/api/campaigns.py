from fastapi import APIRouter, Depends, HTTPException, Query
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from pydantic import BaseModel
import uuid

from app.core.database import get_db
from app.core.deps import get_current_user
from app.core.clickhouse import get_clickhouse_client
from app.models import Campaign

router = APIRouter(prefix="/api/campaigns", tags=["campaigns"])


class CampaignWithMetrics(BaseModel):
    id: uuid.UUID
    name: str
    type: str
    state: str
    daily_budget: float
    target_acos: float
    impressions_7d: int = 0
    clicks_7d: int = 0
    spend_7d: float = 0.0
    sales_7d: float = 0.0
    acos_7d: float = 0.0


@router.get("", response_model=list[CampaignWithMetrics])
async def list_campaigns(
    current=Depends(get_current_user),
    db: AsyncSession = Depends(get_db),
):
    """列出當前 tenant 的所有 campaign + 過去 7 天指標"""
    tenant_uuid = uuid.UUID(current["tenant_id"])

    result = await db.execute(
        select(Campaign).where(Campaign.tenant_id == tenant_uuid)
    )
    campaigns = result.scalars().all()

    if not campaigns:
        return []

    ch = get_clickhouse_client()
    output = []

    for c in campaigns:
        try:
            rows = ch.query(f"""
                SELECT
                    sum(impressions),
                    sum(clicks),
                    sum(spend),
                    sum(sales),
                    if(sum(sales) > 0, sum(spend) / sum(sales) * 100, 0)
                FROM ad_metrics_hourly
                WHERE campaign_id = '{c.id}'
                  AND hour >= now() - INTERVAL 7 DAY
            """).result_rows

            if rows and rows[0][0] is not None:
                imp, clk, spd, sls, acos = rows[0]
            else:
                imp, clk, spd, sls, acos = 0, 0, 0, 0, 0
        except Exception:
            imp, clk, spd, sls, acos = 0, 0, 0, 0, 0

        output.append(CampaignWithMetrics(
            id=c.id,
            name=c.name,
            type=c.type,
            state=c.state,
            daily_budget=float(c.daily_budget),
            target_acos=float(c.target_acos),
            impressions_7d=int(imp or 0),
            clicks_7d=int(clk or 0),
            spend_7d=float(spd or 0),
            sales_7d=float(sls or 0),
            acos_7d=float(acos or 0),
        ))

    return output


@router.get("/{campaign_id}")
async def get_campaign(
    campaign_id: uuid.UUID,
    current=Depends(get_current_user),
    db: AsyncSession = Depends(get_db),
):
    result = await db.execute(
        select(Campaign).where(
            Campaign.id == campaign_id,
            Campaign.tenant_id == uuid.UUID(current["tenant_id"]),
        )
    )
    campaign = result.scalar_one_or_none()
    if not campaign:
        raise HTTPException(404, "Campaign not found")
    return {
        "id": str(campaign.id),
        "name": campaign.name,
        "type": campaign.type,
        "state": campaign.state,
        "daily_budget": float(campaign.daily_budget),
        "target_acos": float(campaign.target_acos),
    }


@router.get("/{campaign_id}/metrics")
async def get_metrics(
    campaign_id: uuid.UUID,
    days: int = Query(30, ge=1, le=90),
    current=Depends(get_current_user),
    db: AsyncSession = Depends(get_db),
):
    result = await db.execute(
        select(Campaign).where(
            Campaign.id == campaign_id,
            Campaign.tenant_id == uuid.UUID(current["tenant_id"]),
        )
    )
    if not result.scalar_one_or_none():
        raise HTTPException(404, "Campaign not found")

    ch = get_clickhouse_client()
    rows = ch.query(f"""
        SELECT
            toDate(hour) AS day,
            sum(impressions),
            sum(clicks),
            sum(spend),
            sum(sales),
            if(sum(sales) > 0, sum(spend) / sum(sales) * 100, 0)
        FROM ad_metrics_hourly
        WHERE campaign_id = '{campaign_id}'
          AND hour >= now() - INTERVAL {days} DAY
        GROUP BY day
        ORDER BY day
    """).result_rows

    return [
        {
            "date": str(r[0]),
            "impressions": int(r[1] or 0),
            "clicks": int(r[2] or 0),
            "spend": float(r[3] or 0),
            "sales": float(r[4] or 0),
            "acos": float(r[5] or 0),
        }
        for r in rows
    ]