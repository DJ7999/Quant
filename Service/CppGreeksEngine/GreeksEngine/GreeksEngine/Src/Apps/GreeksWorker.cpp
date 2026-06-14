#include "GreeksWorker.h"
#include "pricing.pb.h"
#include "../Quant/GreeksUtils.h"
#include "../Quant/BsCalculator.h"
#include "../Quant/IvCalculator.h"
#include <iostream>
#include <zmq.hpp>
#include <atomic>
#include <thread>
#include <chrono>
#include "../Messaging/Ventilator.h"

namespace Apps {

    void GreeksWorker::Execute(int workerId, Messaging::PipelineConfig config, std::atomic<bool>& running) {
        std::cout << "\nworker " << workerId << " online." << std::endl;
        zmq::context_t context(1);

        // Use the Ventilator helper for PUSH/PULL behavior
        Messaging::Ventilator ventilator(context, config);

        int max = 0;
        int min = INT_MAX;
        int avg = 0;

        while (running) {
            zmq::message_t inboundMsg;
            if (!ventilator.Receive(inboundMsg)) {
                continue; // no message (timeout) -> check running again
            }

            bh_dream::OptionBatchRequestProto batchReq;
            if (!batchReq.ParseFromArray(inboundMsg.data(), static_cast<int>(inboundMsg.size()))) {
                std::cerr << "[Worker] Failed to parse batch request." << std::endl;
                continue;
            }

            bh_dream::OptionGreeksBatchResultProto batchRes;
            batchRes.set_batch_id(batchReq.batch_id());

            for (const auto& req : batchReq.option_request_snapshots()) {
                auto start = std::chrono::high_resolution_clock::now();
                auto* res = batchRes.add_option_greeks_result_snapshots();
                res->set_option_history_id(req.option_history_id());
                res->set_contract_id(req.contract_id());
                res->set_rfr_market(req.rfr_market());
                res->set_rfr_tenor(req.rfr_tenor());

                // Reuse shared compute logic
                Quant::GreeksUtils::ComputeGreeks(req, res);

                auto end = std::chrono::high_resolution_clock::now();
                auto duration = static_cast<int>(std::chrono::duration_cast<std::chrono::microseconds>(end - start).count());
                if (avg == 0) avg = duration;
                avg = (avg + duration) / 2;
                if (duration > max) max = duration;
                if (duration < min) min = duration;
            }

            std::cout << "[Thread " << std::this_thread::get_id() << "] Processing in "
                << "Average Time per Option: " << avg << "us\n"
                << "Maximum Time per Option: " << max << "us\n"
                << "Minimum Time per Option: " << min << "us\n";

            std::string output;
            if (batchRes.SerializeToString(&output)) {
                if (!ventilator.Send(output)) {
                    std::cerr << "[Worker] Failed to send batch result after retries." << std::endl;
                }
            }
        }
    }
}
