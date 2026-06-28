#pragma once
#include "PipelineManager.h"
#include <zmq.hpp>
#include <string>

namespace Messaging {
    // Encapsulates PUSH/PULL sockets used by the worker pool (ventilator pattern).
    class Ventilator {
    public:
        Ventilator(zmq::context_t& ctx, const PipelineConfig& config);
        ~Ventilator();

        // Receives next message into 'out'. Returns true on success.
        bool Receive(zmq::message_t& out);

        // Sends a string payload through the push socket. Returns true on success.
        bool Send(const std::string& payload);

    private:
        zmq::socket_t puller_;
        zmq::socket_t pusher_;
    };
}
