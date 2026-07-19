from fastapi import FastAPI
import zmq
import json
import time
import logging
from MlWorker import MlWorker
from Controller.hypothesis_controller import router as hypothesis_router

def setup_logging():
    """Configures global logging layout and levels."""
    logging.basicConfig(
        level=logging.INFO,  # Change to logging.DEBUG to see finer logs
        format="%(asctime)s [%(levelname)s] (%(name)s) %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S",
    )

def start_pull_server():
    context = zmq.Context()
    # PULL = Consumes incoming tasks from the pipeline
    socket = context.socket(zmq.PULL)
    socket.bind("tcp://*:5557c")
    
    print("🧠 ML-Lab ZeroMQ PULL Server is up and listening on port 5555...")
    print("Waiting for .NET pipeline submissions...\n")
    
    while True:
        # This blocks natively until a task arrives
        raw_message = socket.recv_string()
        
        try:
            payload = json.loads(raw_message)
            print(f"📥 [PULL MATCH] Received Training Trigger Request:")
            print(f"   • Model Architecture : {payload.get('modelName')}")
            print(f"   • Total Window Bounds: {payload.get('totalWindowStart')} to {payload.get('totalWindowEnd')}")
            print(f"   • Rolling Sub-Window : {payload.get('trainingWindowSizeMonths')} Months")
            print(f"   • Frequency Step     : {payload.get('retrainFrequency')}")
            print(f"   • Pipeline Features  : {len(payload.get('featuresPipeline', []))} custom blocks configured")
            
            # Simulate processing the ML training workload
            print("   ⚙️ Slicing timeline segments and initializing training...")
            time.sleep(2)  # Simulating heavy processing
            print("   ✅ Training execution run completed.\n")
            
        except json.JSONDecodeError:
            print(f"⚠️ Received plain-text frame payload (Unstructured JSON): {raw_message}\n")
worker = None

async def lifespan(app: FastAPI):
    global worker
    setup_logging()
    main_logger = logging.getLogger("fastapi_app")
    main_logger.info("Initilizing worker for ML training orchestration...")

    worker = MlWorker(check_interval=5)
    worker.start()

    main_logger.info("Ready for other extension tasks!")

    try:
        yield  # This allows the app to run while the worker is active
    finally:
        main_logger.warning("Shutdown signal received.")
        if worker:
            worker.stop()
        main_logger.info("Clean shutdown complete.")

app = FastAPI(lifespan=lifespan)

app.include_router(hypothesis_router, prefix="/api/v1/hypothesis", tags=["Hypothesis Engine"])