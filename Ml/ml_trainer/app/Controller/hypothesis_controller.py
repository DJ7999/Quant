import logging

from fastapi import APIRouter
from pydantic import BaseModel

from Dto.HypothesisRequestDto import HypothesisRequestDto
from hypothesis_evaluation_service.EvaluationOrchestrator import EvaluationOrchestrator

# Create a router instead of an app instance
logger = logging.getLogger(__name__)
router = APIRouter()
@router.post("/hypothesis", response_model=dict)
async def hypothesis_executer(payload: HypothesisRequestDto):
    logger.info(f"Received Hypothesis Request for ModelGuid: {payload.ModelGuid} with {len(payload.Strategies)} strategy configurations.")
    orchestrator = EvaluationOrchestrator()
    result = orchestrator.run_hypothesis_assessment(payload)
    return result