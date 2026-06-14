#include "Ventilator.h"
#include "SocketOptions.h"
#include <iostream>
#include <chrono>
#include <thread>

namespace Messaging {

    Ventilator::Ventilator(zmq::context_t& ctx, const PipelineConfig& config)
        : puller_(ctx, zmq::socket_type::pull),
          pusher_(ctx, zmq::socket_type::push) {
        // Puller config (inbound batches)
        puller_.set(zmq::sockopt::rcvhwm, SocketOptions::rcv_hwm);
        puller_.set(zmq::sockopt::rcvtimeo, SocketOptions::rcv_timeout_ms);
        puller_.set(zmq::sockopt::rcvbuf, SocketOptions::rcv_buf_bytes);
        puller_.connect(config.pullAddr);

        // Pusher config (outbound results)
        pusher_.set(zmq::sockopt::sndhwm, 2);
        pusher_.set(zmq::sockopt::sndbuf, 64 * 1024 * 512);
        pusher_.connect(config.pushAddr);
    }

    Ventilator::~Ventilator() {
        try { puller_.close(); } catch (...) {}
        try { pusher_.close(); } catch (...) {}
    }

    bool Ventilator::Receive(zmq::message_t& out) {
        auto res = puller_.recv(out, zmq::recv_flags::none);
        return static_cast<bool>(res);
    }

    bool Ventilator::Send(const std::string& payload) {
        zmq::message_t msg(payload.data(), payload.size());

        // 2. Perform a blocking send
        // By using zmq::send_flags::none, we tell ZeroMQ to block the thread
        // if the High Water Mark (HWM) is reached. The OS handles the 
        // suspension, meaning zero CPU usage while waiting.
        auto result = pusher_.send(msg, zmq::send_flags::none);

        // 3. Return true if the send was successful
        return result.has_value();
    }
}
