#define _USE_MATH_DEFINES
#include <cmath>
#include <algorithm>
#include "BsCalculator.h"

namespace Quant {
	double BsCalculator::NormalCDF(double x) {
		return 0.5 * erfc(-x * sqrt(0.5));
	}
	
	double BsCalculator::NormalPDF(double x) {
		return std::exp(-0.5 * x * x) / std::sqrt(2.0 * M_PI);
	}

	double BsCalculator::CalculateD1(double S, double K, double T, double r, double sigma) {
		return (log(S / K) + (r + 0.5 * sigma * sigma) * T) / (sigma * sqrt(T));
	}

	double BsCalculator::CalculateD2(double d1, double sigma, double T) {
		return d1 - sigma * std::sqrt(T);
	}

	double BsCalculator::CalculateOptionPrice(double S, double K, double T, double r, double sigma, bool isCall) {
		if (T <= 0) return (std::max)(0.0, isCall ? (S - K) : (K - S));
		double d1 = CalculateD1(S, K, T, r, sigma);
		double d2 = CalculateD2(d1, sigma, T);
		if (isCall) {
			return S * NormalCDF(d1) - K * std::exp(-r * T) * NormalCDF(d2);
		}
		else {
			return K * std::exp(-r * T) * NormalCDF(-d2) - S * NormalCDF(-d1);
		}
	}

	double BsCalculator::CalculateVega(double S, double K, double T, double r, double sigma) {
		if (T <= 0) return 0.0;
		double d1 = CalculateD1(S, K, T, r, sigma);
		return S * NormalPDF(d1) * std::sqrt(T);
	}

	double BsCalculator::CalculateVomma(double S, double K, double T, double r, double sigma) {
		if (T <= 0 || sigma <= 0) return 0.0;
		double d1 = CalculateD1(S, K, T, r, sigma);
		double d2 = CalculateD2(d1, sigma, T);
		double vega = CalculateVega(S, K, T, r, sigma);
		return (vega * d1 * d2) / sigma;
	}

	double BsCalculator::CalculateDelta(double S, double K, double T, double r, double sigma, bool isCall) {
		double d1 = CalculateD1(S, K, T, r, sigma);
		double cdfD1 = NormalCDF(d1);
		return isCall ? cdfD1 : (cdfD1 - 1);
	}

	double BsCalculator::CalculateGamma(double S, double K, double T, double r, double sigma) {
		double d1 = CalculateD1(S, K, T, r, sigma);
		double pdfD1 = NormalPDF(d1);
		return pdfD1 / (S * sigma * std::sqrt(T));
	}

	double BsCalculator::CalculateTheta(double S, double K, double T, double r, double sigma, bool isCall) {
		if (T <= 0) return 0.0;
		double d1 = CalculateD1(S, K, T, r, sigma);
		double d2 = CalculateD2(d1, sigma, T);
		double pdfD1 = NormalPDF(d1);
		double firstTerm = -(S * pdfD1 * sigma) / (2 * std::sqrt(T));
		double secondTerm = r * K * std::exp(-r * T) * (isCall ? NormalCDF(d2) : NormalCDF(-d2));
		return (isCall ? (firstTerm - secondTerm) : (firstTerm + secondTerm)) / 365;
	}

	double BsCalculator::CalculateRho(double S, double K, double T, double r, double sigma, bool isCall) {
		double d1 = CalculateD1(S, K, T, r, sigma);
		double d2 = CalculateD2(d1, sigma, T);
		return (isCall ? 1 : -1) * K * T * std::exp(-r * T) * NormalCDF((isCall ? 1 : -1) * d2) / 100;
	}
}