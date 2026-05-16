#include "GreeksWorker.h"
#include "pricing.pb.h"
#include "../Quant/BsCalculator.h" 
#include "../Quant/IvCalculator.h"
#include <iostream>
#include <zmq.hpp>      // CRITICAL: This fixes the 'zmq' errors
#include <atomic>       // Required for the 'running' variable
#include <thread> // Fixes std::this_thread
#include <chrono> // Fixes std::chrono

namespace Apps {

    // Helper to compute individual greeks and populate the benchmark sub-message
    static void ComputeGreeks(const bh_dream::OptionRequestSnapshotProto& req, bh_dream::OptionGreeksResultSnapshotProto* res) {
        // Map Identifiers
        res->set_option_history_id(req.option_history_id());
        res->set_contract_id(req.contract_id());

        // Extract parameters
        double S = req.underlying_value();
        double K = req.strike_price();
        double T = req.time_to_expiry();
        double r = req.risk_free_rate();
        bool isCall = req.is_call();

        // 1. Calculate Implied Volatility
        double sigma = Quant::IvCalculator::CalculateImpliedVolatility(
            req.option_price_close(), // 1. marketPrice
            S,                        // 2. underlyingPrice
            K,                        // 3. strikePrice
            T,                        // 4. timeToMaturity
            r,                        // 5. riskFreeRate
            isCall                    // 6. isCall
        );

        // 2. Calculate Primary Greeks
        res->set_delta(Quant::BsCalculator::CalculateDelta(S, K, T, r, sigma, isCall));
        res->set_gamma(Quant::BsCalculator::CalculateGamma(S, K, T, r, sigma));
        res->set_vega(Quant::BsCalculator::CalculateVega(S, K, T, r, sigma));
        res->set_theta(Quant::BsCalculator::CalculateTheta(S, K, T, r, sigma, isCall));
        res->set_rho(Quant::BsCalculator::CalculateRho(S, K, T, r, sigma, isCall));
        res->set_vomma(Quant::BsCalculator::CalculateVomma(S, K, T, r, sigma));
        res->set_implied_volatility(sigma);

        // 3. Initialize Benchmark Results (mutable_ handles allocation)
        auto* bench = res->mutable_benchmark_result();
        bench->set_delta(0.0);
        bench->set_gamma(0.0);
        bench->set_vega(0.0);
        bench->set_theta(0.0);
        bench->set_rho(0.0);
        bench->set_vomma(0.0);
        bench->set_implied_volatility(0.0);
    }

    void GreeksWorker::Execute(int workerId, Messaging::PipelineConfig config, std::atomic<bool>& running) {
        std::cout << "\nworker "<<workerId<<"onlline." << std::endl;
        zmq::context_t context(1);

        zmq::socket_t puller(context, zmq::socket_type::pull);
        puller.set(zmq::sockopt::rcvhwm, 2);
        puller.set(zmq::sockopt::rcvtimeo, 5000);
		puller.set(zmq::sockopt::rcvbuf, 64 * 1024 * 512);
        puller.connect(config.pullAddr);

        zmq::socket_t pusher(context, zmq::socket_type::push);
        pusher.set(zmq::sockopt::sndhwm, 2);
		pusher.set(zmq::sockopt::sndbuf, 64 * 1024 * 512);

        pusher.connect(config.pushAddr);
		int max = 0;
        int min = INFINITY;
		int avg = 0;
        while (running) {
            zmq::message_t inboundMsg;

            // Receive directly—no mutex needed!
            auto recvResult = puller.recv(inboundMsg, zmq::recv_flags::none);
            
            if (recvResult) {
                std::cout << "result actually received" << std::endl;
                bh_dream::OptionBatchRequestProto batchReq;
                if (batchReq.ParseFromArray(inboundMsg.data(), (int)inboundMsg.size())) {

                    bh_dream::OptionGreeksBatchResultProto batchRes;
                    batchRes.set_batch_id(batchReq.batch_id());

                    

                    for (const auto& req : batchReq.option_request_snapshots()) {
                        auto start = std::chrono::high_resolution_clock::now();
                        auto* res = batchRes.add_option_greeks_result_snapshots();
                        res->set_option_history_id(req.option_history_id());
                        res->set_contract_id(req.contract_id());
                        res->set_rfr_market(req.rfr_market());
						res->set_rfr_tenor(req.rfr_tenor());
                        ComputeGreeks(req, res);
                        auto end = std::chrono::high_resolution_clock::now();
                        auto duration = std::chrono::duration_cast<std::chrono::microseconds>(end - start).count();
						if (avg == 0) avg = duration;
						avg = (avg + duration) / 2;
						if (duration > max) max = duration;
						if (duration < min) min = duration;
                    }

                    

                    std::cout << "[Thread " << std::this_thread::get_id() << "] Processing in "
						<< "Average Time per Option: " << avg << "us\n"
						<< "Maximum Time per Option: " << max<< "us\n"
                        << "Minimum Time per Option: " << min<< "us\n";
                    // Serialize and Push
                    std::string output;
                    if (batchRes.SerializeToString(&output)) {
                        zmq::message_t outboundMsg(output.data(), output.size());

                        bool sent = false;
                        while (!sent && running) {
                            // Send directly—no mutex needed!
                            if (pusher.send(outboundMsg, zmq::send_flags::dontwait)) {
                                sent = true;
                            }
                            else {
                                std::this_thread::sleep_for(std::chrono::milliseconds(1));
                            }
                        }
                    }
                }
            }
        }
    }
}