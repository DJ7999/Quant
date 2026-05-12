#pragma once
namespace Quant
{
	class QlibBenchmarkBsCalculator
	{
	public:
		static double CalculateOptionPrice(double S, double K, double T, double r, double sigma, bool isCall);

	public:
		static double CalculateVega(double S, double K, double T, double r, double sigma);

	public:
		static double CalculateVomma(double S, double K, double T, double r, double sigma);

	public:
		static double CalculateDelta(double S, double K, double T, double r, double sigma, bool isCall);

	public:
		static double CalculateGamma(double S, double K, double T, double r, double sigma);

	public:
		static double CalculateTheta(double S, double K, double T, double r, double sigma, bool isCall);

	public:
		static double CalculateRho(double S, double K, double T, double r, double sigma, bool isCall);
	};
}