#pragma once  // <--- Add this line at the very top!
#include <string>

namespace Messaging {
    class PipelineConfig {
    public:
        std::string pullAddr;
        std::string pushAddr;

        void Initialize(const std::string& pull, const std::string& push) {
            pullAddr = pull;
            pushAddr = push;
        }
    };
}