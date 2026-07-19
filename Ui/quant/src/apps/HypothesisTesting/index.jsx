import React, { useState, useEffect, useMemo } from 'react';
import { theme } from '../../core/theme';
import { api } from '../../core/api';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, ReferenceArea } from 'recharts';

const commonStyles = {
  appContainer: { padding: '24px', maxWidth: '1200px', margin: '0 auto', fontFamily: 'system-ui, -apple-system, sans-serif' },
  surface: { backgroundColor: '#ffffff', borderRadius: '12px', padding: '32px', boxShadow: '0 4px 12px rgba(0,0,0,0.05)', border: '1px solid #e2e8f0' }
};

const TimelineBoxPlotChart = ({ instances, formatDate }) => {
  if (!instances || instances.length === 0) return null;

  // 1. Find date boundaries
  const dateTimes = instances.map(inst => ({
    start: new Date(inst.startDate || inst.StartDate).getTime(),
    end: new Date(inst.endDate || inst.EndDate).getTime()
  }));
  const minTime = Math.min(...dateTimes.map(d => d.start));
  const maxTime = Math.max(...dateTimes.map(d => d.end));
  const totalTimeRange = maxTime - minTime || 1;

  // 2. Find return boundaries
  let minReturn = 0;
  let maxReturn = 0;
  let hasData = false;

  instances.forEach(inst => {
    const q = inst.metrics?.boxPlotQuartiles || inst.Metrics?.BoxPlotQuartiles;
    if (Array.isArray(q) && q.length === 5) {
      if (!hasData) {
        minReturn = q[0];
        maxReturn = q[4];
        hasData = true;
      } else {
        minReturn = Math.min(minReturn, q[0]);
        maxReturn = Math.max(maxReturn, q[4]);
      }
    }
  });

  if (!hasData) {
    return (
      <div style={{ textAlign: 'center', padding: '20px', color: '#64748b', backgroundColor: '#f8fafc', borderRadius: '8px', border: '1px dashed #cbd5e1', marginBottom: '24px' }}>
        No return distribution quartile data found for plotting.
      </div>
    );
  }

  const range = maxReturn - minReturn || 0.01;
  const padding = range * 0.1;
  const yMin = minReturn - padding;
  const yMax = maxReturn + padding;
  const yRange = yMax - yMin;

  const width = 1100;
  const height = 400;
  const margin = { top: 30, right: 40, bottom: 40, left: 60 };
  const chartWidth = width - margin.left - margin.right;
  const chartHeight = height - margin.top - margin.bottom;

  const getX = (time) => margin.left + ((time - minTime) / totalTimeRange) * chartWidth;
  const getY = (val) => margin.top + chartHeight - ((val - yMin) / yRange) * chartHeight;

  const getRegimeColor = (regimeId) => {
    const colors = {
      '0': '#2563eb',
      '1': '#10b981',
      '2': '#f59e0b',
      '3': '#8b5cf6',
      '4': '#ec4899',
    };
    return colors[regimeId] || '#64748b';
  };

  const [hoveredInstance, setHoveredInstance] = useState(null);
  const [tooltipPos, setTooltipPos] = useState({ x: 0, y: 0 });

  const gridTicks = [];
  const step = range / 5;
  for (let i = 0; i <= 5; i++) {
    gridTicks.push(minReturn + i * step);
  }

  return (
    <div style={{ position: 'relative', backgroundColor: '#ffffff', border: '1px solid #cbd5e1', borderRadius: '12px', padding: '24px', marginBottom: '24px', boxShadow: '0 1px 3px rgba(0,0,0,0.05)' }}>
      <h5 style={{ fontSize: '14px', fontWeight: '700', color: '#1e293b', margin: '0 0 16px 0', display: 'flex', alignItems: 'center', gap: '8px' }}>
        📈 Regime Occurrence Timeline & Box Plots <span style={{ fontSize: '11px', fontWeight: '400', color: '#64748b' }}>(Hover blocks for details)</span>
      </h5>
      
      <svg viewBox={`0 0 ${width} ${height}`} width="100%" height="auto" style={{ overflow: 'visible', userSelect: 'none' }}>
        {gridTicks.map((val, idx) => {
          const y = getY(val);
          return (
            <g key={idx}>
              <line x1={margin.left} y1={y} x2={width - margin.right} y2={y} stroke="#f1f5f9" strokeWidth="1" />
              <text x={margin.left - 8} y={y + 4} textAnchor="end" fontSize="10px" fill="#64748b" fontFamily="monospace">
                {(val * 100).toFixed(1)}%
              </text>
            </g>
          );
        })}

        {yMin < 0 && yMax > 0 && (
          <line x1={margin.left} y1={getY(0)} x2={width - margin.right} y2={getY(0)} stroke="#94a3b8" strokeWidth="1" strokeDasharray="4 4" />
        )}

        {[0, 0.25, 0.5, 0.75, 1].map((ratio, idx) => {
          const time = minTime + ratio * totalTimeRange;
          const x = getX(time);
          const dateStr = new Date(time).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: '2-digit' });
          return (
            <g key={idx}>
              <line x1={x} y1={margin.top} x2={x} y2={height - margin.bottom} stroke="#f8fafc" strokeWidth="1" />
              <text x={x} y={height - margin.bottom + 18} textAnchor="middle" fontSize="10px" fill="#64748b">
                {dateStr}
              </text>
            </g>
          );
        })}

        {instances.map((inst, idx) => {
          const q = inst.metrics?.boxPlotQuartiles || inst.Metrics?.BoxPlotQuartiles;
          if (!Array.isArray(q) || q.length !== 5) return null;

          const tStart = new Date(inst.startDate || inst.StartDate).getTime();
          const tEnd = new Date(inst.endDate || inst.EndDate).getTime();
          
          const xStart = getX(tStart);
          const xEnd = getX(tEnd);
          
          const boxWidth = Math.max(14, xEnd - xStart);
          const xCenter = xStart + (boxWidth / 2);

          const yMinVal = getY(q[0]);
          const yQ25 = getY(q[1]);
          const yMedian = getY(q[2]);
          const yQ75 = getY(q[3]);
          const yMaxVal = getY(q[4]);

          const color = getRegimeColor(inst.regimeId);

          return (
            <g 
              key={idx}
              onMouseEnter={(e) => {
                setHoveredInstance(inst);
                setTooltipPos({
                  x: xCenter + 15,
                  y: yMedian - 90
                });
              }}
              onMouseLeave={() => setHoveredInstance(null)}
              style={{ cursor: 'pointer' }}
            >
              <line x1={xCenter} y1={yMinVal} x2={xCenter} y2={yQ25} stroke={color} strokeWidth="1.5" />
              <line x1={xCenter} y1={yQ75} x2={xCenter} y2={yMaxVal} stroke={color} strokeWidth="1.5" />

              <line x1={xCenter - 4} y1={yMinVal} x2={xCenter + 4} y2={yMinVal} stroke={color} strokeWidth="1.5" />
              <line x1={xCenter - 4} y1={yMaxVal} x2={xCenter + 4} y2={yMaxVal} stroke={color} strokeWidth="1.5" />

              <rect 
                x={xCenter - (boxWidth / 2)}
                y={yQ75}
                width={boxWidth}
                height={Math.max(2, yQ25 - yQ75)}
                fill={color}
                fillOpacity="0.2"
                stroke={color}
                strokeWidth="1.5"
              />

              <line 
                x1={xCenter - (boxWidth / 2)}
                y1={yMedian}
                x2={xCenter + (boxWidth / 2)}
                y2={yMedian}
                stroke={color}
                strokeWidth="2.5"
              />
            </g>
          );
        })}
      </svg>

      {hoveredInstance && (() => {
        const q = hoveredInstance.metrics?.boxPlotQuartiles || hoveredInstance.Metrics?.BoxPlotQuartiles || [];
        return (
          <div style={{
            position: 'absolute',
            left: `${tooltipPos.x}px`,
            top: `${tooltipPos.y}px`,
            width: '260px',
            backgroundColor: 'rgba(15, 23, 42, 0.95)',
            color: '#ffffff',
            padding: '12px',
            borderRadius: '6px',
            boxShadow: '0 8px 16px rgba(0, 0, 0, 0.25)',
            fontSize: '11px',
            zIndex: 10,
            pointerEvents: 'none',
            border: '1px solid #334155'
          }}>
            <div style={{ fontWeight: '700', borderBottom: '1px solid #334155', paddingBottom: '4px', marginBottom: '6px', display: 'flex', justifyContent: 'space-between' }}>
              <span>Instance #{hoveredInstance.instanceId || hoveredInstance.InstanceId}</span>
              <span style={{ color: '#38bdf8' }}>Regime {hoveredInstance.regimeId}</span>
            </div>
            <div style={{ marginBottom: '4px' }}>
              📅 {formatDate(hoveredInstance.startDate || hoveredInstance.StartDate)} to {formatDate(hoveredInstance.endDate || hoveredInstance.EndDate)}
            </div>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px' }}>
              <div>CAGR: <strong style={{ color: (hoveredInstance.metrics?.cagr ?? hoveredInstance.Metrics?.Cagr ?? 0) >= 0 ? '#4ade80' : '#f87171' }}>{((hoveredInstance.metrics?.cagr ?? hoveredInstance.Metrics?.Cagr ?? 0) * 100).toFixed(1)}%</strong></div>
              <div>Sharpe: <strong>{(hoveredInstance.metrics?.sharpe ?? hoveredInstance.Metrics?.Sharpe ?? 0).toFixed(2)}</strong></div>
            </div>
            {q.length === 5 && (
              <div style={{ borderTop: '1px solid #334155', paddingTop: '6px', fontFamily: 'monospace' }}>
                <div style={{ color: '#94a3b8', fontSize: '9px', marginBottom: '2px' }}>Return Quartiles:</div>
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '9px' }}>
                  <div>Min: {(q[0]*100).toFixed(1)}%</div>
                  <div>25%: {(q[1]*100).toFixed(1)}%</div>
                  <div>Med: {(q[2]*100).toFixed(1)}%</div>
                  <div>75%: {(q[3]*100).toFixed(1)}%</div>
                  <div>Max: {(q[4]*100).toFixed(1)}%</div>
                </div>
              </div>
            )}
          </div>
        );
      })()}
    </div>
  );
};
const computeCumulativeReturns = (data, keys) => {
  if (!data || data.length === 0) return [];
  
  const accumulators = {};
  keys.forEach(k => {
    accumulators[k] = 1.0;
  });
  
  return data.map(row => {
    const newRow = { ...row };
    keys.forEach(k => {
      const val = Number(row[k]) || 0;
      accumulators[k] *= (1.0 + val);
      newRow[k] = (accumulators[k] - 1.0) * 100;
    });
    return newRow;
  });
};

const formatDailyReturns = (data, keys) => {
  if (!data || data.length === 0) return [];
  return data.map(row => {
    const newRow = { ...row };
    keys.forEach(k => {
      newRow[k] = (Number(row[k]) || 0) * 100;
    });
    return newRow;
  });
};

const ReturnsLineChart = ({ data, variantKeys, instances }) => {
  const [chartType, setChartType] = useState('cumulative');
  const [selectedRegime, setSelectedRegime] = useState('all');
  const [hiddenKeys, setHiddenKeys] = useState({});

  const handleLegendClick = (e) => {
    const { dataKey } = e;
    if (dataKey) {
      setHiddenKeys(prev => ({
        ...prev,
        [dataKey]: !prev[dataKey]
      }));
    }
  };

  if (!data || data.length === 0 || variantKeys.length === 0) return null;

  // Extract distinct regimes from data
  const distinctRegimes = Array.from(new Set(data.map(row => {
    const rVal = row.ClusterLabel !== undefined ? row.ClusterLabel : row.clusterLabel;
    return rVal !== undefined && rVal !== null ? String(rVal) : null;
  }).filter(v => v !== null))).sort();

  // Filter data by selected regime
  const filteredData = selectedRegime === 'all'
    ? data
    : data.filter(row => {
        const rVal = row.ClusterLabel !== undefined ? row.ClusterLabel : row.clusterLabel;
        return String(rVal) === selectedRegime;
      });

  const processedData = chartType === 'cumulative' 
    ? computeCumulativeReturns(filteredData, variantKeys)
    : formatDailyReturns(filteredData, variantKeys);

  const normalizeDate = (dateStr) => {
    if (!dateStr) return '';
    try {
      const d = new Date(dateStr);
      if (isNaN(d.getTime())) return dateStr;
      return d.toISOString().split('T')[0];
    } catch (e) {
      return dateStr;
    }
  };

  const chartData = processedData.map(row => {
    const rawDate = row.Date || row.date || '';
    return {
      ...row,
      normalizedDate: normalizeDate(rawDate)
    };
  });

  const getLineColor = (index) => {
    const colors = ['#2563eb', '#10b981', '#ef4444', '#f59e0b', '#8b5cf6', '#ec4899'];
    return colors[index % colors.length];
  };

  const getRegimeColor = (regimeId) => {
    const colors = {
      '0': '#2563eb',
      '1': '#10b981',
      '2': '#f59e0b',
      '3': '#8b5cf6',
      '4': '#ec4899',
    };
    return colors[regimeId] || '#64748b';
  };

  return (
    <div style={{ backgroundColor: '#ffffff', border: '1px solid #cbd5e1', borderRadius: '12px', padding: '24px', marginBottom: '24px', boxShadow: '0 1px 3px rgba(0,0,0,0.05)' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px', borderBottom: '1px solid #f1f5f9', paddingBottom: '10px', flexWrap: 'wrap', gap: '12px' }}>
        <h5 style={{ fontSize: '14px', fontWeight: '700', color: '#1e293b', margin: 0 }}>
          📈 Strategy Returns Performance Chart
        </h5>
        
        <div style={{ display: 'flex', alignItems: 'center', gap: '16px' }}>
          {/* Regime Filter Dropdown */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
            <span style={{ fontSize: '11.5px', fontWeight: '700', color: '#64748b' }}>Regime Filter:</span>
            <select
              value={selectedRegime}
              onChange={(e) => setSelectedRegime(e.target.value)}
              style={{
                padding: '4px 8px',
                fontSize: '11.5px',
                fontWeight: '600',
                border: '1px solid #cbd5e1',
                borderRadius: '6px',
                color: '#475569',
                backgroundColor: '#ffffff',
                cursor: 'pointer'
              }}
            >
              <option value="all">All Regimes Combined</option>
              {distinctRegimes.map(r => (
                <option key={r} value={r}>Regime #{r}</option>
              ))}
            </select>
          </div>

          {/* Chart Type Toggle Buttons */}
          <div style={{ display: 'flex', gap: '4px', backgroundColor: '#f1f5f9', padding: '2px', borderRadius: '6px' }}>
            <button
              onClick={() => setChartType('cumulative')}
              style={{
                padding: '4px 12px',
                fontSize: '11px',
                fontWeight: '600',
                border: 'none',
                borderRadius: '4px',
                backgroundColor: chartType === 'cumulative' ? '#ffffff' : 'transparent',
                color: chartType === 'cumulative' ? '#1e293b' : '#64748b',
                cursor: 'pointer',
                boxShadow: chartType === 'cumulative' ? '0 1px 2px rgba(0,0,0,0.05)' : 'none'
              }}
            >
              Cumulative Return
            </button>
            <button
              onClick={() => setChartType('daily')}
              style={{
                padding: '4px 12px',
                fontSize: '11px',
                fontWeight: '600',
                border: 'none',
                borderRadius: '4px',
                backgroundColor: chartType === 'daily' ? '#ffffff' : 'transparent',
                color: chartType === 'daily' ? '#1e293b' : '#64748b',
                cursor: 'pointer',
                boxShadow: chartType === 'daily' ? '0 1px 2px rgba(0,0,0,0.05)' : 'none'
              }}
            >
              Daily Returns
            </button>
          </div>
        </div>
      </div>

      <div style={{ width: '100%', height: 350 }}>
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={chartData} margin={{ top: 10, right: 30, left: 10, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
            <XAxis 
              dataKey="normalizedDate" 
              stroke="#64748b" 
              fontSize={10} 
              tickLine={false} 
              tickFormatter={(val) => {
                try {
                  return new Date(val).toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
                } catch(e) {
                  return val;
                }
              }}
            />
            <YAxis 
              stroke="#64748b" 
              fontSize={10} 
              tickLine={false} 
              tickFormatter={(value) => `${value.toFixed(1)}%`}
            />
            <Tooltip 
              contentStyle={{ backgroundColor: 'rgba(15, 23, 42, 0.95)', color: '#ffffff', borderRadius: '6px', fontSize: '11px', border: '1px solid #334155' }}
              labelStyle={{ fontWeight: '700', borderBottom: '1px solid #334155', paddingBottom: '4px', marginBottom: '4px' }}
              labelFormatter={(val) => {
                try {
                  return new Date(val).toLocaleDateString(undefined, { month: 'long', day: 'numeric', year: 'numeric' });
                } catch(e) {
                  return val;
                }
              }}
              formatter={(value) => [`${value.toFixed(2)}%`]}
            />
            <Legend 
              verticalAlign="top" 
              height={36} 
              iconType="circle" 
              wrapperStyle={{ fontSize: '11px', cursor: 'pointer' }}
              onClick={handleLegendClick}
              formatter={(value, entry) => (
                <span style={{ color: hiddenKeys[entry.dataKey] ? '#94a3b8' : '#1e293b', textDecoration: hiddenKeys[entry.dataKey] ? 'line-through' : 'none' }}>
                  {value}
                </span>
              )}
            />
            
            {Array.isArray(instances) && instances.map((inst, idx) => {
              const x1 = normalizeDate(inst.startDate || inst.StartDate);
              const x2 = normalizeDate(inst.endDate || inst.EndDate);
              const color = getRegimeColor(inst.regimeId);
              return (
                <ReferenceArea
                  key={`ref-${idx}`}
                  x1={x1}
                  x2={x2}
                  fill={color}
                  fillOpacity={0.06}
                  stroke="none"
                  ifOverflow="extendDomain"
                />
              );
            })}

            {variantKeys.map((key, idx) => (
              <Line 
                key={key}
                type="monotone"
                dataKey={key}
                name={key}
                stroke={getLineColor(idx)}
                strokeWidth={2}
                dot={false}
                activeDot={{ r: 4 }}
                hide={!!hiddenKeys[key]}
              />
            ))}
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};

const PayoffDiagram = ({ strategiesList, activeConfigId }) => {
  // Generate payoff curve data
  const data = useMemo(() => {
    const spot = 100.0;
    const steps = 41; // 41 points from 80 to 120
    const payoffData = [];

    const calcLegPayoff = (leg, sTVal) => {
      const isOption = leg.legType === 'Option' || leg.LegType === 'Option';
      const rawIsLong = leg.isLong !== undefined ? leg.isLong : leg.IsLong;
      const isLong = rawIsLong === 'Long' || rawIsLong === true;
      const rawIsCall = leg.isCall !== undefined ? leg.isCall : leg.IsCall;
      const isCall = rawIsCall === 'Call' || rawIsCall === true;
      const offset = Number(leg.moneynessOffset !== undefined ? leg.moneynessOffset : (leg.MoneynessOffset || 0));
      
      const strike = spot * (1.0 + offset);
      const direction = isLong ? 1.0 : -1.0;
      
      if (isOption) {
        if (isCall) {
          return Math.max(sTVal - strike, 0) * direction;
        } else {
          return Math.max(strike - sTVal, 0) * direction;
        }
      } else {
        return (sTVal - spot) * direction;
      }
    };

    for (let i = 0; i < steps; i++) {
      const sT = spot * (0.8 + (i * 0.4) / (steps - 1)); // ranges from 80% to 120% of spot
      const formattedSt = sT.toFixed(1);
      
      const point = {
        underlyingPrice: Number(formattedSt)
      };

      strategiesList.forEach(strat => {
        let netPayoff = 0;
        const legs = strat.legs || [];
        legs.forEach(leg => {
          netPayoff += calcLegPayoff(leg, sT);
        });
        point[`payoff_${strat.id}`] = Number(netPayoff.toFixed(2));
        
        // Render individual legs only for the active strategy tab
        if (strat.id === activeConfigId) {
          legs.forEach((leg, idx) => {
            point[`leg_${idx + 1}`] = Number(calcLegPayoff(leg, sT).toFixed(2));
          });
        }
      });

      payoffData.push(point);
    }

    return payoffData;
  }, [strategiesList, activeConfigId]);

  const activeStrat = strategiesList.find(s => s.id === activeConfigId);
  const activeLegs = activeStrat ? (activeStrat.legs || []) : [];
  const colors = ['#2563eb', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6', '#06b6d4'];

  return (
    <div style={{ border: '1px solid #e2e8f0', borderRadius: '12px', padding: '24px', backgroundColor: '#ffffff', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.05)', textAlign: 'left' }}>
      <h4 style={{ fontSize: '15px', fontWeight: '700', color: '#1e293b', marginBottom: '8px' }}>
        📈 Payoff Diagram at Expiration
      </h4>
      <p style={{ color: '#64748b', fontSize: '12.5px', marginBottom: '20px', lineHeight: '1.5' }}>
        Combined intrinsic payoff structure of all compared strategies at expiration (normalized to spot baseline of 100.0). Dashed lines show individual legs of the active strategy.
      </p>

      <div style={{ width: '100%', height: '300px' }}>
        <ResponsiveContainer width="100%" height="100%">
          <LineChart data={data} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
            <XAxis 
              dataKey="underlyingPrice" 
              stroke="#64748b" 
              fontSize={11} 
              tickLine={false} 
            />
            <YAxis 
              stroke="#64748b" 
              fontSize={11} 
              tickLine={false} 
            />
            <Tooltip 
              contentStyle={{ backgroundColor: '#ffffff', borderRadius: '8px', border: '1px solid #e2e8f0', fontSize: '12px' }}
              formatter={(value, name) => {
                if (name.startsWith('payoff_')) {
                  const sId = name.replace('payoff_', '');
                  const stratObj = strategiesList.find(s => s.id === sId);
                  return [`$${value}`, stratObj ? stratObj.variantId : 'Strategy Payoff'];
                }
                return [`$${value}`, name.replace('leg_', 'Leg #')];
              }}
            />
            <Legend 
              wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }} 
              formatter={(value) => {
                if (value.startsWith('payoff_')) {
                  const sId = value.replace('payoff_', '');
                  const stratObj = strategiesList.find(s => s.id === sId);
                  const idx = strategiesList.findIndex(s => s.id === sId);
                  return <strong style={{ color: colors[idx % colors.length] }}>{stratObj ? stratObj.variantId : 'Strategy'} P&L</strong>;
                }
                return value.replace('leg_', 'Leg #');
              }}
            />
            {strategiesList.map((strat, idx) => (
              <Line 
                key={strat.id}
                type="monotone" 
                dataKey={`payoff_${strat.id}`} 
                stroke={colors[idx % colors.length]} 
                strokeWidth={3} 
                dot={false} 
                activeDot={{ r: 6 }} 
              />
            ))}
            {activeLegs.map((leg, idx) => (
              <Line 
                key={leg.id}
                type="monotone" 
                dataKey={`leg_${idx + 1}`} 
                stroke={['#f59e0b', '#ec4899', '#8b5cf6', '#06b6d4'][idx % 4]} 
                strokeWidth={1.5}
                strokeDasharray="4 4"
                dot={false} 
              />
            ))}
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};

const HypothesisTesting = () => {
  const [models, setModels] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  
  // Available architecture names for dropdown
  const [availableArchitectures, setAvailableArchitectures] = useState([]);

  // Dynamic filter blueprints (Features & Parameters) based on selected architecture
  const [featureBlueprints, setFeatureBlueprints] = useState([]);
  const [modelParameters, setModelParameters] = useState([]);

  // Filter States
  const [modelName, setModelName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [status, setStatus] = useState('2'); // Default to '2' (Trained)
  const [selectedFeature, setSelectedFeature] = useState('');
  const [parameterFilters, setParameterFilters] = useState({});
  const [featureParamFilters, setFeatureParamFilters] = useState({});
  const [featureFilterBlocks, setFeatureFilterBlocks] = useState([]);

  // Add a new feature block instance to filters
  const addFeatureFilterBlock = () => {
    if (!selectedFeature) return;
    const blueprint = featureBlueprints.find(fb => (fb.featureCode || fb.FeatureCode) === selectedFeature);
    if (!blueprint) return;

    const defaultValues = {};
    const blueprintParams = blueprint.parameters || blueprint.Parameters;

    blueprintParams?.forEach(p => {
      const pName = p.name || p.Name;
      const pDefault = p.defaultValue !== null ? (p.defaultValue ?? p.DefaultValue) : '';
      if (pName) {
        defaultValues[pName] = pDefault !== null ? pDefault : '';
      }
    });

    const newBlockInstance = {
      id: `filter-${Date.now()}-${Math.random().toString(36).substring(2, 6)}`,
      featureCode: blueprint.featureCode || blueprint.FeatureCode,
      displayName: blueprint.displayName || blueprint.DisplayName,
      configuredValues: defaultValues
    };

    setFeatureFilterBlocks(prev => [...prev, newBlockInstance]);
    setSelectedFeature(''); // Reset selector
  };

  // Remove a feature filter block
  const removeFeatureFilterBlock = (instanceId) => {
    setFeatureFilterBlocks(prev => prev.filter(b => b.id !== instanceId));
  };

  // Handle value change inside a feature filter block
  const handleFeatureBlockValueChange = (instanceId, paramName, value, type) => {
    const parsedValue = (type === 'number' || type === 'Integer') && value !== '' ? Number(value) : value;

    setFeatureFilterBlocks(prev => prev.map(block => {
      if (block.id !== instanceId) return block;
      return {
        ...block,
        configuredValues: { 
          ...block.configuredValues, 
          [paramName]: parsedValue 
        }
      };
    }));
  };

  // --- Hypothesis Backtesting Workspace State ---
  const [backtestModel, setBacktestModel] = useState(null);
  const [strategyMetadata, setStrategyMetadata] = useState(null);
  const [activeConfigId, setActiveConfigId] = useState(null);
  
  // Track configurations for N strategies in an array
  const [strategies, setStrategies] = useState([]);

  const [backtestResults, setBacktestResults] = useState(null); // stores the full variants dict now
  const [selectedResultVariantId, setSelectedResultVariantId] = useState(null); // active variant tab in results
  const [labeledData, setLabeledData] = useState(null);
  const [backtesting, setBacktesting] = useState(false);
  const [backtestError, setBacktestError] = useState(null);
  const [activeRegimeTab, setActiveRegimeTab] = useState(null);

  // Fetch backtest metadata from .NET backend when entering backtest mode
  useEffect(() => {
    if (!backtestModel) {
      setStrategyMetadata(null);
      setStrategies([]);
      setActiveConfigId(null);
      setBacktestResults(null);
      setSelectedResultVariantId(null);
      setBacktestError(null);
      setActiveRegimeTab(null);
      return;
    }

    const fetchMetadata = async () => {
      try {
        const data = await api.get('/api/hypothesis/metadata');
        setStrategyMetadata(data);
        if (data.strategies && data.strategies.length > 0) {
          const defaultStrategy = data.strategies[0];
          const initialId = `strat-${Date.now()}`;
          const initialConfig = {
            id: initialId,
            selectedStrategy: defaultStrategy.strategyName,
            variantId: defaultStrategy.strategyName + "_1",
            legs: []
          };
          setStrategies([initialConfig]);
          setActiveConfigId(initialId);
          
          // Apply template
          const defaultLegs = defaultStrategy.defaultLegs || defaultStrategy.DefaultLegs || [];
          const legs = defaultLegs.map((leg, index) => {
            const legObj = { id: `leg-${Date.now()}-${index}-${Math.random().toString(36).substring(2, 6)}` };
            data.legParameters.forEach(param => {
              const pKey = param.key || param.Key;
              let templateVal = leg[pKey] !== undefined ? leg[pKey] : (leg[pKey.charAt(0).toUpperCase() + pKey.slice(1)]);
              if (pKey === 'isLong' && typeof templateVal === 'boolean') {
                templateVal = templateVal ? 'Long' : 'Short';
              } else if (pKey === 'isCall' && typeof templateVal === 'boolean') {
                templateVal = templateVal ? 'Put' : 'Call';
              }
              legObj[pKey] = templateVal !== undefined ? templateVal : param.defaultValue;
            });
            return legObj;
          });
          
          setStrategies([{ ...initialConfig, legs }]);
        }
      } catch (err) {
        console.error("Failed to load strategy metadata:", err);
        setBacktestError("Failed to load strategy configuration parameters from .NET backend.");
      }
    };
    fetchMetadata();
  }, [backtestModel]);

  const applyStrategyTemplate = (template, legParams, targetId = activeConfigId) => {
    if (!template) return;
    const defaultLegs = template.defaultLegs || template.DefaultLegs || [];
    
    const legs = defaultLegs.map((leg, index) => {
      const legObj = { id: `leg-${Date.now()}-${index}-${Math.random().toString(36).substring(2, 6)}` };
      legParams.forEach(param => {
        const pKey = param.key || param.Key;
        let templateVal = leg[pKey] !== undefined ? leg[pKey] : (leg[pKey.charAt(0).toUpperCase() + pKey.slice(1)]);
        
        if (pKey === 'isLong' && typeof templateVal === 'boolean') {
          templateVal = templateVal ? 'Long' : 'Short';
        } else if (pKey === 'isCall' && typeof templateVal === 'boolean') {
          templateVal = templateVal ? 'Put' : 'Call';
        }
        
        legObj[pKey] = templateVal !== undefined ? templateVal : param.defaultValue;
      });
      return legObj;
    });
    
    setStrategies(prev => prev.map(strat => {
      if (strat.id !== targetId) return strat;
      return { ...strat, legs };
    }));
  };

  const handleStrategyTemplateChange = (strategyName) => {
    setStrategies(prev => prev.map(strat => {
      if (strat.id !== activeConfigId) return strat;
      return {
        ...strat,
        selectedStrategy: strategyName,
        variantId: strategyName + "_" + (prev.findIndex(s => s.id === activeConfigId) + 1)
      };
    }));
    
    if (!strategyMetadata) return;
    const template = strategyMetadata.strategies.find(s => s.strategyName === strategyName);
    if (template) {
      applyStrategyTemplate(template, strategyMetadata.legParameters, activeConfigId);
    }
  };

  const addStrategyConfig = () => {
    if (!strategyMetadata || strategyMetadata.strategies.length === 0) return;
    const defaultStrategy = strategyMetadata.strategies[0];
    const newId = `strat-${Date.now()}`;
    const newNum = strategies.length + 1;
    
    const newConfig = {
      id: newId,
      selectedStrategy: defaultStrategy.strategyName,
      variantId: `${defaultStrategy.strategyName}_${newNum}`,
      legs: []
    };
    
    const defaultLegs = defaultStrategy.defaultLegs || defaultStrategy.DefaultLegs || [];
    const legs = defaultLegs.map((leg, index) => {
      const legObj = { id: `leg-${Date.now()}-${index}-${Math.random().toString(36).substring(2, 6)}` };
      strategyMetadata.legParameters.forEach(param => {
        const pKey = param.key || param.Key;
        let templateVal = leg[pKey] !== undefined ? leg[pKey] : (leg[pKey.charAt(0).toUpperCase() + pKey.slice(1)]);
        if (pKey === 'isLong' && typeof templateVal === 'boolean') {
          templateVal = templateVal ? 'Long' : 'Short';
        } else if (pKey === 'isCall' && typeof templateVal === 'boolean') {
          templateVal = templateVal ? 'Put' : 'Call';
        }
        legObj[pKey] = templateVal !== undefined ? templateVal : param.defaultValue;
      });
      return legObj;
    });

    setStrategies(prev => [...prev, { ...newConfig, legs }]);
    setActiveConfigId(newId);
  };

  const removeStrategyConfig = (stratId) => {
    if (strategies.length <= 1) return;
    const activeIdx = strategies.findIndex(s => s.id === activeConfigId);
    const newStrategies = strategies.filter(s => s.id !== stratId);
    setStrategies(newStrategies);
    
    if (activeConfigId === stratId) {
      const nextActive = newStrategies[activeIdx === 0 ? 0 : activeIdx - 1];
      setActiveConfigId(nextActive.id);
    }
  };

  const addNewLeg = () => {
    if (!strategyMetadata) return;
    const newLeg = { id: `leg-${Date.now()}-${Math.random().toString(36).substring(2, 6)}` };
    strategyMetadata.legParameters.forEach(param => {
      const pKey = param.key || param.Key;
      newLeg[pKey] = param.defaultValue;
    });
    
    setStrategies(prev => prev.map(strat => {
      if (strat.id !== activeConfigId) return strat;
      return { ...strat, legs: [...strat.legs, newLeg] };
    }));
  };

  const removeLeg = (legId) => {
    setStrategies(prev => prev.map(strat => {
      if (strat.id !== activeConfigId) return strat;
      return { ...strat, legs: strat.legs.filter(l => l.id !== legId) };
    }));
  };

  const handleLegValueChange = (legId, key, value, inputType) => {
    let parsedValue = value;
    if (inputType === 'number') {
      parsedValue = value !== '' ? Number(value) : '';
    } else if (inputType === 'boolean' || value === 'true' || value === 'false') {
      parsedValue = value === 'true' || value === true;
    }

    setStrategies(prev => prev.map(strat => {
      if (strat.id !== activeConfigId) return strat;
      return {
        ...strat,
        legs: strat.legs.map(leg => {
          if (leg.id !== legId) return leg;
          return { ...leg, [key]: parsedValue };
        })
      };
    }));
  };

  const isLegParamVisible = (param, leg) => {
    const visibleIfProp = param.visibleIfProperty || param.VisibleIfProperty;
    if (!visibleIfProp) return true;

    const visibleIfValues = (param.visibleIfValues || param.VisibleIfValues || []).map(v => String(v).toLowerCase());
    const parentVal = String(leg[visibleIfProp] ?? '').toLowerCase();

    if (visibleIfValues.length === 0) {
      return parentVal !== '';
    }
    return visibleIfValues.includes(parentVal);
  };

  const handleRunBacktest = async () => {
    setBacktesting(true);
    setBacktestError(null);
    setBacktestResults(null);
    setLabeledData(null);

    const strategyConfigsList = [];
    strategies.forEach(config => {
      if (config.legs.length > 0) {
        strategyConfigsList.push({
          variantId: `${config.variantId}_${Date.now().toString().slice(-4)}`,
          strategyName: config.selectedStrategy,
          legs: config.legs.map(leg => {
            const { id, ...rest } = leg;
            return {
              legType: rest.legType,
              isLong: rest.isLong === 'Long' || rest.isLong === true || rest.isLong === 'true',
              isCall: rest.isCall === 'Put' || rest.isCall === true || rest.isCall === 'true',
              moneynessOffset: Number(rest.moneynessOffset) || 0.0,
              expiryTenorDays: Number(rest.expiryTenorDays) || 30
            };
          })
        });
      }
    });

    if (strategyConfigsList.length === 0) {
      setBacktestError("At least one strategy must have configured option legs.");
      setBacktesting(false);
      return;
    }

    const payload = {
      modelGuid: backtestModel.id || backtestModel.Id,
      strategyConfigs: strategyConfigsList
    };

    try {
      const data = await api.post('/api/hypothesis/backtest', payload);
      let parsedData = data;
      if (typeof data === 'string') {
        try {
          parsedData = JSON.parse(data);
        } catch (e) {
          console.error("Failed to parse backtest response string:", e);
        }
      }

      const variants = parsedData?.variants || parsedData?.Variants || {};
      setBacktestResults(variants);
      setLabeledData(parsedData?.labeledData || parsedData?.LabeledData || []);

      const firstVariantKey = Object.keys(variants)[0];
      setSelectedResultVariantId(firstVariantKey || null);

      if (firstVariantKey) {
        const firstVariantResult = variants[firstVariantKey];
        const profiles = firstVariantResult?.regimeProfiles || firstVariantResult?.RegimeProfiles || {};
        const firstRegime = Object.keys(profiles)[0];
        if (firstRegime) {
          setActiveRegimeTab(firstRegime);
        }
      }
    } catch (err) {
      console.error("Backtest failed:", err);
      setBacktestError(err?.message || "Failed to execute backtest on the Python engine.");
    } finally {
      setBacktesting(false);
    }
  };

  // Render the backtest workspace component
  const renderBacktestWorkspace = () => {
    const computeRegimeSummaries = (profilesObj) => {
      if (!profilesObj) return [];
      
      return Object.entries(profilesObj).map(([regimeId, profile]) => {
        const instances = profile.instances || profile.Instances || [];
        const count = instances.length;
        
        if (count === 0) {
          return { regimeId, count, avgCagr: 0, avgSharpe: 0, avgSigma: 0, worstDrawdown: 0 };
        }
        
        let totalCagr = 0;
        let totalSharpe = 0;
        let totalSigma = 0;
        let worstDrawdown = 0;
        
        instances.forEach(inst => {
          const metrics = inst.metrics || inst.Metrics || {};
          totalCagr += metrics.cagr !== undefined ? metrics.cagr : (metrics.Cagr ?? 0);
          totalSharpe += metrics.sharpe !== undefined ? metrics.sharpe : (metrics.Sharpe ?? 0);
          totalSigma += metrics.sigma !== undefined ? metrics.sigma : (metrics.Sigma ?? 0);
          
          const dd = metrics.drawdown !== undefined ? metrics.drawdown : (metrics.Drawdown ?? 0);
          if (dd < worstDrawdown) {
            worstDrawdown = dd;
          }
        });
        
        return {
          regimeId,
          count,
          avgCagr: totalCagr / count,
          avgSharpe: totalSharpe / count,
          avgSigma: totalSigma / count,
          worstDrawdown
        };
      });
    };

    const activeResultObj = backtestResults
      ? (backtestResults[selectedResultVariantId] || Object.values(backtestResults)[0])
      : null;
    const summaries = activeResultObj
      ? computeRegimeSummaries(activeResultObj.regimeProfiles || activeResultObj.RegimeProfiles || {})
      : [];

    const activeStrat = strategies.find(s => s.id === activeConfigId) || strategies[0];
    const activeLegs = activeStrat ? activeStrat.legs : [];
    const hasAnyLegs = strategies.some(s => s.legs.length > 0);

    return (
      <div style={{ textAlign: 'left' }}>
        <button
          onClick={() => setBacktestModel(null)}
          style={{
            marginBottom: '24px',
            padding: '8px 16px',
            backgroundColor: '#f1f5f9',
            color: '#475569',
            border: '1px solid #cbd5e1',
            borderRadius: '6px',
            fontWeight: '600',
            cursor: 'pointer',
            transition: 'background-color 0.2s'
          }}
        >
          🔙 Back to Models
        </button>

        <div style={{ backgroundColor: '#f8fafc', border: '1px solid #cbd5e1', borderRadius: '8px', padding: '16px 24px', marginBottom: '32px' }}>
          <h3 style={{ fontSize: '18px', fontWeight: '700', color: '#1e293b', margin: '0 0 8px 0' }}>
            Backtesting Model: <span style={{ color: '#2563eb' }}>{backtestModel.modelName || backtestModel.ModelName}</span>
          </h3>
          <div style={{ display: 'flex', gap: '24px', fontSize: '13px', color: '#64748b' }}>
            <div><strong>Model ID:</strong> {backtestModel.id || backtestModel.Id}</div>
            <div><strong>Training Window:</strong> {formatDate(backtestModel.startDateTime || backtestModel.StartDateTime)} to {formatDate(backtestModel.endDateTime || backtestModel.EndDateTime)}</div>
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '32px' }}>
          {/* Left Column: Configuration */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '24px' }}>
            <div style={{ border: '1px solid #e2e8f0', borderRadius: '12px', padding: '24px', backgroundColor: '#ffffff', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.05)' }}>
              <h4 style={{ fontSize: '15px', fontWeight: '700', color: '#1e293b', marginBottom: '20px', borderBottom: '1px solid #f1f5f9', paddingBottom: '10px' }}>
                🔧 Multi-Leg Strategy Builder
              </h4>

              {/* Dynamic tabs for compared strategies */}
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px', marginBottom: '20px', borderBottom: '1px solid #f1f5f9', paddingBottom: '12px' }}>
                {strategies.map((strat, idx) => {
                  const isSelected = activeConfigId === strat.id;
                  return (
                    <div key={strat.id} style={{ display: 'flex', alignItems: 'center', gap: '4px', backgroundColor: isSelected ? '#eff6ff' : '#ffffff', border: '1px solid ' + (isSelected ? '#bfdbfe' : '#cbd5e1'), borderRadius: '6px', padding: '2px 8px' }}>
                      <button
                        type="button"
                        onClick={() => setActiveConfigId(strat.id)}
                        style={{
                          border: 'none',
                          backgroundColor: 'transparent',
                          fontWeight: '700',
                          fontSize: '12.5px',
                          color: isSelected ? '#2563eb' : '#64748b',
                          cursor: 'pointer',
                          padding: '6px'
                        }}
                      >
                        {strat.variantId || `Strategy #${idx + 1}`}
                      </button>
                      {strategies.length > 1 && (
                        <button
                          type="button"
                          onClick={() => removeStrategyConfig(strat.id)}
                          style={{
                            border: 'none',
                            background: 'none',
                            color: '#ef4444',
                            fontWeight: '700',
                            fontSize: '12px',
                            cursor: 'pointer',
                            padding: '0 4px'
                          }}
                          title="Remove from comparison"
                        >
                          ✕
                        </button>
                      )}
                    </div>
                  );
                })}
                <button
                  type="button"
                  onClick={addStrategyConfig}
                  style={{
                    padding: '8px 12px',
                    border: '1px dashed #2563eb',
                    borderRadius: '6px',
                    fontWeight: '600',
                    fontSize: '12.5px',
                    backgroundColor: '#ffffff',
                    color: '#2563eb',
                    cursor: 'pointer',
                    transition: 'all 0.2s'
                  }}
                >
                  ➕ Add Strategy
                </button>
              </div>

              {activeStrat && (
                <>
                  {/* Strategy Template Selector */}
                  <div style={{ marginBottom: '16px' }}>
                    <label style={{ fontSize: '13px', fontWeight: '600', color: '#475569', display: 'block', marginBottom: '6px' }}>
                      Options Strategy Template
                    </label>
                    <select
                      value={activeStrat.selectedStrategy}
                      onChange={(e) => handleStrategyTemplateChange(e.target.value)}
                      style={{ ...filterInputStyle, width: '100%' }}
                    >
                      {strategyMetadata?.strategies?.map(s => (
                        <option key={s.strategyName} value={s.strategyName}>{s.displayName}</option>
                      ))}
                    </select>
                  </div>

                  {/* Variant ID / Execution Name */}
                  <div style={{ marginBottom: '24px' }}>
                    <label style={{ fontSize: '13px', fontWeight: '600', color: '#475569', display: 'block', marginBottom: '6px' }}>
                      Backtest Execution Variant Name
                    </label>
                    <input
                      type="text"
                      placeholder="e.g. CoveredCall_Sweep"
                      value={activeStrat.variantId}
                      onChange={(e) => {
                        const val = e.target.value;
                        setStrategies(prev => prev.map(s => {
                          if (s.id !== activeConfigId) return s;
                          return { ...s, variantId: val };
                        }));
                      }}
                      style={{ ...filterInputStyle, width: '100%' }}
                    />
                  </div>

                  {/* Legs Header */}
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
                    <h5 style={{ fontSize: '13px', fontWeight: '700', color: '#334155', margin: 0 }}>
                      Strategy Position Legs ({activeLegs.length})
                    </h5>
                    <button
                      type="button"
                      onClick={addNewLeg}
                      style={{
                        padding: '4px 10px',
                        fontSize: '12px',
                        fontWeight: '600',
                        backgroundColor: '#10b981',
                        color: '#ffffff',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: 'pointer'
                      }}
                    >
                      ➕ Add Leg
                    </button>
                  </div>

                  {/* Legs List */}
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '12px', marginBottom: '24px' }}>
                    {activeLegs.map((leg, index) => (
                      <div key={leg.id} style={{ border: '1px solid #e2e8f0', borderRadius: '8px', padding: '16px', backgroundColor: '#f8fafc', position: 'relative' }}>
                        <button
                          type="button"
                          onClick={() => removeLeg(leg.id)}
                          style={{
                            position: 'absolute',
                            top: '12px',
                            right: '12px',
                            border: 'none',
                            background: 'none',
                            color: '#ef4444',
                            fontWeight: '600',
                            cursor: 'pointer',
                            fontSize: '12px'
                          }}
                        >
                          🗑️ Delete Leg
                        </button>
                        <div style={{ fontSize: '12px', fontWeight: '700', color: '#475569', marginBottom: '12px' }}>
                          Leg #{index + 1}
                        </div>

                        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(130px, 1fr))', gap: '12px' }}>
                          {strategyMetadata?.legParameters?.map(param => {
                            const pKey = param.key || param.Key;
                            const pLabel = param.label || param.Label;
                            const pType = param.inputType || param.InputType;
                            const pOptions = param.options || param.Options;

                            if (!isLegParamVisible(param, leg)) return null;

                            return (
                              <div key={pKey} style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                                <label style={{ fontSize: '11px', fontWeight: '600', color: '#64748b' }}>{pLabel}</label>
                                {pType === 'select' ? (
                                  <select
                                    value={String(leg[pKey] ?? '')}
                                    onChange={(e) => handleLegValueChange(leg.id, pKey, e.target.value, pType)}
                                    style={{ ...filterInputStyle, padding: '4px 8px', fontSize: '12px', width: '100%' }}
                                  >
                                    {pOptions?.map(opt => (
                                      <option key={opt} value={opt}>{opt}</option>
                                    ))}
                                  </select>
                                ) : (
                                  <input
                                    type="number"
                                    step="any"
                                    value={leg[pKey] ?? ''}
                                    onChange={(e) => handleLegValueChange(leg.id, pKey, e.target.value, pType)}
                                    style={{ ...filterInputStyle, padding: '4px 8px', fontSize: '12px', width: '100%' }}
                                  />
                                )}
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    ))}

                    {activeLegs.length === 0 && (
                      <div style={{ textAlign: 'center', padding: '24px', border: '1px dashed #cbd5e1', borderRadius: '8px', color: '#94a3b8', fontSize: '13px' }}>
                        No legs configured. Click "Add Leg" to start building your strategy options.
                      </div>
                    )}
                  </div>
                </>
              )}

              {/* Action Buttons */}
              <button
                type="button"
                onClick={handleRunBacktest}
                disabled={backtesting || !hasAnyLegs}
                style={{
                  width: '100%',
                  padding: '12px',
                  backgroundColor: backtesting ? '#94a3b8' : '#2563eb',
                  color: '#ffffff',
                  border: 'none',
                  borderRadius: '6px',
                  fontWeight: '700',
                  cursor: backtesting || !hasAnyLegs ? 'not-allowed' : 'pointer',
                  fontSize: '14px',
                  boxShadow: '0 4px 6px -1px rgba(37,99,235,0.2)'
                }}
              >
                {backtesting ? '⏳ Running Hypothesis Simulation...' : '🚀 Execute Hypothesis Backtest'}
              </button>
            </div>

            {/* Combined Option Legs Expiration Payoff Diagram */}
            {hasAnyLegs && (
              <PayoffDiagram 
                strategiesList={strategies} 
                activeConfigId={activeConfigId} 
              />
            )}
          </div>

          {/* Right Column: Results */}
          <div>
            <div style={{ border: '1px solid #e2e8f0', borderRadius: '12px', padding: '24px', backgroundColor: '#ffffff', minHeight: '400px', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.05)', display: 'flex', flexDirection: 'column' }}>
              <h4 style={{ fontSize: '15px', fontWeight: '700', color: '#1e293b', marginBottom: '20px', borderBottom: '1px solid #f1f5f9', paddingBottom: '10px' }}>
                📊 Backtest Performance Metrics
              </h4>

              {backtesting && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', flex: 1 }}>
                  <div style={spinnerStyle}></div>
                  <p style={{ marginTop: '16px', color: '#64748b', fontSize: '14px', fontWeight: '500' }}>
                    Simulating position lifecycles against options history database...
                  </p>
                </div>
              )}

              {backtestError && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '16px', backgroundColor: '#fff2f0', border: '1px solid #ffccc7', borderRadius: '8px', color: '#ff4d4f', fontSize: '13px' }}>
                  <span>⚠️</span>
                  <span>{backtestError}</span>
                </div>
              )}

              {!backtesting && !backtestError && !backtestResults && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', flex: 1, color: '#94a3b8', textAlign: 'center' }}>
                  <div style={{ fontSize: '48px', marginBottom: '12px' }}>📉</div>
                  <h5 style={{ fontSize: '14px', fontWeight: '600', color: '#64748b', margin: '0 0 6px 0' }}>Awaiting Execution</h5>
                  <p style={{ fontSize: '12px', maxWidth: '300px', margin: 0 }}>
                    Configure your multi-leg strategy parameters on the left and run the backtest to evaluate metrics.
                  </p>
                </div>
              )}

              {backtestResults && (() => {
                const selectedVariantResult = backtestResults[selectedResultVariantId] || Object.values(backtestResults)[0];
                if (!selectedVariantResult) {
                  return (
                    <div style={{ textAlign: 'center', color: '#94a3b8', padding: '24px' }}>
                      No backtesting variant results found.
                    </div>
                  );
                }

                const profiles = selectedVariantResult.regimeProfiles || selectedVariantResult.RegimeProfiles || {};
                const allInstances = [];
                Object.entries(profiles).forEach(([regimeId, profile]) => {
                  const insts = profile.instances || profile.Instances || [];
                  insts.forEach(inst => {
                    allInstances.push({
                      regimeId,
                      ...inst
                    });
                  });
                });
                
                // Sort by Instance ID
                allInstances.sort((a, b) => {
                  const idA = a.instanceId ?? a.InstanceId ?? 0;
                  const idB = b.instanceId ?? b.InstanceId ?? 0;
                  return idA - idB;
                });

                if (allInstances.length === 0) {
                  return (
                    <div style={{ textAlign: 'center', color: '#94a3b8', padding: '24px' }}>
                      No backtesting timeline instances detected for this regime shift window.
                    </div>
                  );
                }

                const calculateDays = (startStr, endStr) => {
                  if (!startStr || !endStr) return '—';
                  try {
                    const start = new Date(startStr);
                    const end = new Date(endStr);
                    const diffTime = Math.abs(end - start);
                    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
                    return isNaN(diffDays) ? '—' : diffDays;
                  } catch (e) {
                    return '—';
                  }
                };

                const keys = labeledData && labeledData.length > 0 ? Object.keys(labeledData[0]) : [];
                const variantKeys = keys.filter(k => 
                  k.toLowerCase() !== 'date' && 
                  k.toLowerCase() !== 'clusterlabel' &&
                  k.toLowerCase() !== 'instanceblock'
                );

                const resultsVariantKeys = Object.keys(backtestResults);
                const activeVariantId = selectedResultVariantId || resultsVariantKeys[0];

                return (
                  <>
                    {/* Variant switcher panel */}
                    <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '24px', backgroundColor: '#f8fafc', padding: '12px 16px', borderRadius: '8px', border: '1px solid #e2e8f0' }}>
                      <span style={{ fontSize: '13px', fontWeight: '700', color: '#475569' }}>Selected Variant Details:</span>
                      <div style={{ display: 'flex', gap: '8px' }}>
                        {resultsVariantKeys.map(vk => {
                          const isSelected = activeVariantId === vk;
                          return (
                            <button
                              key={vk}
                              type="button"
                              onClick={() => setSelectedResultVariantId(vk)}
                              style={{
                                padding: '6px 12px',
                                fontSize: '12px',
                                fontWeight: '700',
                                borderRadius: '4px',
                                border: isSelected ? '1px solid #2563eb' : '1px solid #cbd5e1',
                                backgroundColor: isSelected ? '#2563eb' : '#ffffff',
                                color: isSelected ? '#ffffff' : '#475569',
                                cursor: 'pointer',
                                transition: 'all 0.2s'
                              }}
                            >
                              {vk}
                            </button>
                          );
                        })}
                      </div>
                    </div>

                    {/* Regime Performance Summary Cards */}
                    {(() => {
                      const aggregates = selectedVariantResult.regimeAggregates || selectedVariantResult.RegimeAggregates || {};
                      if (Object.keys(aggregates).length === 0) return null;

                      const regimeDayCounts = {};
                      allInstances.forEach(inst => {
                        const rId = String(inst.regimeId);
                        const days = calculateDays(inst.startDate || inst.StartDate, inst.endDate || inst.EndDate);
                        const daysNum = typeof days === 'number' ? days : 0;
                        regimeDayCounts[rId] = (regimeDayCounts[rId] || 0) + daysNum;
                      });

                      return (
                        <div style={{ marginBottom: '24px' }}>
                          <h5 style={{ fontSize: '13px', fontWeight: '700', color: '#334155', marginBottom: '12px' }}>
                            🎯 Cumulative Regime Performance Summary (Averages & Significance)
                          </h5>
                          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '16px' }}>
                            {Object.entries(aggregates).map(([rId, metrics]) => {
                              const cagr = metrics.cagr !== undefined ? metrics.cagr : metrics.Cagr;
                              const sharpe = metrics.sharpe !== undefined ? metrics.sharpe : metrics.Sharpe;
                              const drawdown = metrics.drawdown !== undefined ? metrics.drawdown : metrics.Drawdown;
                              const sigma = metrics.sigma !== undefined ? metrics.sigma : metrics.Sigma;
                              const var95 = metrics.vaR_95 !== undefined ? metrics.vaR_95 : (metrics.VaR_95 !== undefined ? metrics.VaR_95 : metrics.var_95);
                              const cvar = metrics.cvar !== undefined ? metrics.cvar : (metrics.CVaR !== undefined ? metrics.CVaR : metrics.cvar_95);
                              const pValue = metrics.pValue !== undefined ? metrics.pValue : metrics.PValue;
                              const tValue = metrics.tValue !== undefined ? metrics.tValue : metrics.TValue;
                              
                              const totalDays = regimeDayCounts[String(rId)] || 0;
                              const isSignificant = pValue !== null && pValue !== undefined && pValue < 0.05;

                              const colors = ['#2563eb', '#10b981', '#f59e0b', '#ec4899', '#8b5cf6', '#06b6d4'];
                              const themeColor = colors[Number(rId) % colors.length];

                              return (
                                <div key={rId} style={{ border: `1px solid #e2e8f0`, borderTop: `4px solid ${themeColor}`, borderRadius: '8px', padding: '16px', backgroundColor: '#ffffff', boxShadow: '0 1px 3px rgba(0,0,0,0.05)' }}>
                                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
                                    <span style={{ fontSize: '13px', fontWeight: '700', color: themeColor }}>
                                      Regime #{rId}
                                    </span>
                                    <span style={{ fontSize: '11px', fontWeight: '600', color: '#64748b', backgroundColor: '#f1f5f9', padding: '2px 8px', borderRadius: '12px' }}>
                                      {totalDays} Active Days
                                    </span>
                                  </div>
                                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px 16px', fontSize: '11.5px', color: '#475569' }}>
                                    <div><strong>CAGR:</strong> <span style={{ color: cagr >= 0 ? '#10b981' : '#ef4444', fontWeight: '700' }}>{(cagr * 100).toFixed(2)}%</span></div>
                                    <div><strong>Sharpe Ratio:</strong> <span style={{ fontWeight: '700' }}>{sharpe !== undefined && sharpe !== null ? sharpe.toFixed(2) : '—'}</span></div>
                                    <div><strong>Max Drawdown:</strong> <span style={{ color: '#ef4444', fontWeight: '600' }}>{(drawdown * 100).toFixed(2)}%</span></div>
                                    <div><strong>Volatility (σ):</strong> <span>{(sigma * 100).toFixed(2)}%</span></div>
                                    <div><strong>95% VaR:</strong> <span>{(var95 * 100).toFixed(2)}%</span></div>
                                    <div><strong>95% CVaR:</strong> <span>{(cvar * 100).toFixed(2)}%</span></div>
                                    <div><strong>T-Statistic:</strong> <span>{tValue !== undefined && tValue !== null ? tValue.toFixed(4) : '—'}</span></div>
                                    <div>
                                      <strong>P-Value:</strong>{' '}
                                      <span style={{ fontWeight: isSignificant ? '700' : 'normal', color: isSignificant ? '#10b981' : '#475569' }}>
                                        {pValue !== undefined && pValue !== null ? pValue.toFixed(4) : '—'}
                                        {isSignificant && ' *'}
                                      </span>
                                    </div>
                                  </div>
                                </div>
                              );
                            })}
                          </div>
                        </div>
                      );
                    })()}

                    <ReturnsLineChart data={labeledData} variantKeys={variantKeys} instances={allInstances} />
                    <TimelineBoxPlotChart instances={allInstances} formatDate={formatDate} />
                    <div style={{ overflowX: 'auto', border: '1px solid #e2e8f0', borderRadius: '8px', marginTop: '10px' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '11px', textAlign: 'left', minWidth: '1350px' }}>
                      <thead>
                        <tr style={{ backgroundColor: '#f8fafc', borderBottom: '1px solid #e2e8f0' }}>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>Instance ID</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>Regime</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>Start Date</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>End Date</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569', textAlign: 'center' }}>Days</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>CAGR</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>Sharpe</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>Max DD</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>Sigma (σ)</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>95% VaR</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>95% CVaR</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>T-Stat</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>P-Value</th>
                          <th style={{ padding: '10px 12px', fontWeight: '700', color: '#475569' }}>Quartiles [Min, 25%, Med, 75%, Max]</th>
                        </tr>
                      </thead>
                      <tbody>
                        {allInstances.map(inst => {
                          const metrics = inst.metrics || inst.Metrics || {};
                          const cagr = metrics.cagr !== undefined ? metrics.cagr : metrics.Cagr;
                          const sharpe = metrics.sharpe !== undefined ? metrics.sharpe : metrics.Sharpe;
                          const drawdown = metrics.drawdown !== undefined ? metrics.drawdown : metrics.Drawdown;
                          const sigma = metrics.sigma !== undefined ? metrics.sigma : metrics.Sigma;
                          const var95 = metrics.vaR_95 !== undefined ? metrics.vaR_95 : (metrics.VaR_95 !== undefined ? metrics.VaR_95 : metrics.var_95);
                          const cvar = metrics.cvar !== undefined ? metrics.cvar : (metrics.CVaR !== undefined ? metrics.CVaR : metrics.cvar_95);
                          const pValue = metrics.pValue !== undefined ? metrics.pValue : metrics.PValue;
                          const tValue = metrics.tValue !== undefined ? metrics.tValue : metrics.TValue;
                          const quartiles = metrics.boxPlotQuartiles || metrics.BoxPlotQuartiles || [];

                          const days = calculateDays(inst.startDate || inst.StartDate, inst.endDate || inst.EndDate);

                          return (
                            <tr key={`${inst.regimeId}-${inst.instanceId || inst.InstanceId}`} style={{ borderBottom: '1px solid #f1f5f9' }}>
                              <td style={{ padding: '10px 12px', fontWeight: '600', color: '#1e293b' }}>
                                #{inst.instanceId || inst.InstanceId}
                              </td>
                              <td style={{ padding: '10px 12px' }}>
                                <span style={{ padding: '3px 8px', backgroundColor: '#e0f2fe', color: '#0369a1', borderRadius: '4px', fontSize: '9px', fontWeight: '700', textTransform: 'uppercase' }}>
                                  Regime {inst.regimeId}
                                </span>
                              </td>
                              <td style={{ padding: '10px 12px' }}>{formatDate(inst.startDate || inst.StartDate)}</td>
                              <td style={{ padding: '10px 12px' }}>{formatDate(inst.endDate || inst.EndDate)}</td>
                              <td style={{ padding: '10px 12px', textAlign: 'center', fontWeight: '600' }}>{days}</td>
                              <td style={{ padding: '10px 12px', color: (cagr ?? 0) >= 0 ? '#10b981' : '#ef4444', fontWeight: '600' }}>
                                {cagr !== undefined ? `${(cagr * 100).toFixed(2)}%` : '—'}
                              </td>
                              <td style={{ padding: '10px 12px', fontWeight: '600' }}>
                                {sharpe !== undefined ? sharpe.toFixed(2) : '—'}
                              </td>
                              <td style={{ padding: '10px 12px', color: '#ef4444' }}>
                                {drawdown !== undefined ? `${(drawdown * 100).toFixed(2)}%` : '—'}
                              </td>
                              <td style={{ padding: '10px 12px' }}>
                                {sigma !== undefined ? `${(sigma * 100).toFixed(2)}%` : '—'}
                              </td>
                              <td style={{ padding: '10px 12px', color: '#ef4444' }}>
                                {var95 !== undefined ? `${(var95 * 100).toFixed(2)}%` : '—'}
                              </td>
                              <td style={{ padding: '10px 12px', color: '#ef4444' }}>
                                {cvar !== undefined ? `${(cvar * 100).toFixed(2)}%` : '—'}
                              </td>
                              <td style={{ padding: '10px 12px' }}>
                                {tValue !== undefined && tValue !== null ? tValue.toFixed(4) : '—'}
                              </td>
                              <td style={{ padding: '10px 12px' }}>
                                {pValue !== undefined && pValue !== null ? (
                                  <span style={{ color: pValue < 0.05 ? '#166534' : '#475569', fontWeight: pValue < 0.05 ? '700' : '400' }}>
                                    {pValue.toFixed(4)}
                                    {pValue < 0.05 && ' *'}
                                  </span>
                                ) : '—'}
                              </td>
                              <td style={{ padding: '10px 12px', fontFamily: 'monospace', color: '#64748b' }}>
                                {quartiles.length === 5 
                                  ? `[${quartiles.map(q => `${(q * 100).toFixed(1)}%`).join(', ')}]`
                                  : '—'
                                }
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
                </>
              );
              })()}
            </div>
          </div>
        </div>
      </div>
    );
  };

  // Fetch available architectures on mount
  useEffect(() => {
    const fetchArchitectures = async () => {
      try {
        const data = await api.get('/api/ml-manager/get-model');
        setAvailableArchitectures(data || ['KMeansClustering']);
      } catch (err) {
        console.error("Failed to load architectures:", err);
        setAvailableArchitectures(['KMeansClustering']);
      }
    };
    fetchArchitectures();
  }, []);

  // Fetch feature blueprints and strategy parameters when modelName changes
  useEffect(() => {
    if (!modelName) {
      setFeatureBlueprints([]);
      setModelParameters([]);
      setSelectedFeature('');
      setParameterFilters({});
      setFeatureParamFilters({});
      setFeatureFilterBlocks([]);
      return;
    }

    const fetchFeaturesAndParams = async () => {
      try {
        const data = await api.get(`/api/ml-manager/get-features?modelName=${encodeURIComponent(modelName)}`);
        
        // Handles PascalCase vs camelCase fallbacks seamlessly
        const featuresList = data?.features || data?.Features;
        const parametersWrapper = data?.parameters || data?.Parameters;
        const targetParametersList = parametersWrapper?.parameters || parametersWrapper?.Parameters || [];

        const verifiedFeatures = Array.isArray(featuresList) 
          ? featuresList 
          : (Array.isArray(data) ? data : []);

        setFeatureBlueprints(verifiedFeatures);
        setModelParameters(targetParametersList);
        setSelectedFeature('');
        setFeatureParamFilters({});
        setFeatureFilterBlocks([]);
        
        // Pre-populate empty filters
        const initialFilters = {};
        targetParametersList.forEach(p => {
          const pKey = p.key || p.Key;
          if (pKey) initialFilters[pKey] = '';
        });
        setParameterFilters(initialFilters);
      } catch (err) {
        console.error("Failed to load details for " + modelName, err);
      }
    };

    fetchFeaturesAndParams();
  }, [modelName]);

  // Fetch models based on active filters
  const fetchTrainedModels = async (isInitial = false) => {
    setLoading(true);
    setError(null);
    try {
      let payload = {};
      if (isInitial) {
        payload = { status: 2 }; // Default to Trained
      } else {
        payload = {
          modelName: modelName || null,
          startDateTime: startDate ? new Date(startDate).toISOString() : null,
          endDateTime: endDate ? new Date(endDate).toISOString() : null,
          status: status !== '' ? Number(status) : 2,
          featuresPipeline: featureFilterBlocks.length > 0 ? featureFilterBlocks.map(block => ({
            featureCode: block.featureCode,
            parameters: Object.entries(block.configuredValues).reduce((acc, [key, val]) => {
              if (val !== '') {
                const activeFb = featureBlueprints.find(fb => (fb.featureCode || fb.FeatureCode) === block.featureCode);
                const activeFbParams = activeFb?.parameters || activeFb?.Parameters || [];
                const meta = activeFbParams.find(p => (p.name || p.Name) === key);
                const pType = meta?.type || meta?.Type;
                acc[key] = (pType === 'number' || pType === 'Integer') ? Number(val) : val;
              }
              return acc;
            }, {})
          })) : null,
          parameters: null
        };

        const activeParams = {};
        Object.entries(parameterFilters).forEach(([key, val]) => {
          if (val !== '') {
            const meta = modelParameters.find(p => (p.key || p.Key) === key);
            const inputType = meta?.inputType || meta?.InputType;
            const parsedVal = (inputType === 'Integer' || inputType === 'number') ? Number(val) : val;
            activeParams[key] = parsedVal;
          }
        });

        if (Object.keys(activeParams).length > 0) {
          payload.parameters = activeParams;
        }
      }

      const data = await api.post('/api/ml-manager/trained-models', payload);
      setModels(data || []);
    } catch (err) {
      setError("Failed to retrieve trained models from the database.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Run initial fetch on mount
  useEffect(() => {
    fetchTrainedModels(true);
  }, []);

  const handleFilterSubmit = (e) => {
    e.preventDefault();
    fetchTrainedModels(false);
  };

  const handleClearFilters = () => {
    setModelName('');
    setStartDate('');
    setEndDate('');
    setStatus('2'); // Reset to Trained
    setSelectedFeature('');
    setParameterFilters({});
    setFeatureParamFilters({});
    setFeatureFilterBlocks([]);
    
    setLoading(true);
    api.post('/api/ml-manager/trained-models', { status: 2 })
      .then(data => setModels(data || []))
      .catch(() => setError("Failed to retrieve trained models."))
      .finally(() => setLoading(false));
  };

  const handleParamChange = (key, value) => {
    setParameterFilters(prev => ({
      ...prev,
      [key]: value
    }));
  };

  const handleFeatureParamChange = (key, value) => {
    setFeatureParamFilters(prev => ({
      ...prev,
      [key]: value
    }));
  };

  // Helper to format dates nicely
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    try {
      const d = new Date(dateString);
      return d.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
    } catch {
      return dateString;
    }
  };

  // Status badge styling helper
  const getStatusBadge = (statusValue) => {
    switch (statusValue) {
      case 0:
        return { text: 'Pending', color: '#64748b', bg: '#f1f5f9', border: '#cbd5e1' };
      case 1:
        return { text: 'In Progress', color: '#d97706', bg: '#fef3c7', border: '#fcd34d' };
      case 2:
        return { text: 'Trained', color: '#16a34a', bg: '#dcfce7', border: '#bbf7d0' };
      case 3:
        return { text: 'Failed', color: '#dc2626', bg: '#fee2e2', border: '#fecaca' };
      default:
        return { text: 'Unknown', color: '#94a3b8', bg: '#f8fafc', border: '#e2e8f0' };
    }
  };

  // Helper to parse and render JSON fields nicely (Features / Parameters / Metrics)
  const renderJsonField = (jsonString) => {
    if (!jsonString) return <span style={{ color: '#94a3b8' }}>None</span>;
    try {
      const parsed = JSON.parse(jsonString);
      if (typeof parsed === 'object') {
        const entries = Object.entries(parsed);
        if (entries.length === 0) return <span style={{ color: '#94a3b8' }}>Empty</span>;
        
        // If it contains a pipeline array (like FeaturesPipeline or featuresPipeline)
        const pipeline = parsed.FeaturesPipeline || parsed.featuresPipeline;
        if (Array.isArray(pipeline)) {
          return (
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
              {pipeline.map((p, idx) => (
                <span key={idx} style={jsonBadgeStyle}>
                  {p.featureCode || p.FeatureCode}
                </span>
              ))}
            </div>
          );
        }

        return (
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px', maxWidth: '280px' }}>
            {entries.slice(0, 4).map(([k, v]) => (
              <span key={k} style={jsonBadgeStyle} title={`${k}: ${JSON.stringify(v)}`}>
                <strong>{k}</strong>: {typeof v === 'object' ? '...' : String(v)}
              </span>
            ))}
            {entries.length > 4 && <span style={jsonBadgeMoreStyle}>+{entries.length - 4} more</span>}
          </div>
        );
      }
      return <span style={textTruncateStyle}>{jsonString}</span>;
    } catch {
      return <span style={textTruncateStyle}>{jsonString}</span>;
    }
  };

  // Helper to parse and render pipeline features with all active parameters shown clearly
  const renderFeaturesField = (jsonString) => {
    if (!jsonString) return <span style={{ color: '#94a3b8' }}>None</span>;
    try {
      const parsed = JSON.parse(jsonString);
      if (typeof parsed !== 'object' || parsed === null) {
        return <span style={textTruncateStyle}>{jsonString}</span>;
      }

      const callPutList = parsed.CallPutSpreadFeatures || parsed.callPutSpreadFeatures || [];
      const movingAvgList = parsed.MovingAverageFeatures || parsed.movingAverageFeatures || [];

      const activeFeatures = [];

      callPutList.forEach(item => {
        activeFeatures.push({
          name: item.FeatureName || item.featureName || 'Call-Put Spread',
          params: Object.entries(item).filter(([k]) => k !== 'FeatureName' && k !== 'featureName')
        });
      });

      movingAvgList.forEach(item => {
        activeFeatures.push({
          name: item.FeatureName || item.featureName || 'Moving Average',
          params: Object.entries(item).filter(([k]) => k !== 'FeatureName' && k !== 'featureName')
        });
      });

      if (activeFeatures.length === 0) {
        return <span style={{ color: '#94a3b8' }}>No Active Features</span>;
      }

      return (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
          {activeFeatures.map((feat, idx) => {
            const tooltip = feat.params.map(([k, v]) => `${k}: ${v}`).join('\n');
            return (
              <div key={idx} style={{ border: '1px solid #e2e8f0', borderRadius: '6px', padding: '6px 10px', backgroundColor: '#f8fafc', minWidth: '180px' }} title={tooltip}>
                <div style={{ fontWeight: '600', fontSize: '12px', color: '#1e293b', marginBottom: '4px' }}>
                  {feat.name}
                </div>
                {feat.params.length > 0 && (
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: '4px' }}>
                    {feat.params.map(([k, v]) => {
                      if (v === '' || v === null || v === undefined) return null;
                      return (
                        <span key={k} style={{ fontSize: '10px', backgroundColor: '#e2e8f0', color: '#475569', padding: '2px 6px', borderRadius: '4px', fontWeight: '500' }}>
                          <strong>{k}</strong>: {String(v)}
                        </span>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      );
    } catch {
      return <span style={textTruncateStyle}>{jsonString}</span>;
    }
  };

  const activeFeatureBlueprint = featureBlueprints.find(fb => (fb.featureCode || fb.FeatureCode) === selectedFeature);
  const featureParams = activeFeatureBlueprint?.parameters || activeFeatureBlueprint?.Parameters || [];

  const isFeatureParamVisible = (param, currentFilters) => {
    const visibleIfProp = param.visibleIfProperty || param.VisibleIfProperty;
    if (!visibleIfProp) return true;
    
    const visibleIfValues = (param.visibleIfValues || param.VisibleIfValues || []).map(v => v.toLowerCase());
    const parentVal = (currentFilters[visibleIfProp] || '').toLowerCase();
    
    if (visibleIfValues.length === 0) {
      return parentVal !== '';
    }
    return visibleIfValues.includes(parentVal);
  };

  if (backtestModel) {
    return (
      <div style={commonStyles.appContainer}>
        <div style={commonStyles.surface}>
          {renderBacktestWorkspace()}
        </div>
      </div>
    );
  }

  return (
    <div style={commonStyles.appContainer}>
      <div style={commonStyles.surface}>
        {/* Header Section */}
        <div style={{ fontSize: '48px', marginBottom: '16px' }}>🧪</div>
        <h2 style={{ marginBottom: '8px', color: '#1e293b' }}>Hypothesis Testing Workspace</h2>
        <p style={{ color: '#64748b', marginBottom: '32px', fontSize: '15px', lineHeight: '1.5' }}>
          Query, filter, and inspect trained machine learning models to run options pricing backtests and evaluate model performance.
        </p>

        {/* Horizontal Filters Panel */}
        <form onSubmit={handleFilterSubmit} style={filterPanelStyle}>
          <div style={filterGridStyle}>
            {/* Architecture Selector */}
            <div style={filterGroupStyle}>
              <label style={filterLabelStyle}>Model Name</label>
              <select
                value={modelName}
                onChange={(e) => setModelName(e.target.value)}
                style={filterInputStyle}
              >
                <option value="">-- All Architectures --</option>
                {availableArchitectures.map(arch => (
                  <option key={arch} value={arch}>{arch}</option>
                ))}
              </select>
            </div>

            {/* Start Date */}
            <div style={filterGroupStyle}>
              <label style={filterLabelStyle}>Start Date</label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                style={filterInputStyle}
              />
            </div>

            {/* End Date */}
            <div style={filterGroupStyle}>
              <label style={filterLabelStyle}>End Date</label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                style={filterInputStyle}
              />
            </div>

            {/* Status Selector */}
            <div style={filterGroupStyle}>
              <label style={filterLabelStyle}>Training Status</label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                style={filterInputStyle}
              >
                <option value="">-- All Statuses --</option>
                <option value="0">Pending</option>
                <option value="1">In Progress</option>
                <option value="2">Trained</option>
                <option value="3">Failed</option>
              </select>
            </div>

            {/* Target Feature Dropdown and Add Button */}
            <div style={filterGroupStyle}>
              <label style={filterLabelStyle}>Pipeline Feature to Filter</label>
              <div style={{ display: 'flex', gap: '8px' }}>
                <select
                  value={selectedFeature}
                  onChange={(e) => setSelectedFeature(e.target.value)}
                  disabled={!modelName || featureBlueprints.length === 0}
                  style={{ ...filterInputStyle, flex: 1 }}
                >
                  <option value="">{modelName ? '-- Select Feature --' : 'Select Architecture First'}</option>
                  {featureBlueprints.map(fb => {
                    const code = fb.featureCode || fb.FeatureCode;
                    const name = fb.displayName || fb.DisplayName;
                    return <option key={code} value={code}>{name}</option>;
                  })}
                </select>
                <button
                  type="button"
                  onClick={addFeatureFilterBlock}
                  disabled={!selectedFeature}
                  style={{
                    padding: '8px 16px',
                    backgroundColor: selectedFeature ? '#3b82f6' : '#cbd5e1',
                    color: '#ffffff',
                    border: 'none',
                    borderRadius: '6px',
                    fontWeight: '600',
                    cursor: selectedFeature ? 'pointer' : 'not-allowed',
                    transition: 'background-color 0.2s',
                    fontSize: '13px'
                  }}
                >
                  ➕ Add
                </button>
              </div>
            </div>

            {/* Configured Feature Filters Pipeline List */}
            {featureFilterBlocks.length > 0 && (
              <div style={{ gridColumn: '1 / -1', marginTop: '16px', display: 'flex', flexDirection: 'column', gap: '16px' }}>
                <h4 style={{ fontSize: '14px', fontWeight: '600', color: '#475569', margin: '0 0 4px 0' }}>
                  Active Feature Filters Pipeline:
                </h4>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
                  {featureFilterBlocks.map(block => (
                    <div key={block.id} style={{ border: '1px solid #e2e8f0', borderRadius: '8px', padding: '16px', backgroundColor: '#f8fafc', position: 'relative' }}>
                      <button
                        type="button"
                        onClick={() => removeFeatureFilterBlock(block.id)}
                        style={{
                          position: 'absolute',
                          top: '12px',
                          right: '12px',
                          border: 'none',
                          background: 'none',
                          color: '#ef4444',
                          fontWeight: '600',
                          cursor: 'pointer',
                          fontSize: '12px'
                        }}
                      >
                        🗑️ Remove Filter
                      </button>
                      <div style={{ fontWeight: '600', fontSize: '13px', color: '#1e293b', marginBottom: '12px' }}>
                        {block.displayName} Filter Block
                      </div>
                      
                      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px' }}>
                        {(() => {
                          const blueprint = featureBlueprints.find(fb => (fb.featureCode || fb.FeatureCode) === block.featureCode);
                          const params = blueprint?.parameters || blueprint?.Parameters || [];
                          return params.map(param => {
                            const pName = param.name || param.Name;
                            const pLabel = param.label || param.Label;
                            const pType = param.type || param.Type;
                            const pOptions = param.options || param.Options;

                            if (!isFeatureParamVisible(param, block.configuredValues)) return null;

                            return (
                              <div key={pName} style={{ display: 'flex', flexDirection: 'column', gap: '4px', minWidth: '180px' }}>
                                <label style={{ fontSize: '12px', fontWeight: '500', color: '#64748b' }}>{pLabel}</label>
                                {pType === 'select' ? (
                                  <select
                                    value={block.configuredValues[pName] || ''}
                                    onChange={(e) => handleFeatureBlockValueChange(block.id, pName, e.target.value, pType)}
                                    style={{ ...filterInputStyle, padding: '6px 8px', fontSize: '13px' }}
                                  >
                                    <option value="">-- All --</option>
                                    {pOptions?.map(opt => (
                                      <option key={opt} value={opt}>{opt}</option>
                                    ))}
                                  </select>
                                ) : (
                                  <input
                                    type="number"
                                    placeholder="Filter value..."
                                    value={block.configuredValues[pName] ?? ''}
                                    onChange={(e) => handleFeatureBlockValueChange(block.id, pName, e.target.value, pType)}
                                    style={{ ...filterInputStyle, padding: '6px 8px', fontSize: '13px' }}
                                  />
                                )}
                              </div>
                            );
                          });
                        })()}
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Dynamic Strategy Hyperparameters Inputs */}
            {modelName && modelParameters.length > 0 && (
              modelParameters.map(param => {
                const pKey = param.key || param.Key;
                const pLabel = param.label || param.Label;
                const pInputType = param.inputType || param.InputType;
                const pMin = param.minvalue || param.Minvalue;

                return (
                  <div key={pKey} style={filterGroupStyle}>
                    <label style={filterLabelStyle}>{pLabel}</label>
                    <input
                      type={pInputType === 'Integer' ? 'number' : 'text'}
                      min={pMin ?? undefined}
                      placeholder="Filter value..."
                      value={parameterFilters[pKey] ?? ''}
                      onChange={(e) => handleParamChange(pKey, e.target.value)}
                      style={filterInputStyle}
                    />
                  </div>
                );
              })
            )}
          </div>

          <div style={filterActionsStyle}>
            <button type="submit" disabled={loading} style={filterButtonStyle}>
              🔍 Filter Models
            </button>
            <button type="button" onClick={handleClearFilters} disabled={loading} style={clearButtonStyle}>
              Clear Filters
            </button>
          </div>
        </form>

        {/* Error Notification */}
        {error && (
          <div style={errorStyle}>
            ⚠️ {error}
          </div>
        )}

        {/* Models Results List / Grid */}
        {loading ? (
          <div style={loadingContainerStyle}>
            <div style={spinnerStyle}></div>
            <p style={{ color: '#64748b', marginTop: '12px', fontSize: '14px' }}>Loading workspace records...</p>
          </div>
        ) : models.length > 0 ? (
          <div style={tableWrapperStyle}>
            <table style={tableStyle}>
              <thead>
                <tr style={tableHeaderRowStyle}>
                  <th style={tableHeaderStyle}>Model / Architecture</th>
                  <th style={tableHeaderStyle}>Training Window</th>
                  <th style={tableHeaderStyle}>Status</th>
                  <th style={tableHeaderStyle}>Features Pipeline</th>
                  <th style={tableHeaderStyle}>Hyperparameters</th>
                  <th style={tableHeaderStyle}>Metrics</th>
                  <th style={tableHeaderStyle}>Failure Reason</th>
                  <th style={tableHeaderStyle}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {models.map((model) => {
                  const badge = getStatusBadge(model.status !== undefined ? model.status : model.Status);
                  return (
                    <tr key={model.id || model.Id} style={tableRowStyle}>
                      <td style={tableCellBoldStyle}>
                        {model.modelName || model.ModelName}
                      </td>
                      <td style={tableCellStyle}>
                        <div style={{ fontSize: '13px', fontWeight: '500' }}>
                          {formatDate(model.startDateTime || model.StartDateTime)}
                        </div>
                        <div style={{ fontSize: '11px', color: '#94a3b8' }}>
                          to {formatDate(model.endDateTime || model.EndDateTime)}
                        </div>
                      </td>
                      <td style={tableCellStyle}>
                        <span style={{
                          display: 'inline-block',
                          padding: '4px 10px',
                          borderRadius: '12px',
                          fontSize: '12px',
                          fontWeight: '600',
                          color: badge.color,
                          backgroundColor: badge.bg,
                          border: `1px solid ${badge.border}`
                        }}>
                          {badge.text}
                        </span>
                      </td>
                      <td style={tableCellStyle}>
                        {renderFeaturesField(model.features || model.Features)}
                      </td>
                      <td style={tableCellStyle}>
                        {renderJsonField(model.parameters || model.Parameters)}
                      </td>
                      <td style={tableCellStyle}>
                        {renderJsonField(model.modelMetrics || model.ModelMetrics)}
                      </td>
                      <td style={tableCellStyle}>
                        {(model.status !== undefined ? model.status : model.Status) === 3 ? (
                          <span style={{ color: '#dc2626', fontWeight: '500', fontSize: '12px' }} title={model.failureReason || model.FailureReason || 'Unknown error'}>
                            {model.failureReason || model.FailureReason || 'Unknown error'}
                          </span>
                        ) : (
                          <span style={{ color: '#cbd5e1' }}>—</span>
                        )}
                      </td>
                      <td style={tableCellStyle}>
                        <button
                          disabled={(model.status !== undefined ? model.status : model.Status) !== 2}
                          style={{
                            padding: '6px 12px',
                            fontSize: '12px',
                            fontWeight: '600',
                            borderRadius: '6px',
                            border: 'none',
                            color: '#ffffff',
                            backgroundColor: (model.status !== undefined ? model.status : model.Status) === 2 ? '#3b82f6' : '#94a3b8',
                            cursor: (model.status !== undefined ? model.status : model.Status) === 2 ? 'pointer' : 'not-allowed',
                            transition: 'background-color 0.2s'
                          }}
                          onClick={() => setBacktestModel(model)}
                        >
                          🧪 Backtest
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
            <div style={footerCounterStyle}>
              Showing <strong>{models.length}</strong> model execution record(s) matching current criteria.
            </div>
          </div>
        ) : (
          <div style={emptyStateStyle}>
            <div style={{ fontSize: '32px', marginBottom: '8px' }}>📂</div>
            <h4>No Trained Models Found</h4>
            <p>No model executions match your query in the database. Try adjusting your dates, architecture, or keyword filters.</p>
          </div>
        )}
      </div>
    </div>
  );
};

// --- STYLING SYSTEMS ---
const filterPanelStyle = {
  marginTop: '24px',
  marginBottom: '32px',
  padding: '24px',
  borderRadius: '8px',
  backgroundColor: '#f8fafc',
  border: '1px solid #e2e8f0',
  textAlign: 'left'
};

const filterGridStyle = {
  display: 'grid',
  gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
  gap: '16px',
  marginBottom: '20px'
};

const filterGroupStyle = {
  display: 'flex',
  flexDirection: 'column',
  gap: '6px'
};

const filterLabelStyle = {
  fontSize: '12px',
  fontWeight: '600',
  color: '#475569'
};

const filterInputStyle = {
  padding: '9px 12px',
  borderRadius: '6px',
  border: '1px solid #cbd5e1',
  fontSize: '13px',
  outline: 'none',
  backgroundColor: '#ffffff',
  color: '#1e293b',
  width: '100%',
  boxSizing: 'border-box'
};

const filterActionsStyle = {
  display: 'flex',
  justifyContent: 'flex-end',
  gap: '12px',
  borderTop: '1px solid #e2e8f0',
  paddingTop: '16px'
};

const filterButtonStyle = {
  padding: '10px 20px',
  fontSize: '13px',
  fontWeight: '600',
  color: '#ffffff',
  backgroundColor: '#3b82f6',
  border: 'none',
  borderRadius: '6px',
  cursor: 'pointer',
  transition: 'background-color 0.15s ease'
};

const clearButtonStyle = {
  padding: '10px 20px',
  fontSize: '13px',
  fontWeight: '500',
  color: '#475569',
  backgroundColor: '#ffffff',
  border: '1px solid #cbd5e1',
  borderRadius: '6px',
  cursor: 'pointer',
  transition: 'all 0.15s ease'
};

const errorStyle = {
  backgroundColor: '#fff2f0',
  border: '1px solid #ffccc7',
  color: '#ff4d4f',
  padding: '14px',
  borderRadius: '6px',
  marginBottom: '24px',
  textAlign: 'left',
  fontSize: '14px',
  fontWeight: '500'
};

const loadingContainerStyle = {
  padding: '48px 0',
  textAlign: 'center'
};

const spinnerStyle = {
  width: '36px',
  height: '36px',
  border: '4px solid #f3f3f3',
  borderTop: '4px solid #3b82f6',
  borderRadius: '50%',
  animation: 'spin 1s linear infinite',
  margin: '0 auto'
};

// Inline animations keyframes helper injected via styled system
const styleInject = document.createElement('style');
styleInject.innerHTML = `@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }`;
document.head.appendChild(styleInject);

const tableWrapperStyle = {
  overflowX: 'auto',
  borderRadius: '8px',
  border: '1px solid #e2e8f0',
  backgroundColor: '#ffffff'
};

const tableStyle = {
  width: '100%',
  borderCollapse: 'collapse',
  textAlign: 'left'
};

const tableHeaderRowStyle = {
  backgroundColor: '#f8fafc',
  borderBottom: '1px solid #e2e8f0'
};

const tableHeaderStyle = {
  padding: '14px 16px',
  fontSize: '12px',
  fontWeight: '700',
  color: '#475569',
  textTransform: 'uppercase',
  letterSpacing: '0.5px'
};

const tableRowStyle = {
  borderBottom: '1px solid #f1f5f9',
  transition: 'background-color 0.15s'
};

const tableCellStyle = {
  padding: '14px 16px',
  fontSize: '13px',
  color: '#334155',
  verticalAlign: 'middle'
};

const tableCellBoldStyle = {
  ...tableCellStyle,
  fontWeight: '600',
  color: '#0f172a'
};

const jsonBadgeStyle = {
  fontSize: '11px',
  backgroundColor: '#f1f5f9',
  color: '#475569',
  padding: '2px 6px',
  borderRadius: '4px',
  border: '1px solid #e2e8f0',
  fontFamily: 'monospace'
};

const jsonBadgeMoreStyle = {
  fontSize: '11px',
  color: '#94a3b8',
  padding: '2px 4px',
  fontStyle: 'italic'
};

const textTruncateStyle = {
  display: 'inline-block',
  maxWidth: '180px',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap'
};

const emptyStateStyle = {
  padding: '48px 24px',
  color: '#64748b',
  border: '1px dashed #cbd5e1',
  backgroundColor: '#f8fafc',
  borderRadius: '8px',
  textAlign: 'center'
};

const footerCounterStyle = {
  padding: '12px 16px',
  fontSize: '12px',
  color: '#64748b',
  borderTop: '1px solid #e2e8f0',
  backgroundColor: '#f8fafc',
  textAlign: 'left'
};

export default HypothesisTesting;
