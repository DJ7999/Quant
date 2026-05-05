#pragma once

namespace Quant {
	class IvCalculator {
	public:
		/**
 * @brief Calculates the Implied Volatility (IV) using the Householder (Halley) method.
 *
 * This solver uses first and second-order derivatives (Vega and Vomma) to find the
 * volatility that matches the given market price. It is more robust than Newton-Raphson
 * for deep ITM/OTM options.
 *
 * @note This implementation follows the high-order iteration logic popularized by
 * Peter Jäckel. While it uses the Householder "engine," it is a streamlined version
 * that bypasses the full rational cubic spline initialization in favor of a
 * stabilized ATM approximation.
 *
 * @param marketPrice The current market price of the option.
 * @param underlyingPrice The current spot price of the asset (S).
 * @param strikePrice The exercise price of the option (K).
 * @param timeToMaturity Time to expiration in years (T).
 * @param riskFreeRate Annualized risk-free interest rate (r).
 * @param isCall True for Call, False for Put.
 *
 * @return The Calculated implied volatility (e.g., 0.25 for 25%).
 *         Returns 0.0 if marketPrice is below intrinsic value.
 *         Returns 5.0 as a maximum ceiling.
 */
		static double CalculateImpliedVolatility(
			double marketPrice, 
			double underlyingPrice, 
			double strikePrice, 
			double timeToMaturity, 
			double riskFreeRate, 
			bool isCall
		);
	};
}