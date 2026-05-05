#pragma once

namespace Quant
{
	class BsCalculator
	{
	public:
		static double NormalCDF(double x);

	public:
		static double NormalPDF(double x);

	public:
		static double CalculateD1(double S, double K, double T, double r, double sigma);

	public:
		static double CalculateD2(double d1, double sigma, double T);

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
