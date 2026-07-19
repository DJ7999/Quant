# Quantitative Options Trading Research System

A production-grade multi-language quantitative trading system built on real NSE options data.

## Tech Stack
.NET (C#) | C++ | Python | PostgreSQL | ZeroMQ | React

## Phases
- ✅ Phase 1: Options Greeks & IV Calculator (C++ engine)
- ✅ Phase 2: Regime Detection ML Pipeline (K-Means, HMM)  
- ✅ Phase 3: Hypothesis Testing Framework
- 🔮 Phase 4: Walk-Forward Backtesting (Planned)
- 🔮 Phase 5: Paper Trading (Planned)

## Key Technical Highlights
- C++ calculation engine processing records with 4 parallel workers
- ~100 thousand NSE historical options records across 3 years
- Async job queue pattern for ML training (no blocking)
- Statistical validation via t-test (p-value) per regime per strategy

## Architecture
![System Architecture](Info/HLD%20Architectuure.drawio.svg)

---

## 🚀 Application Ecosystem: How the System Works

This platform is a modular, offline quantitative research suite designed to ingest options histories, calculate historical derivatives metrics, train regime models, and validate trading hypotheses statistically:

### 1. Ingestion Engine — Historical Option Data Processor
*   **Purpose**: Processes and structures historical NSE option tick and daily data.
*   **How it Works**: Parses raw historical exchange files through the C# .NET processor, maps contract details, and saves them in PostgreSQL to build the historical options database.

### 2. Yield Curve Ingestion — Risk-Free Rate (RFR) Importer
*   **Purpose**: Ingests historical Interest Rate (Risk-Free Rate) yield curves from CSV files.
*   **How it Works**: Loads historical interest rate curves into the database. These rates serve as a direct parameter input for the Black-Scholes pricing model, allowing the calculation engine to discount strike values correctly when computing historical Greeks and Implied Volatilities.

### 3. Analytics Terminal — Option Basket Visualizer
*   **Purpose**: Historical visualization of calculated Greeks and Implied Volatilities.
*   **How it Works**: Allows users to define custom options portfolios (baskets of call and put options) and view their historical calculated Greeks (Delta, Gamma, Vega, Theta) and IV metrics over past timeframes to analyze theoretical position behavior.

### 4. ML Workspace — Custom Model Trainer
*   **Purpose**: Configuration and training of market regime classifiers.
*   **How it Works**: Quantitative researchers select features (such as rolling price standard deviation, implied volatility metrics, and log returns) to train KMeans clustering models. The trained classifiers are serialized (`.joblib`) to label historical dates with regime states.

### 5. Research Terminal — Hypothesis Backtester
*   **Purpose**: Statistical testing of multi-leg option strategies under predicted market regimes.
*   **How it Works**: Simulates option strategy lifecycles across historical regime-shift windows. It calculates returns relative to a CBOE-standard margin capital base to prevent return spikes on cheap/short contracts, averages overlapping daily positions horizontally, and runs t-tests (T-stat/P-value) to verify whether the strategy's average returns are statistically different from random noise.