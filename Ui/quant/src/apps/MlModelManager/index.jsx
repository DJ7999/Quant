import React, { useState, useEffect } from 'react';
import { theme } from '../../core/theme';
import { api } from '../../core/api'; 
import { commonStyles } from '../../components/common';

const MlModelManager = () => {
  const [models, setModels] = useState([]);
  const [loading, setLoading] = useState(false);
  const [featuresLoading, setFeaturesLoading] = useState(false);
  const [error, setError] = useState(null);
  const [selectedModel, setSelectedModel] = useState('');
  
  // 1. TOTAL SPAN WINDOW BOUNDS ('YYYY-MM')
  const [totalWindowStart, setTotalWindowStart] = useState('');
  const [totalWindowEnd, setTotalWindowEnd] = useState('');
  
  // 2. ROLLING ITERATION PARAMETERS
  const [trainingWindowSize, setTrainingWindowSize] = useState('24'); // Value tracked in months (e.g., 24 months = 2 years)
  const [retrainFrequency, setRetrainFrequency] = useState('Monthly');

  // Blueprint and Workspace layout pipelines
  const [featureBlueprints, setFeatureBlueprints] = useState([]);
  const [activeWorkspaceBlocks, setActiveWorkspaceBlocks] = useState([]);
  const [training, setTraining] = useState(false);

  const handleDropdownClick = async () => {
    if (loading || models.length > 0) return;
    setLoading(true);
    setError(null);
    try {
      const data = await api.get('/api/ml-manager/get-model');
      setModels(data || []);
    } catch (err) {
      setError("Failed to load models");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!selectedModel) {
      setFeatureBlueprints([]);
      setActiveWorkspaceBlocks([]);
      setTotalWindowStart('');
      setTotalWindowEnd('');
      setTrainingWindowSize('24');
      setRetrainFrequency('Monthly');
      return;
    }

    const fetchFeatures = async () => {
      setFeaturesLoading(true);
      setError(null);
      try {
        const data = await api.get(`/api/ml-manager/get-features?modelName=${selectedModel}`);
        setFeatureBlueprints(data || []);
        setActiveWorkspaceBlocks([]); 
      } catch (err) {
        setError("Failed to load feature schemas.");
        console.error(err);
      } finally {
        setFeaturesLoading(false);
      }
    };

    fetchFeatures();
  }, [selectedModel]);

  const addFeatureInstance = (blueprint) => {
    const defaultValues = {};
    blueprint.parameters.forEach(p => {
      defaultValues[p.name] = p.defaultValue ?? '';
    });

    const newBlockInstance = {
      id: `instance-${Date.now()}-${Math.random().toString(36).substr(2, 4)}`,
      featureCode: blueprint.featureCode,
      displayName: blueprint.displayName,
      configuredValues: defaultValues
    };

    setActiveWorkspaceBlocks(prev => [...prev, newBlockInstance]);
  };

  const removeFeatureInstance = (instanceId) => {
    setActiveWorkspaceBlocks(prev => prev.filter(b => b.id !== instanceId));
  };

  const handleInstanceValueChange = (instanceId, paramName, value, type) => {
    const parsedValue = type === 'number' && value !== '' ? Number(value) : value;

    setActiveWorkspaceBlocks(prev => prev.map(block => {
      if (block.id !== instanceId) return block;
      return {
        ...block,
        configuredValues: { ...block.configuredValues, [paramName]: parsedValue }
      };
    }));
  };

  const isParamVisible = (block, param) => {
    if (!param.visibleIfProperty) return true;
    const currentParentValue = block.configuredValues[param.visibleIfProperty.toLowerCase()];
    if (!currentParentValue) return false;

    if (param.visibleIfValues && param.visibleIfValues.length > 0) {
      return param.visibleIfValues.map(v => v.toLowerCase()).includes(String(currentParentValue).toLowerCase());
    }
    return currentParentValue !== '';
  };

  // Maps "YYYY-MM" cleanly into explicit C# timestamp endpoints
  const formatToDateTimeString = (yearMonthString, isEndOfMonth) => {
    if (!yearMonthString) return null;
    return isEndOfMonth ? `${yearMonthString}-28T23:59:59Z` : `${yearMonthString}-01T00:00:00Z`;
  };

  const handleTrainPipelineSubmit = async () => {
    if (activeWorkspaceBlocks.length === 0) return;
    
    // Validations
    if (!totalWindowStart || !totalWindowEnd) {
      setError("Please specify the absolute Total Window bounds (Start and End parameters).");
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }
    if (new Date(totalWindowStart) > new Date(totalWindowEnd)) {
      setError("Total window configuration mismatch: Start boundary cannot step past End boundary.");
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }
    if (!trainingWindowSize || Number(trainingWindowSize) <= 0) {
      setError("Please enter a valid slide model Training Window Size parameter.");
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    setTraining(true);
    setError(null);

    const payload = {
      uniqueCustomName: `${selectedModel}_roll_${trainingWindowSize}M_by_${retrainFrequency}`,
      modelName: selectedModel,
      totalWindowStart: formatToDateTimeString(totalWindowStart, false),
      totalWindowEnd: formatToDateTimeString(totalWindowEnd, true),
      trainingWindowSizeMonths: Number(trainingWindowSize),
      retrainFrequency: retrainFrequency,
      featuresPipeline: activeWorkspaceBlocks.map(block => ({
        featureCode: block.featureCode,
        parameters: block.configuredValues 
      }))
    };

    try {
      const response = await api.post('/api/ml-manager/train-model', payload);
      alert(response?.message || "Rolling model cross-validation backtest training initialized!");
    } catch (err) {
      setError("Failed to execute remote ML pipeline rolling training run.");
      console.error(err);
    } finally {
      setTraining(false);
    }
  };

  return (
    <div style={commonStyles.appContainer}>
      <div style={commonStyles.surface}>
        <div style={{ fontSize: '48px', marginBottom: '16px' }}>🤖</div>
        <h2 style={{ marginBottom: '8px' }}>ML Model Pipeline Builder</h2>
        <p style={{ color: '#666', marginBottom: '32px' }}>
          Select an architecture, construct rolling window simulation constraints, and stack data generation features.
        </p>

        {error && (
          <div style={{ backgroundColor: '#fff2f0', border: '1px solid #ffccc7', color: '#ff4d4f', padding: '12px', borderRadius: '4px', marginBottom: '24px', textAlign: 'left', fontSize: '14px' }}>
            ⚠️ {error}
          </div>
        )}

        {/* Model Dropdown Selection Area */}
        <div style={containerStyle}>
          <label style={labelStyle}>Active Model Architecture</label>
          <div style={inputGroupStyle}>
            <select
              value={selectedModel}
              onFocus={handleDropdownClick}
              onChange={(e) => setSelectedModel(e.target.value)}
              disabled={loading || training}
              style={{ ...dropdownStyle, backgroundColor: '#fff', color: '#000' }}
            >
              <option value="">{loading ? 'Loading...' : '-- Select Architecture --'}</option>
              {models.map(m => <option key={m} value={m}>{m}</option>)}
            </select>
          </div>
        </div>

        {/* ROLLING RUN AND TIME SEGMENTATION METADATA PARAMETERS SECTION */}
        {selectedModel && (
          <div style={settingsPanelStyle}>
            <span style={{ ...labelStyle, display: 'block', marginBottom: '16px', color: '#000' }}>
              Time Span &amp; Rolling Optimization Parameters
            </span>
            <div style={settingsGridStyle}>
              
              {/* Block 1: Total Duration Window Bounds */}
              <div style={settingFieldGroupStyle}>
                <label style={settingFieldLabelStyle}>Total Window Start (Month/Year)</label>
                <input 
                  type="month" 
                  value={totalWindowStart}
                  onChange={(e) => setTotalWindowStart(e.target.value)}
                  disabled={training}
                  style={settingInputStyle}
                />
              </div>

              <div style={settingFieldGroupStyle}>
                <label style={settingFieldLabelStyle}>Total Window End (Month/Year)</label>
                <input 
                  type="month" 
                  value={totalWindowEnd}
                  onChange={(e) => setTotalWindowEnd(e.target.value)}
                  disabled={training}
                  style={settingInputStyle}
                />
              </div>

              {/* Block 2: Sub-Window Iteration Dynamics */}
              <div style={settingFieldGroupStyle}>
                <label style={settingFieldLabelStyle}>Training Window Size (In Months)</label>
                <input 
                  type="number"
                  min="1"
                  placeholder="e.g. 24"
                  value={trainingWindowSize}
                  onChange={(e) => setTrainingWindowSize(e.target.value)}
                  disabled={training}
                  style={settingInputStyle}
                />
              </div>

              <div style={settingFieldGroupStyle}>
                <label style={settingFieldLabelStyle}>Retraining Frequency</label>
                <select
                  value={retrainFrequency}
                  onChange={(e) => setRetrainFrequency(e.target.value)}
                  disabled={training}
                  style={settingInputStyle}
                >
                  <option value="Manual">Manual (Single Fit Iteration)</option>
                  <option value="Daily">Daily Rolling Step</option>
                  <option value="Weekly">Weekly Rolling Step</option>
                  <option value="Monthly">Monthly Rolling Step</option>
                  <option value="Quarterly">Quarterly Rolling Step</option>
                </select>
              </div>

            </div>
          </div>
        )}

        {/* TOOLBAR PANEL */}
        {selectedModel && featureBlueprints.length > 0 && (
          <div style={toolbarStyle}>
            <span style={{ ...labelStyle, display: 'block', marginBottom: '8px' }}>Available Feature Building Blocks</span>
            <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap' }}>
              {featureBlueprints.map(blueprint => (
                <button
                  key={blueprint.featureCode}
                  onClick={() => addFeatureInstance(blueprint)}
                  disabled={training}
                  style={{ ...addBlockButtonStyle, opacity: training ? 0.6 : 1, cursor: training ? 'not-allowed' : 'pointer' }}
                >
                  ➕ Add {blueprint.displayName}
                </button>
              ))}
            </div>
          </div>
        )}

        {/* PIPELINE WORKSPACE */}
        {activeWorkspaceBlocks.length > 0 ? (
          <div style={formGridStyle}>
            {activeWorkspaceBlocks.map((block, index) => {
              const masterBlueprint = featureBlueprints.find(b => b.featureCode === block.featureCode);
              if (!masterBlueprint) return null;

              return (
                <div key={block.id} style={featureCardStyle}>
                  <div style={cardHeaderStyle}>
                    <span style={instanceBadgeStyle}>#{index + 1}</span>
                    <h4 style={{ margin: 0, fontSize: '15px', fontWeight: 'bold' }}>{block.displayName}</h4>
                    <button 
                      onClick={() => removeFeatureInstance(block.id)}
                      disabled={training}
                      style={{ ...deleteBlockButtonStyle, opacity: training ? 0.5 : 1, cursor: training ? 'not-allowed' : 'pointer' }}
                    >
                      Delete
                    </button>
                  </div>

                  <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
                    {masterBlueprint.parameters.map((param) => {
                      if (!isParamVisible(block, param)) return null;

                      return (
                        <div key={param.name} style={fieldGroupStyle}>
                          <label style={fieldLabelStyle}>{param.label}</label>
                          {param.type === 'select' ? (
                            <select
                              value={block.configuredValues[param.name] || ''}
                              onChange={(e) => handleInstanceValueChange(block.id, param.name, e.target.value, 'select')}
                              disabled={training}
                              style={inputStyle}
                            >
                              <option value="">-- Choose option --</option>
                              {param.options.map(opt => (
                                <option key={opt} value={opt}>{opt}</option>
                              ))}
                            </select>
                          ) : (
                            <input
                              type="number"
                              value={block.configuredValues[param.name] ?? ''}
                              onChange={(e) => handleInstanceValueChange(block.id, param.name, e.target.value, 'number')}
                              disabled={training}
                              style={inputStyle}
                            />
                          )}
                        </div>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>
        ) : (
          selectedModel && !featuresLoading && (
            <div style={emptyWorkspaceStyle}>
              Your target pipeline workspace layout is completely empty. Click on one of the feature buttons above to mount an instance card module.
            </div>
          )
        )}

        {/* Run Button Panel */}
        {activeWorkspaceBlocks.length > 0 && (
          <div style={trainingActionContainerStyle}>
            <button
              onClick={handleTrainPipelineSubmit}
              disabled={training}
              style={{
                ...trainButtonStyle,
                backgroundColor: training ? '#bfbfbf' : '#52c41a',
                cursor: training ? 'not-allowed' : 'pointer'
              }}
            >
              {training ? '⚙️ Processing Rolling Setup...' : '🚀 Train ML Pipeline Model'}
            </button>
          </div>
        )}

        {/* Live Payload Preview Outpost */}
        {activeWorkspaceBlocks.length > 0 && (
          <div style={diagnosticContainerStyle}>
            <span style={labelStyle}>Live Compiled JSON Pipeline Export Structure</span>
            <pre style={jsonCodeStyle}>
              {JSON.stringify({
                uniqueCustomName: `${selectedModel}_roll_${trainingWindowSize || 'X'}M_by_${retrainFrequency}`,
                modelName: selectedModel,
                totalWindowStart: formatToDateTimeString(totalWindowStart, false),
                totalWindowEnd: formatToDateTimeString(totalWindowEnd, true),
                trainingWindowSizeMonths: Number(trainingWindowSize),
                retrainFrequency: retrainFrequency,
                featuresPipeline: activeWorkspaceBlocks.map(b => ({
                  featureCode: b.featureCode,
                  parameters: b.configuredValues
                }))
              }, null, 2)}
            </pre>
          </div>
        )}
      </div>
    </div>
  );
};

// --- STYLING SYSTEMS ---
const containerStyle = { display: 'flex', flexDirection: 'column', gap: '4px', maxWidth: '320px', margin: '0 auto', textAlign: 'left' };
const labelStyle = { fontSize: '11px', color: '#666', fontWeight: 'bold', textTransform: 'uppercase', letterSpacing: '0.5px' };
const inputGroupStyle = { display: 'flex', alignItems: 'center', gap: '6px' };
const dropdownStyle = { padding: '10px', borderRadius: '4px', outline: 'none', fontSize: '14px', width: '100%' };

const settingsPanelStyle = { marginTop: '28px', padding: '20px', border: '1px solid #e2e8f0', borderRadius: '8px', backgroundColor: '#f8fafc', textAlign: 'left' };
const settingsGridStyle = { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '20px' };
const settingFieldGroupStyle = { display: 'flex', flexDirection: 'column', gap: '6px' };
const settingFieldLabelStyle = { fontSize: '12px', fontWeight: '600', color: '#475569' };
const settingInputStyle = { padding: '9px 12px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '13px', outline: 'none', backgroundColor: '#fff', color: '#1e293b', fontFamily: 'inherit' };

const toolbarStyle = { marginTop: '32px', padding: '16px', border: '1px dashed #bbb', borderRadius: '6px', backgroundColor: '#fdfdfd', textAlign: 'left' };
const addBlockButtonStyle = { padding: '8px 14px', fontSize: '13px', border: '1px solid #1890ff', borderRadius: '4px', backgroundColor: '#e6f7ff', color: '#1890ff', fontWeight: '500' };

const formGridStyle = { marginTop: '24px', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '20px', textAlign: 'left' };
const featureCardStyle = { border: '1px solid #d9d9d9', borderRadius: '6px', padding: '16px', backgroundColor: '#fff', boxShadow: '0 2px 4px rgba(0,0,0,0.02)' };

const cardHeaderStyle = { display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: '1px solid #f0f0f0', paddingBottom: '10px', marginBottom: '14px' };
const instanceBadgeStyle = { backgroundColor: '#f5f5f5', color: '#666', padding: '2px 6px', borderRadius: '4px', fontSize: '11px', fontWeight: 'bold' };
const deleteBlockButtonStyle = { border: 'none', background: 'none', color: '#ff4d4f', fontSize: '12px', fontWeight: '500' };

const fieldGroupStyle = { display: 'flex', flexDirection: 'column', gap: '4px' };
const fieldLabelStyle = { fontSize: '12px', fontWeight: '500', color: '#444' };
const inputStyle = { padding: '8px 10px', borderRadius: '4px', border: '1px solid #ccc', fontSize: '13px', outline: 'none', width: '100%', boxSizing: 'border-box' };

const emptyWorkspaceStyle = { marginTop: '40px', padding: '32px', color: '#999', border: '1px dashed #e8e8e8', backgroundColor: '#fafafa', borderRadius: '4px', fontSize: '13px' };

const trainingActionContainerStyle = { marginTop: '32px', padding: '16px 0', borderTop: '1px solid #f0f0f0', display: 'flex', justifyContent: 'flex-end' };
const trainButtonStyle = { padding: '12px 28px', fontSize: '15px', fontWeight: 'bold', color: '#fff', border: 'none', borderRadius: '6px', transition: 'all 0.2s ease', boxShadow: '0 2px 6px rgba(0,0,0,0.1)' };

const diagnosticContainerStyle = { marginTop: '32px', textAlign: 'left', borderTop: '1px dashed #ccc', paddingTop: '24px' };
const jsonCodeStyle = { backgroundColor: '#2d3748', color: '#f7fafc', padding: '16px', borderRadius: '6px', fontSize: '12px', overflowX: 'auto', marginTop: '8px', fontFamily: 'monospace' };

export default MlModelManager;