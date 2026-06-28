#include "ReqRepServer.h"
#include "SocketOptions.h"
#include "../Quant/GreeksUtils.h"
#include <zmq.hpp>
#include <iostream>

namespace Messaging {

    void ReqRepServer::Run(const PipelineConfig& config, std::atomic<bool>& running) {
        zmq::context_t context(1);
        zmq::socket_t rep(context, zmq::socket_type::rep);

        try {
            rep.bind(config.reqAddr);
        }
        catch (const zmq::error_t& e) {
            std::cerr << "[ReqRepServer] Failed to bind to " << config.reqAddr << ": " << e.what() << std::endl;
            return;
        }

        rep.set(zmq::sockopt::rcvtimeo, SocketOptions::reqrep_rcv_timeout_ms); // poll every second to check 'running'
        rep.set(zmq::sockopt::rcvhwm, SocketOptions::reqrep_rcv_hwm);
        rep.set(zmq::sockopt::linger, SocketOptions::reqrep_linger_ms);

        while (running) {
            zmq::message_t reqMsg;
            if (!rep.recv(reqMsg, zmq::recv_flags::none)) {
                continue; // timeout -> loop to check running
            }

            bh_dream::OptionRequestSnapshotProto requestProto;
            if (!requestProto.ParseFromArray(reqMsg.data(), static_cast<int>(reqMsg.size()))) {
                std::cerr << "[ReqRepServer] Failed to parse request proto." << std::endl;
                zmq::message_t empty;
                rep.send(empty, zmq::send_flags::none);
                continue;
            }

            bh_dream::OptionGreeksResultSnapshotProto replyProto;
            Quant::GreeksUtils::ComputeGreeks(requestProto, &replyProto);

            std::string out;
            if (!replyProto.SerializeToString(&out)) {
                std::cerr << "[ReqRepServer] Failed to serialize reply proto." << std::endl;
                zmq::message_t empty;
                rep.send(empty, zmq::send_flags::none);
                continue;
            }

            zmq::message_t reply(out.data(), out.size());
            rep.send(reply, zmq::send_flags::none);
        }

        try { rep.close(); } catch (...) {}
    }
}
