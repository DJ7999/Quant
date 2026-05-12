#pragma once
#include <Messaging/PipelineManager.h>
#include <atomic> // Must include this

namespace Apps {
    class GreeksWorker {
    public:
        static void Execute(int workerId, Messaging::PipelineConfig config, std::atomic<bool>& running);
    };
}