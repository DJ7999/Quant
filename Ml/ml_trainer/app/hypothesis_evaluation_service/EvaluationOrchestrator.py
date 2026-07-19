import logging
from typing import Dict, Any
from Dto.HypothesisRequestDto import HypothesisRequestDto
from persistence.Repository import Repository
from hypothesis_evaluation_service.ModelLabeler import ModelLabeler
from hypothesis_evaluation_service.ConfigHypothesisProcessor import ConfigHypothesisProcessor

logger = logging.getLogger(__name__)

class EvaluationOrchestrator:
    def __init__(self):
        self.labeler = ModelLabeler()
        self.repository = Repository()  # Initialize persistence layer

    def run_hypothesis_assessment(self, request: HypothesisRequestDto) -> Dict[str, Any]:
        """
        Unified Hypothesis Engine entry point.
        Ingests the transport DTO request, triggers data extraction/labeling,
        pre-fetches all option histories, and runs evaluations.
        """
        logger.info(f"Hypothesis Engine running assessment workflow for: {request.ModelGuid}")

        try:
            # Stage 1: Generate labeled regime dataframe matrix
            labeled_df = self.labeler.generate_regime_labels(request.ModelGuid)

            if labeled_df.empty:
                return {
                    "success": False,
                    "error": "Failed to extract historical data sequences or assign cluster regimes."
                }
            logger.info(f"labeled regime DataFrame generated with {len(labeled_df)} rows and {len(labeled_df.columns)} columns. distinct regimes: {labeled_df['ClusterLabel'].nunique()}.")
            # Step 1.1: Fetch historical date boundary markers
            start_date = labeled_df.index.min()
            end_date = labeled_df.index.max()

            # Step 1.2: Eagerly load the option price matrix for this timeframe
            # This completely resolves the "Empty Dict Trap" in the processor loop
            logger.info(f"Eagerly loading price matrix from {start_date} to {end_date}")
            price_matrix = self.repository.get_option_histories(start_date, end_date)
            logger.info(f"Price matrix loaded with {len(price_matrix)} entries.")
            # Stage 2: Instantiate processor with both the DataFrame and pre-loaded price matrix
            hypothesis_processor = ConfigHypothesisProcessor(
                regim_labeled_df=labeled_df,
                price_matrix=price_matrix
            )
            logger.info(f"Hypothesis processor initialized with regime-labeled DataFrame and price matrix.")
            variants_payload = {}
            logger.info(f"Beginning evaluation of {len(request.Strategies)} strategy configurations.")
            # Loop through configurations
            for strategy_config in request.Strategies:
                variant_id = strategy_config.VariantId
                logger.info(f"Evaluating strategy configuration: {variant_id} with {len(strategy_config.Legs)} legs.")
                # Check for duplicates safely BEFORE running evaluations
                if variant_id in labeled_df.columns:
                    logger.warning(f"Duplicate VariantId detected: {variant_id}. Skipping this configuration.")
                    continue
                logger.info(f"Processing strategy configuration: {variant_id}.")   
                # Execute evaluation and unpack the corrected tuple
                returns_series, metrics_result = hypothesis_processor.process_config(
                    strategy_config=strategy_config
                )
                logger.info(f"Evaluation complete for {variant_id}. Metrics: {metrics_result.model_dump()}.")
                # Update our tracking collections
                labeled_df[variant_id] = returns_series
                variants_payload[variant_id] = metrics_result.model_dump() 

            # Stage 3: Return payload back to the primary router
            return {
                "success": True,
                "modelGuid": request.ModelGuid,
                "variants": variants_payload,
                "labeledData": labeled_df.reset_index().to_dict(orient="records")
            }

        except Exception as e:
            logger.error(f"Hypothesis Engine failure while executing assessment: {str(e)}", exc_info=True)
            return {
                "success": False,
                "error": str(e)
            }