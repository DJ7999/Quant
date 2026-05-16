import React, { useEffect, useState } from 'react';
import { api } from '../../../core/api';
import { 
  LineChart, 
  Line, 
  XAxis, 
  YAxis, 
  Tooltip, 
  Legend, 
  ResponsiveContainer, 
  CartesianGrid 
} from 'recharts';
const GREEK_METRICS = [
  { key: 'delta', label: 'Delta (Δ)' },
  { key: 'gamma', label: 'Gamma (Γ)' },
  { key: 'theta', label: 'Theta (θ)' },
  { key: 'vega', label: 'Vega (ν)' },
  { key: 'iv', label: 'Implied Volatility (IV)' }
];
const Visualize = ({ legs, onBack }) => {
  const [chartData, setChartData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Greeks Visualization States
  const [showGreeks, setShowGreeks] = useState(false);
  const [tenors, setTenors] = useState([]);
  const [selectedTenor, setSelectedTenor] = useState('');
  const [selectedGreek, setSelectedGreek] = useState('delta');
  const [loadingTenors, setLoadingTenors] = useState(false);
  
  // Isolated Data State for Greeks Timeline charting
  const [greeksChartData, setGreeksChartData] = useState([]);
  const [loadingGreeks, setLoadingGreeks] = useState(false);

  // Handle Fetching Tenors when "Visualize Greeks" is triggered
  const handleVisualizeGreeksClick = async () => {
    setShowGreeks(true);
    if (tenors.length > 0) return; 

    setLoadingTenors(true);
    try {
      const response = await api.get('/api/option-history/get-tenor');
      if (Array.isArray(response)) {
        setTenors(response);
        if (response.length > 0) {
          setSelectedTenor(response[0]); // Default to first tenor entry
        }
      }
    } catch (err) {
      console.error("Error fetching tenors:", err);
    } finally {
      setLoadingTenors(false);
    }
  };

  // 1. ORIGINAL PERFORMANCE PIPELINE (Untouched, maintains baseline PnL tracking maps)
  useEffect(() => {
    const fetchAllHistory = async () => {
      if (!legs || legs.length === 0) return;

      setLoading(true);
      setError(null);
      try {
        const requests = legs.map(leg => 
          api.post('/api/option-history/get-contract-history', {
            underlying: leg.underlying,
            optionType: leg.type,
            strikePrice: leg.strike,
            expirationDate: new Date(leg.expiry).toLocaleDateString('en-CA')
          })
        );

        const results = await Promise.all(requests);
        const rawMerged = {};
        const allLegIds = new Set();

        results.forEach((history, index) => {
          const currentLeg = legs[index];
          const legId = `leg_${index}_${currentLeg.underlying}_${currentLeg.strike}`.replace(/[^a-zA-Z0-9_]/g, '');
          allLegIds.add(legId);
          
          if (Array.isArray(history)) {
            history.forEach(point => {
              const dateStr = point.date.split('T')[0];
              if (!rawMerged[dateStr]) {
                rawMerged[dateStr] = { date: dateStr, legsData: {} };
              }
              const price = point.close ?? point.Close ?? 0;
              rawMerged[dateStr].legsData[legId] = price * (currentLeg.quantity || 1);
            });
          }
        });

        const sortedDates = Object.keys(rawMerged).sort((a, b) => new Date(a) - new Date(b));
        let runningInvestment = 0;
        let runningRealizedPnL = 0;
        const entryPrices = {};     
        const activeLegs = new Set(); 

        const finalChartData = sortedDates.map((dateStr, dateIdx) => {
          const dayNode = rawMerged[dateStr];
          let dayActiveMarketValue = 0;

          allLegIds.forEach(legId => {
            const hasDataToday = dayNode.legsData[legId] !== undefined;
            const isCurrentlyActive = activeLegs.has(legId);

            if (hasDataToday) {
              const currentLegValue = dayNode.legsData[legId];
              if (!isCurrentlyActive && entryPrices[legId] === undefined) {
                activeLegs.add(legId);
                entryPrices[legId] = currentLegValue; 
                runningInvestment += currentLegValue;
              }
              dayActiveMarketValue += currentLegValue;
            } else {
              if (isCurrentlyActive) {
                activeLegs.delete(legId);
                const prevDateStr = sortedDates[dateIdx - 1];
                const lastKnownValue = rawMerged[prevDateStr].legsData[legId] || 0;
                const originalCost = entryPrices[legId] || 0;
                runningRealizedPnL += (lastKnownValue - originalCost);
                runningInvestment -= originalCost;
              }
            }
          });

          const totalPnL = runningRealizedPnL + (dayActiveMarketValue - runningInvestment);

          return {
            date: dateStr,
            totalInvestment: runningInvestment,
            totalMarketValue: dayActiveMarketValue,
            totalPnL: totalPnL,
            realizedPnL: runningRealizedPnL,
            ...dayNode.legsData 
          };
        });

        setChartData(finalChartData);
      } catch (err) {
        console.error("Visualization Error:", err);
        setError("Failed to load historical data.");
      } finally {
        setLoading(false);
      }
    };

    fetchAllHistory();
  }, [legs]); 

  // 2. NEW ISOLATED GREEKS AND IV PIPELINE (Triggered on Tenor verification updates)
  useEffect(() => {
    const fetchGreeksAndIvHistory = async () => {
      if (!showGreeks || !selectedTenor || !legs || legs.length === 0) return;

      setLoadingGreeks(true);
      try {
        const requests = legs.map(leg => 
          api.post(`/api/option-history/get-contract-history-greeks-iv?tenor=${selectedTenor}`, {
            underlying: leg.underlying,
            optionType: leg.type,
            strikePrice: leg.strike,
            expirationDate: new Date(leg.expiry).toLocaleDateString('en-CA')
          })
        );

        const results = await Promise.all(requests);
        const rawMergedGreeks = {};

        results.forEach((greeksList, index) => {
          const currentLeg = legs[index];
          const legId = `leg_${index}_${currentLeg.underlying}_${currentLeg.strike}`.replace(/[^a-zA-Z0-9_]/g, '');
          
          if (Array.isArray(greeksList)) {
            greeksList.forEach(point => {
              if (!point.optionHistoryDate) return;
              const dateStr = point.optionHistoryDate.split('T')[0];
              
              if (!rawMergedGreeks[dateStr]) {
                rawMergedGreeks[dateStr] = { date: dateStr };
              }

              const multiplier = currentLeg.quantity || 1;
              
              // Map all metrics to flat subkeys based on back-end response models
              rawMergedGreeks[dateStr][`${legId}_delta`] = (point.delta ?? 0) * multiplier;
              rawMergedGreeks[dateStr][`${legId}_gamma`] = (point.gamma ?? 0) * multiplier;
              rawMergedGreeks[dateStr][`${legId}_theta`] = (point.theta ?? 0) * multiplier;
              rawMergedGreeks[dateStr][`${legId}_vega`]  = (point.vega  ?? 0) * multiplier;
              rawMergedGreeks[dateStr][`${legId}_rho`]   = (point.rho   ?? 0) * multiplier;
              rawMergedGreeks[dateStr][`${legId}_vomma`] = (point.vomma ?? 0) * multiplier;
              rawMergedGreeks[dateStr][`${legId}_iv`]    = (point.impliedVolatility ?? 0);
            });
          }
        });

        // Convert the object maps into chronological arrays for Recharts distribution
        const sortedGreeksTimeline = Object.keys(rawMergedGreeks)
          .sort((a, b) => new Date(a) - new Date(b))
          .map(dateKey => rawMergedGreeks[dateKey]);

        setGreeksChartData(sortedGreeksTimeline);
      } catch (err) {
        console.error("Error generating metrics data mapping:", err);
      } finally {
        setLoadingGreeks(false);
      }
    };

    fetchGreeksAndIvHistory();
  }, [legs, selectedTenor, showGreeks]);

  if (loading) return <div style={statusMessageStyle}>Loading portfolio engine...</div>;
  if (error) return <div style={{ ...statusMessageStyle, color: '#f5222d' }}>{error}</div>;
  if (chartData.length === 0) return <div style={statusMessageStyle}>No data found.</div>;

  return (
    <div style={containerStyle}>
      <button onClick={onBack} style={closeButtonStyle}>✕</button>

      {/* CHART 1: STRATEGY ENGINE PNL (ORIGINAL) */}
      <div style={{ marginBottom: '40px' }}>
        <h3 style={chartTitleStyle}>Net Strategy Performance</h3>
        <p style={subtitleStyle}>Maintains continuity across staggered entries and exits</p>
        <div style={{ width: '100%', height: '320px', marginTop: '20px' }}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
              <XAxis 
                dataKey="date" 
                fontSize={10}
                tickFormatter={(tick) => new Date(tick).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })}
              />
              <YAxis fontSize={10} />
              <Tooltip 
                contentStyle={{ fontSize: '12px', borderRadius: '8px' }}
                labelFormatter={(label) => new Date(label).toLocaleDateString('en-GB', { day: '2-digit', month: 'long', year: 'numeric' })}
              />
              <Legend verticalAlign="top" height={36} wrapperStyle={{ fontSize: '12px' }} />
              <Line type="monotone" dataKey="totalPnL" name="Net Strategy PnL" stroke="#52c41a" strokeWidth={3} dot={false} />
              <Line type="monotone" dataKey="realizedPnL" name="Realized Balance" stroke="#000000" strokeWidth={1.5} strokeDasharray="3 3" dot={false} />
              <Line type="monotone" dataKey="totalInvestment" name="Active Cost Basis" stroke="#8c8c8c" strokeWidth={1.5} strokeDasharray="4 4" dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      <hr style={{ border: '0', borderTop: '1px solid #f0f0f0', marginBottom: '40px' }} />

      {/* CHART 2: LEG BREAKDOWN (ORIGINAL) */}
      <div style={{ marginBottom: '40px' }}>
        <h3 style={chartTitleStyle}>Leg Asset Breakdown</h3>
        <p style={subtitleStyle}>Isolated daily absolute performance value per contract position</p>
        <div style={{ width: '100%', height: '320px', marginTop: '20px' }}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
              <XAxis 
                dataKey="date" 
                fontSize={10}
                tickFormatter={(tick) => new Date(tick).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })}
              />
              <YAxis fontSize={10} />
              <Tooltip 
                contentStyle={{ fontSize: '12px', borderRadius: '8px' }}
                labelFormatter={(label) => new Date(label).toLocaleDateString('en-GB', { day: '2-digit', month: 'long' })}
              />
              <Legend verticalAlign="top" height={36} wrapperStyle={{ fontSize: '12px' }} />
              {legs.map((leg, i) => {
                const legId = `leg_${i}_${leg.underlying}_${leg.strike}`.replace(/[^a-zA-Z0-9_]/g, '');
                const colors = ['#722ed1', '#1890ff', '#faad14', '#f5222d', '#eb2f96'];
                return (
                  <Line
                    key={legId}
                    name={`${leg.underlying} ${leg.strike} ${leg.type} (${leg.quantity > 0 ? `+${leg.quantity}` : leg.quantity})`}
                    type="monotone"
                    dataKey={legId} 
                    stroke={colors[i % colors.length]}
                    strokeWidth={2}
                    dot={false}
                    connectNulls={true}
                  />
                );
              })}
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      <hr style={{ border: '0', borderTop: '1px solid #f0f0f0', marginBottom: '30px' }} />

      {/* INTERACTIVE GREEKS SECTION CONTAINER */}
      <div style={{ marginTop: '20px' }}>
        {!showGreeks ? (
          <button onClick={handleVisualizeGreeksClick} style={visualizeGreeksButtonStyle}>
            Visualize Greeks
          </button>
        ) : (
          <div style={greeksExpandedContainerStyle}>
            
            {/* DROPDOWNS CONTROL ROW */}
            <div style={dropdownRowWrapperStyle}>
              {/* TENOR SELECTOR */}
              <div style={dropdownContainerStyle}>
                <label style={dropdownLabelStyle}>TENOR</label>
                <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <select 
                    value={selectedTenor} 
                    onChange={(e) => setSelectedTenor(e.target.value)}
                    disabled={loadingTenors}
                    style={{ ...dropdownSelectorStyle, backgroundColor: loadingTenors ? '#f5f5f5' : 'white' }}
                  >
                    <option value="">{loadingTenors ? "Loading..." : "-- Tenor --"}</option>
                    {tenors.map((tenor, i) => (
                      <option key={i} value={tenor}>{tenor}</option>
                    ))}
                  </select>
                  {selectedTenor && tenors.length > 0 && selectedTenor !== tenors[0] && (
                    <button onClick={() => setSelectedTenor(tenors[0])} style={clearButtonStyle}>×</button>
                  )}
                </div>
              </div>

              {/* RISK METRIC SELECTOR */}
              <div style={dropdownContainerStyle}>
                <label style={dropdownLabelStyle}>RISK METRIC</label>
                <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                  <select 
                    value={selectedGreek} 
                    onChange={(e) => setSelectedGreek(e.target.value)}
                    style={dropdownSelectorStyle}
                  >
                    <option value="delta">Delta (Δ)</option>
                    <option value="gamma">Gamma (Γ)</option>
                    <option value="theta">Theta (θ)</option>
                    <option value="vega">Vega (ν)</option>
                    <option value="rho">Rho (ρ)</option>
                    <option value="vomma">Vomma</option>
                    <option value="iv">Implied Volatility (IV)</option>
                  </select>
                  {selectedGreek !== 'delta' && (
                    <button onClick={() => setSelectedGreek('delta')} style={clearButtonStyle}>×</button>
                  )}
                </div>
              </div>
            </div>

            {/* RISK METRICS DISTRIBUTION GRAPH AREA
            <div style={{ marginTop: '30px', border: '1px solid #e8e8e8', padding: '24px', borderRadius: '8px', backgroundColor: '#fff' }}>
              <h3 style={chartTitleStyle}>
                {selectedGreek === 'iv' ? "Individual Contract IV Curves" : `Leg Risk Matrix Timeline (${selectedGreek.toUpperCase()})`}
              </h3>
              <div style={{ width: '100%', height: '300px', marginTop: '20px' }}>
                {loadingGreeks ? (
                  <div style={{ ...statusMessageStyle, padding: '100px 0' }}>Updating analytics channels...</div>
                ) : greeksChartData.length === 0 ? (
                  <div style={{ ...statusMessageStyle, padding: '100px 0' }}>Select an explicit tenor option to construct curves.</div>
                ) : (
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart data={greeksChartData}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
                      <XAxis 
                        dataKey="date" 
                        fontSize={10}
                        tickFormatter={(tick) => new Date(tick).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })}
                      />
                      <YAxis fontSize={10} />
                      <Tooltip 
                        contentStyle={{ fontSize: '12px', borderRadius: '8px' }}
                        labelFormatter={(label) => new Date(label).toLocaleDateString('en-GB', { day: '2-digit', month: 'long' })}
                      />
                      <Legend verticalAlign="top" height={36} wrapperStyle={{ fontSize: '12px' }} />
                      {legs.map((leg, i) => {
                        const legId = `leg_${i}_${leg.underlying}_${leg.strike}`.replace(/[^a-zA-Z0-9_]/g, '');
                        const colors = ['#722ed1', '#1890ff', '#faad14', '#f5222d', '#eb2f96'];
                        const targetKey = `${legId}_${selectedGreek}`;

                        return (
                          <Line
                            key={legId}
                            name={`${leg.underlying} ${leg.strike} ${leg.type} (${leg.quantity > 0 ? `+${leg.quantity}` : leg.quantity})`}
                            type="monotone"
                            dataKey={targetKey}
                            stroke={colors[i % colors.length]}
                            strokeWidth={2}
                            dot={false}
                            connectNulls={true}
                          />
                        );
                      })}
                    </LineChart>
                  </ResponsiveContainer>
                )}
              </div>
            </div> */}
            {/* ALL GREEKS GRID VIEW */}
<div style={{ 
  marginTop: '30px', 
  display: 'grid', 
  gridTemplateColumns: 'repeat(auto-fit, minmax(450px, 1fr))', 
  gap: '20px' 
}}>
  {GREEK_METRICS.map(metric => (
    <div key={metric.key} style={{ 
      border: '1px solid #e8e8e8', 
      padding: '20px', 
      borderRadius: '8px', 
      backgroundColor: '#fff' 
    }}>
      <h3 style={chartTitleStyle}>{metric.label}</h3>
      <div style={{ width: '100%', height: '250px', marginTop: '15px' }}>
        {loadingGreeks ? (
          <div style={statusMessageStyle}>Loading...</div>
        ) : (
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={greeksChartData}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
              <XAxis 
                dataKey="date" 
                fontSize={9}
                tickFormatter={(t) => new Date(t).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })}
              />
              <YAxis fontSize={9} width={30} />
              <Tooltip 
                contentStyle={{ fontSize: '11px' }}
                labelFormatter={(l) => new Date(l).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })}
              />
              {legs.map((leg, i) => {
                const legId = `leg_${i}_${leg.underlying}_${leg.strike}`.replace(/[^a-zA-Z0-9_]/g, '');
                const colors = ['#722ed1', '#1890ff', '#faad14', '#f5222d', '#eb2f96'];
                return (
                  <Line
                    key={legId}
                    type="monotone"
                    dataKey={`${legId}_${metric.key}`}
                    name={`${leg.strike} ${leg.type}`}
                    stroke={colors[i % colors.length]}
                    strokeWidth={2}
                    dot={false}
                    connectNulls={true}
                  />
                );
              })}
            </LineChart>
          </ResponsiveContainer>
        )}
      </div>
    </div>
  ))}
</div>

          </div>
        )}
      </div>
    </div>
  );
};

// Styling Rules
const containerStyle = { position: 'relative', padding: '30px', backgroundColor: '#fff', borderRadius: '12px', border: '1px solid #f0f0f0', marginTop: '20px', boxShadow: '0 2px 8px rgba(0,0,0,0.05)', maxHeight: '95vh', overflowY: 'auto' };
const closeButtonStyle = { position: 'absolute', top: '20px', right: '20px', background: 'none', border: 'none', color: '#bfbfbf', fontSize: '20px', cursor: 'pointer', zIndex: 10 };
const chartTitleStyle = { margin: 0, fontSize: '18px', fontWeight: '600', color: '#262626' };
const subtitleStyle = { margin: '4px 0 0 0', fontSize: '13px', color: '#8c8c8c' };
const statusMessageStyle = { padding: '60px', textAlign: 'center', fontSize: '14px', color: '#8c8c8c' };

const visualizeGreeksButtonStyle = { padding: '10px 20px', backgroundColor: '#1890ff', color: '#fff', border: 'none', borderRadius: '4px', fontSize: '14px', fontWeight: '600', cursor: 'pointer', transition: 'background 0.2s' };
const greeksExpandedContainerStyle = { marginTop: '10px', animation: 'fadeIn 0.3s ease-in-out' };
const dropdownRowWrapperStyle = { display: 'flex', gap: '16px', alignItems: 'center', flexWrap: 'wrap' };

const dropdownContainerStyle = { flex: '1', display: 'flex', flexDirection: 'column', gap: '4px', minWidth: '150px', maxWidth: '240px' };
const dropdownLabelStyle = { fontSize: '11px', fontWeight: 'bold', color: '#666' };
const dropdownSelectorStyle = { padding: '8px', borderRadius: '4px', border: '1px solid #ddd', width: '100%', fontSize: '13px', color: '#262626', outline: 'none' };
const clearButtonStyle = { border: 'none', background: 'none', color: '#ff4d4f', cursor: 'pointer', fontSize: '20px', lineHeight: '1' };

export default Visualize;