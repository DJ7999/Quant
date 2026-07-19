import logging
import threading 
import time
import datetime
from persistence import MlModels, get_db_session
from CommonHelper import CommonHelper
from MlOrchastrator import MlOrchastrator
from StrategyType import StrategyType

logger = logging.getLogger(__name__)

class MlWorker:
    def __init__(self, check_interval: int = 5):
        self.check_interval = check_interval
        self._is_running = False
        self._thread = None
        self._stop_event = threading.Event()
        self.common_helper = CommonHelper()
    
    def start(self):
        if self._is_running:
            logger.warning("Worker is already running.")
            return
        self._is_running = True
        self._stop_event.clear()
        self._thread = threading.Thread(target=self._run_loop, daemon=True)
        self._thread.start()
        logger.info("Background thread successfully initialized.")

    def stop(self):
        if self._thread:
            logger.info("Stopping worker thread...")
            self._is_running = False
            self._stop_event.set()  
            self._thread.join(timeout=2)
            logger.info("Worker thread stopped.")

    def _run_loop(self):
        """The core loop monitoring the Postgres table."""
        logger.info("Active and monitoring ml jobs...")
        while self._is_running:
            try:
                logger.info("Trying to fetch task")
                task = self._fetch_and_reserve_task()
                
                if task:
                    logger.info(f"Task found starting processing {task.id if hasattr(task, 'id') else task}")
                    self._process_task(task)
                else:
                    logger.info("Task not found")
                    if self._stop_event.wait(timeout=self.check_interval):
                        break
            except Exception as e:
                logger.error(f"Error in background worker loop: {str(e)}", exc_info=True)
                time.sleep(5)

    def _fetch_and_reserve_task(self):
        """Queries Postgres for oldest queued job, locks it, and marks it as Processing (Status=1)."""
        with get_db_session() as session:
            # Row-level locking ensures multi-worker safety
            task = (
                session.query(MlModels)
                .filter(MlModels.Status == 0)
                .order_by(MlModels.StartDateTime.asc())
                .with_for_update(skip_locked=True)
                .first()
            )
            
            if task:
                task.Status = 1
                task.LastUpdatedAt = datetime.datetime.now(datetime.timezone.utc)
                session.commit()
                
                # Expire_on_commit configuration or refreshing ensures attributes 
                # don't trigger DetachedInstanceError out of session context.
                session.refresh(task)
                session.expunge(task) # Safely unbind from session lifecycle so it holds data read-only
                return task
            
            return None
            
    def _process_task(self, payload: MlModels):
        """Executes the orchestrator heavy lifting."""
        logger.info(f"Starting task processing execution for: {payload}")
        strategy = StrategyType("KMeansClustering")
        orchastrator = MlOrchastrator(strategy=strategy, common_helper=self.common_helper)
        
        try:
            file_path, metrics = orchastrator.orchastrate_training(model=payload)
    
            # Example: Assign them back to the database columns before saving!
            payload.ModelReference = file_path
            payload.ModelMetrics = metrics  # Correct column name is ModelMetrics!
            self._update_task_status(payload, 2)  # 2 = Completed
            logger.info("Task completed successfully.")
        except Exception as e:
            logger.error(f"Error occurred while training model: {str(e)}")
            payload.FailureReason = str(e)
            self._update_task_status(payload, 3)  # 3 = Failed (Aligned comment with execution code)
        
        time.sleep(4)  

    def _update_task_status(self, payload: MlModels, status: int):
        with get_db_session() as session:
            # Merge brings the detached payload back safely into a fresh transaction context
            task = session.merge(payload)
            task.Status = status
            task.LastUpdatedAt = datetime.datetime.now(datetime.timezone.utc)
            session.commit()
            session.refresh(task)