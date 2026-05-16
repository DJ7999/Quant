#include <iostream>
#include <csignal>
#include <atomic>
#include <thread>
#include <vector>
#include <Messaging/PipelineManager.h>
#include <Apps/GreeksWorker.h>
#include "Quant/IvCalculator.h"
#include <iomanip>

// We use std::atomic to ensure thread-safety across the app
std::atomic<bool> globalRunning{ true };

// This function is called by the OS when you press Ctrl+C
void handleManualStop(int signal) {
    std::cout << "\n[System] Manual stop signal received (" << signal << ")." << std::endl;
    std::cout << "[System] Shutting down workers gracefully..." << std::endl;
    globalRunning = false;
}

int main() {
    // 1. Register the signal handler for SIGINT (Ctrl+C)
    std::signal(SIGINT, handleManualStop);

    Messaging::PipelineConfig pipeline;
   

    try {
        // Connect to your C# endpoints
        pipeline.Initialize("tcp://127.0.0.1:5555", "tcp://127.0.0.1:5556");
        std::cout << "[System] Pipeline initialized. Backpressure (HWM) active." << std::endl;
    }
    catch (const std::exception& e) {
        std::cerr << "Failed to init: " << e.what() << std::endl;
        return -1;
    }

    // 2. Spawn Worker Pool
    unsigned int cores = std::thread::hardware_concurrency()/2;
    std::vector<std::thread> pool;

    for (unsigned int i = 0; i < cores; ++i) {
        // We pass 'globalRunning' by reference so workers see the signal change
        pool.emplace_back(Apps::GreeksWorker::Execute, (int)(i+1), pipeline, std::ref(globalRunning));
    }

    std::cout << "[System] Running with " << cores << " workers. Press Ctrl+C to stop." << std::endl;

    // 3. The "Infinite" Loop
    // The main thread just sits here doing nothing until you trigger the signal
    while (globalRunning) {
        std::this_thread::sleep_for(std::chrono::seconds(1));
    }

    // 4. Cleanup
    for (auto& t : pool) {
        if (t.joinable()) t.join();
    }

    std::cout << "[System] All workers stopped. Engine offline." << std::endl;
    return 0;
}