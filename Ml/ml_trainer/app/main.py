import zmq
import json
import time

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

if __name__ == "__main__":
    start_pull_server()