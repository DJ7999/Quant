#pragma once
#include "PipelineManager.h"
#include <atomic>

namespace Messaging {
    // REQ/REP server for single-request synchronous calls (binds REP).
    class ReqRepServer {
    public:
        static void Run(const PipelineConfig& config, std::atomic<bool>& running);
    };
}
