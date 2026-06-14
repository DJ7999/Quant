#pragma once
#include <string>

namespace Messaging {
    class PipelineConfig {
    public:
        std::string pullAddr;
        std::string pushAddr;
        std::string reqAddr; // address for REQ/REP (this service will bind REP)

        // Keep existing two-arg initializer for backward compatibility.
        void Initialize(const std::string& pull, const std::string& push, const std::string& req = "") {
            pullAddr = pull;
            pushAddr = push;
            reqAddr = req;
        }
    };
}
