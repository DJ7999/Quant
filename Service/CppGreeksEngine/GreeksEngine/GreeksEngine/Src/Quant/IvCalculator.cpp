#define _USE_MATH_DEFINES 
#include <algorithm>
#include <cmath> 
#include "IvCalculator.h"
#include "BsCalculator.h"

namespace Quant {
	double IvCalculator::CalculateImpliedVolatility(double marketPrice, double underlyingPrice, double strikePrice, double timeToMaturity, double riskFreeRate, bool isCall) {
		// 1. Calculate the Present Value of the Strike
		// We use exp(-r * T) to bring the future strike price to today's dollars
		double discountedK = strikePrice * std::exp(-riskFreeRate * timeToMaturity);
		double intrinsicValue = (std::max)(0.0, isCall ? (underlyingPrice - discountedK) : (discountedK - underlyingPrice));

		if (marketPrice <= intrinsicValue + 1e-9) {
			return 0.0;
		}

		// 2. Setup Solver Bounds (For Bisection Fallback)
		double low = 1e-5;
		double high = 5.0; // 500% cap
		double sigma = std::sqrt(2.0 * M_PI / timeToMaturity) * (marketPrice / underlyingPrice);

		// 3. Hybrid Loop: Halley's Method + Bisection
		for (int i = 0;i < 15;++i) {
			double CalculatedPrice = BsCalculator::CalculateOptionPrice(underlyingPrice, strikePrice, timeToMaturity, riskFreeRate, sigma, isCall);
			double diff = CalculatedPrice - marketPrice;
			// Precision check
			if (std::abs(diff) < 1e-8) return sigma;

			// Update Bisection Brackets
			if (diff > 0) {
				high = sigma;
			}
			else {
				low = sigma;
			}

			double vega = BsCalculator::CalculateVega(underlyingPrice, strikePrice, timeToMaturity, riskFreeRate, sigma);
			if (vega < 1e-10) {
				sigma = (low + high) / 2.0; // Fallback to bisection if Vega is too small
				continue;
			}

			double vomma = BsCalculator::CalculateVomma(underlyingPrice, strikePrice, timeToMaturity, riskFreeRate, sigma);

			// Halley's Update (Householder Order 2)
			double step = diff / vega;
			double adj = 1.0 - step * (vomma / (2.0 * vega));
			double nextSigma;
			if (std::abs(adj) < 0.2) {
				nextSigma = sigma - step; // fallback to Newton-Raphson if adjustment is too small
			}
			else {
				nextSigma = sigma - step / adj; // Halley's method
			}

			if (nextSigma <= low || nextSigma >= high) {
				sigma = (low + high) / 2.0; // Bisection fallback
			}
			else {
				sigma = nextSigma;
			}
		}
		return sigma;
	}
}