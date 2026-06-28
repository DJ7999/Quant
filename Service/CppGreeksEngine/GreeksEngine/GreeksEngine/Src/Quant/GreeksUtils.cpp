#include "GreeksUtils.h"
#include "BsCalculator.h"
#include "IvCalculator.h"

void Quant::GreeksUtils::ComputeGreeks(const bh_dream::OptionRequestSnapshotProto& req,
                                       bh_dream::OptionGreeksResultSnapshotProto* res) {
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
        req.option_price_close(), // marketPrice
        S,                        // underlyingPrice
        K,                        // strikePrice
        T,                        // timeToMaturity
        r,                        // riskFreeRate
        isCall
    );

    // 2. Calculate Primary Greeks
    res->set_delta(Quant::BsCalculator::CalculateDelta(S, K, T, r, sigma, isCall));
    res->set_gamma(Quant::BsCalculator::CalculateGamma(S, K, T, r, sigma));
    res->set_vega(Quant::BsCalculator::CalculateVega(S, K, T, r, sigma));
    res->set_theta(Quant::BsCalculator::CalculateTheta(S, K, T, r, sigma, isCall));
    res->set_rho(Quant::BsCalculator::CalculateRho(S, K, T, r, sigma, isCall));
    res->set_vomma(Quant::BsCalculator::CalculateVomma(S, K, T, r, sigma));
    res->set_implied_volatility(sigma);

    // 3. Initialize Benchmark Results
    auto* bench = res->mutable_benchmark_result();
    bench->set_delta(0.0);
    bench->set_gamma(0.0);
    bench->set_vega(0.0);
    bench->set_theta(0.0);
    bench->set_rho(0.0);
    bench->set_vomma(0.0);
    bench->set_implied_volatility(0.0);
}
