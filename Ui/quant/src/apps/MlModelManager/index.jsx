import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { theme } from '../../core/theme';
import { api } from '../../core/api'; 

// Fallback placeholder formatting structure to guarantee zero-break renders
const commonStyles = {
  appContainer: { padding: '24px', maxWidth: '1200px', margin: '0 auto', fontFamily: 'system-ui, sans-serif' },
  surface: { backgroundColor: '#ffffff', borderRadius: '12px', padding: '32px', boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }
};

const MlModelManager = () => {
  const [models, setModels] = useState([]);
  const [loading, setLoading] = useState(false);
  const [featuresLoading, setFeaturesLoading] = useState(false);
  const [error, setError] = useState(null);
  const [selectedModel, setSelectedModel] = useState('');
  
  // ROLLING ITERATION PARAMETERS (Aligned with C# DTO defaults)
  const [trainingWindowSize, setTrainingWindowSize] = useState('12'); // DTO default: 12
  const [retrainFrequency, setRetrainFrequency] = useState('1');    // DTO default: 1

  // Blueprint and Workspace layout pipelines
  const [featureBlueprints, setFeatureBlueprints] = useState([]);
  const [modelParameters, setModelParameters] = useState([]); // Dynamic storage for C# Strategy Parameters list
  const [configuredParams, setConfiguredParams] = useState({}); // Stores user changes to algorithm hyperparameters
  const [activeWorkspaceBlocks, setActiveWorkspaceBlocks] = useState([]);
  const [training, setTraining] = useState(false);

  // Prefetch model definitions cleanly on primary explicit activation click
  const handleDropdownClick = async () => {
    if (loading || models.length > 0) return;
    setLoading(true);
    setError(null);
    try {
      const data = await api.get('/api/ml-manager/get-model');
      setModels(data || []);
    } catch (err) {
      setError("Failed to load available ML architectures.");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // Synchronize architectural configurations and wipe dirty states
  useEffect(() => {
    if (!selectedModel) {
      setFeatureBlueprints([]);
      setModelParameters([]);
      setConfiguredParams({});
      setActiveWorkspaceBlocks([]);
      setTrainingWindowSize('12');
      setRetrainFrequency('1');
      return;
    }

    const fetchFeaturesAndParams = async () => {
      setFeaturesLoading(true);
      setError(null);
      try {
        const data = await api.get(`/api/ml-manager/get-features?modelName=${encodeURIComponent(selectedModel)}`);
        
        console.log("Raw API Object Response:", data);

        // SAFE PROPERTY EXTRACTION (Handles PascalCase vs camelCase fallbacks seamlessly)
        const featuresList = data?.features || data?.Features;
        const parametersWrapper = data?.parameters || data?.Parameters;
        const targetParametersList = parametersWrapper?.parameters || parametersWrapper?.Parameters || [];

        // Direct fallback array assignments if structure is completely flat
        const verifiedFeatures = Array.isArray(featuresList) 
          ? featuresList 
          : (Array.isArray(data) ? data : []);

        setFeatureBlueprints(verifiedFeatures);
        setModelParameters(targetParametersList);

        // Pre-populate structural strategy parameters fallback map dictionary safely converting nulls to empty string
        const initialParamValues = {};
        targetParametersList.forEach(p => {
          const pKey = p.key || p.Key;
          const pDefault = p.defaultValue !== null ? (p.defaultValue ?? p.DefaultValue) : '';
          if (pKey) {
            initialParamValues[pKey] = pDefault !== null ? pDefault : '';
          }
        });
        setConfiguredParams(initialParamValues);
        setActiveWorkspaceBlocks([]); 
      } catch (err) {
        setError("Failed to load feature templates or strategy parameters for this architecture.");
        console.error(err);
      } finally {
        setFeaturesLoading(false);
      }
    };

    fetchFeaturesAndParams();
  }, [selectedModel]);

  // Intercept and parse incoming target strategy parameters dynamically
  const handleModelParamChange = (key, value, inputType) => {
    const parsedValue = (inputType === 'Integer' || inputType === 'number') && value !== '' ? Number(value) : value;
    setConfiguredParams(prev => ({
      ...prev,
      [key]: parsedValue
    }));
  };

  // Append customized architectural modules to staging pipeline array
  const addFeatureInstance = useCallback((blueprint) => {
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
      id: `instance-${Date.now()}-${Math.random().toString(36).substring(2, 6)}`,
      featureCode: blueprint.featureCode || blueprint.FeatureCode,
      displayName: blueprint.displayName || blueprint.DisplayName,
      configuredValues: defaultValues
    };

    setActiveWorkspaceBlocks(prev => [...prev, newBlockInstance]);
  }, []);

  // Remove targeted instance cards from tracking states
  const removeFeatureInstance = useCallback((instanceId) => {
    setActiveWorkspaceBlocks(prev => prev.filter(b => b.id !== instanceId));
  }, []);

  // Intercept changes and parse values appropriately
  const handleInstanceValueChange = useCallback((instanceId, paramName, value, type) => {
    const parsedValue = type === 'number' && value !== '' ? Number(value) : value;

    setActiveWorkspaceBlocks(prev => prev.map(block => {
      if (block.id !== instanceId) return block;
      return {
        ...block,
        configuredValues: { 
          ...block.configuredValues, 
          [paramName]: parsedValue 
        }
      };
    }));
  }, []);

  // Handle nested conditional rendering dependencies across cards
  const isParamVisible = useCallback((block, param) => {
    const visibleIfProperty = param.visibleIfProperty || param.VisibleIfProperty;
    const visibleIfValues = param.visibleIfValues || param.VisibleIfValues;

    if (!visibleIfProperty) return true;
    
    const targetKey = visibleIfProperty.toLowerCase();
    const matchingKey = Object.keys(block.configuredValues).find(k => k.toLowerCase() === targetKey);
    const currentParentValue = matchingKey ? block.configuredValues[matchingKey] : undefined;
    
    if (currentParentValue === undefined || currentParentValue === null || currentParentValue === '') {
      return false;
    }

    if (visibleIfValues && visibleIfValues.length > 0) {
      return visibleIfValues
        .map(v => String(v).toLowerCase())
        .includes(String(currentParentValue).toLowerCase());
    }
    return true;
  }, []);

  // Compile full structural context state matching your backend payload schema definition
  const compiledPipelinePayload = useMemo(() => {
    return {
      ModelName: selectedModel,
      RetrainingFrequencyInMonths: Number(retrainFrequency) || 1,
      ModelTrainingWindowInMonths: Number(trainingWindowSize) || 12,
      Parameters: configuredParams, // Property explicitly aligned to your parameters block
      FeaturesPipeline: activeWorkspaceBlocks.map(block => ({
        featureCode: block.featureCode,
        parameters: block.configuredValues 
      }))
    };
  }, [selectedModel, trainingWindowSize, retrainFrequency, activeWorkspaceBlocks, configuredParams]);

  // Execute external target training post operations safely
  const handleTrainPipelineSubmit = async () => {
    if (activeWorkspaceBlocks.length === 0) return;
    
    // Core structural constraint checks
    if (!trainingWindowSize || Number(trainingWindowSize) <= 0) {
      setError("Please input a valid positive integer value for the Model Training Window.");
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }
    if (!retrainFrequency || Number(retrainFrequency) <= 0) {
      setError("Please input a valid positive integer value for the Retraining Frequency.");
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    setTraining(true);
    setError(null);

    try {
      const response = await api.post('/api/ml-manager/train-model', compiledPipelinePayload);
      alert(response?.message || "ML pipeline backend model execution processing initialized!");
    } catch (err) {
      setError("An error occurred executing remote pipeline calculations.");
      console.error(err);
    } finally {
      setTraining(false);
    }
  };

  return (
    <div style={commonStyles.appContainer}>
      <div style={commonStyles.surface}>
        <div style={{ fontSize: '48px', marginBottom: '16px' }}>🤖</div>
        <h2 style={{ marginBottom: '8px', color: '#1e293b' }}>ML Model Pipeline Builder</h2>
        <p style={{ color: '#64748b', marginBottom: '32px', fontSize: '15px', lineHeight: '1.5' }}>
          Select an architecture, construct rolling window simulation constraints, and stack data generation features.
        </p>

        {error && (
          <div style={{ backgroundColor: '#fff2f0', border: '1px solid #ffccc7', color: '#ff4d4f', padding: '14px', borderRadius: '6px', marginBottom: '24px', textAlign: 'left', fontSize: '14px', fontWeight: '500' }}>
            ⚠️ {error}
          </div>
        )}

        {/* Target Architecture Dropdown Option Selection Menu */}
        <div style={containerStyle}>
          <label style={labelStyle}>Active Model Architecture</label>
          <div style={inputGroupStyle}>
            <select
              value={selectedModel}
              onFocus={handleDropdownClick}
              onChange={(e) => setSelectedModel(e.target.value)}
              disabled={loading || training}
              style={{ ...dropdownStyle, backgroundColor: '#fff', color: '#1e293b', border: '1px solid #cbd5e1' }}
            >
              <option value="">{loading ? 'Loading remote options...' : '-- Select Architecture --'}</option>
              {models.map(m => <option key={m} value={m}>{m}</option>)}
            </select>
          </div>
        </div>

        {/* Parameter Rolling Bounds UI Panel */}
        {selectedModel && (
          <div style={settingsPanelStyle}>
            <span style={{ ...labelStyle, display: 'block', marginBottom: '16px', color: '#0f172a', fontSize: '12px' }}>
              Rolling Optimization Parameters
            </span>
            <div style={settingsGridStyle}>
              
              <div style={settingFieldGroupStyle}>
                <label style={settingFieldLabelStyle}>Model Training Window (Months)</label>
                <input 
                  type="number"
                  min="1"
                  placeholder="e.g. 12"
                  value={trainingWindowSize || ''}
                  onChange={(e) => setTrainingWindowSize(e.target.value)}
                  disabled={training}
                  style={settingInputStyle}
                />
              </div>

              <div style={settingFieldGroupStyle}>
                <label style={settingFieldLabelStyle}>Retraining Frequency (Months)</label>
                <input 
                  type="number"
                  min="1"
                  placeholder="e.g. 1"
                  value={retrainFrequency || ''}
                  onChange={(e) => setRetrainFrequency(e.target.value)}
                  disabled={training}
                  style={settingInputStyle}
                />
              </div>
            </div>

            {/* DYNAMIC HYPERPARAMETER STRATEGY INJECTION PANEL */}
            {modelParameters.length > 0 && (
              <div style={{ marginTop: '24px', paddingTop: '20px', borderTop: '1px solid #e2e8f0' }}>
                <span style={{ ...labelStyle, display: 'block', marginBottom: '16px', color: '#0f172a', fontSize: '12px' }}>
                  {selectedModel} Strategy Parameters
                </span>
                <div style={settingsGridStyle}>
                  {modelParameters.map(param => {
                    const pKey = param.key || param.Key;
                    const pLabel = param.label || param.Label;
                    const pInputType = param.inputType || param.InputType;
                    const pMin = param.minvalue || param.Minvalue;
                    const pRequired = param.isRequired || param.IsRequired;
                    const pDefault = param.defaultValue || param.DefaultValue;

                    return (
                      <div key={pKey} style={settingFieldGroupStyle}>
                        <label style={settingFieldLabelStyle}>
                          {pLabel} {pRequired && <span style={{ color: '#ef4444' }}>*</span>}
                        </label>
                        <input 
                          type={pInputType === 'Integer' ? 'number' : 'text'}
                          min={pMin ?? undefined}
                          placeholder={pDefault ? `Default: ${pDefault}` : ''}
                          value={configuredParams[pKey] ?? ''}
                          onChange={(e) => handleModelParamChange(pKey, e.target.value, pInputType)}
                          disabled={training}
                          required={pRequired}
                          style={settingInputStyle}
                        />
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
          </div>
        )}

        {/* Feature Blueprint Append Block Ribbon Toolbar */}
        {selectedModel && featureBlueprints.length > 0 && (
          <div style={toolbarStyle}>
            <span style={{ ...labelStyle, display: 'block', marginBottom: '12px' }}>Available Feature Building Blocks</span>
            <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap' }}>
              {featureBlueprints.map(blueprint => {
                const fCode = blueprint.featureCode || blueprint.FeatureCode;
                const fDisplay = blueprint.displayName || blueprint.DisplayName;
                return (
                  <button
                    key={fCode}
                    onClick={() => addFeatureInstance(blueprint)}
                    disabled={training}
                    style={{ ...addBlockButtonStyle, opacity: training ? 0.6 : 1, cursor: training ? 'not-allowed' : 'pointer' }}
                  >
                    ➕ Add {fDisplay}
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* Pipeline Workspace Grid Blocks Layout Frame Area */}
        {activeWorkspaceBlocks.length > 0 ? (
          <div style={formGridStyle}>
            {activeWorkspaceBlocks.map((block, index) => {
              const masterBlueprint = featureBlueprints.find(b => {
                const fCode = b.featureCode || b.FeatureCode;
                return fCode === block.featureCode;
              });
              if (!masterBlueprint) return null;

              const masterBlueprintParams = masterBlueprint.parameters || masterBlueprint.Parameters;

              return (
                <div key={block.id} style={featureCardStyle}>
                  <div style={cardHeaderStyle}>
                    <span style={instanceBadgeStyle}>#{index + 1}</span>
                    <h4 style={{ margin: 0, fontSize: '14px', fontWeight: '700', color: '#1e293b' }}>{block.displayName}</h4>
                    <button 
                      onClick={() => removeFeatureInstance(block.id)}
                      disabled={training}
                      style={{ ...deleteBlockButtonStyle, opacity: training ? 0.5 : 1, cursor: training ? 'not-allowed' : 'pointer' }}
                    >
                      Delete
                    </button>
                  </div>

                  <div style={{ display: 'flex', flexDirection: 'column', gap: '14px' }}>
                    {masterBlueprintParams?.map((param) => {
                      const pName = param.name || param.Name;
                      const pLabel = param.label || param.Label;
                      const pType = param.type || param.Type;
                      const pOptions = param.options || param.Options;

                      if (!isParamVisible(block, param)) return null;

                      return (
                        <div key={pName} style={fieldGroupStyle}>
                          <label style={fieldLabelStyle}>{pLabel}</label>
                          {pType === 'select' ? (
                            <select
                              value={block.configuredValues[pName] || ''}
                              onChange={(e) => handleInstanceValueChange(block.id, pName, e.target.value, 'select')}
                              disabled={training}
                              style={inputStyle}
                            >
                              <option value="">-- Choose option --</option>
                              {pOptions?.map(opt => (
                                <option key={opt} value={opt}>{opt}</option>
                              ))}
                            </select>
                          ) : (
                            <input
                              type="number"
                              value={block.configuredValues[pName] ?? ''}
                              onChange={(e) => handleInstanceValueChange(block.id, pName, e.target.value, 'number')}
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

        {/* Pipeline Execution Post Confirmation Triggers */}
        {activeWorkspaceBlocks.length > 0 && (
          <div style={trainingActionContainerStyle}>
            <button
              onClick={handleTrainPipelineSubmit}
              disabled={training}
              style={{
                ...trainButtonStyle,
                backgroundColor: training ? '#94a3b8' : '#22c55e',
                cursor: training ? 'not-allowed' : 'pointer'
              }}
            >
              {training ? '⚙️ Processing Setup...' : '🚀 Train ML Pipeline Model'}
            </button>
          </div>
        )}

        {/* Structural Interactive Dynamic JSON Pipeline Output Live Logs Panel */}
        {activeWorkspaceBlocks.length > 0 && (
          <div style={diagnosticContainerStyle}>
            <span style={labelStyle}>Live Compiled Payload Structure</span>
            <pre style={jsonCodeStyle}>
              {JSON.stringify(compiledPipelinePayload, null, 2)}
            </pre>
          </div>
        )}
      </div>
    </div>
  );
};

// --- STYLING SYSTEMS ---
const containerStyle = { display: 'flex', flexDirection: 'column', gap: '6px', maxWidth: '340px', margin: '0 auto', textAlign: 'left' };
const labelStyle = { fontSize: '11px', color: '#64748b', fontWeight: 'bold', textTransform: 'uppercase', letterSpacing: '0.5px' };
const inputGroupStyle = { display: 'flex', alignItems: 'center', gap: '6px' };
const dropdownStyle = { padding: '10px 12px', borderRadius: '6px', outline: 'none', fontSize: '14px', width: '100%', transition: 'border-color 0.15s ease' };

const settingsPanelStyle = { marginTop: '28px', padding: '20px', border: '1px solid #e2e8f0', borderRadius: '8px', backgroundColor: '#f8fafc', textAlign: 'left' };
const settingsGridStyle = { display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '20px' };
const settingFieldGroupStyle = { display: 'flex', flexDirection: 'column', gap: '6px' };
const settingFieldLabelStyle = { fontSize: '12px', fontWeight: '600', color: '#475569' };
const settingInputStyle = { padding: '9px 12px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '13px', outline: 'none', backgroundColor: '#fff', color: '#1e293b', fontFamily: 'inherit' };

const toolbarStyle = { marginTop: '32px', padding: '18px', border: '1px dashed #cbd5e1', borderRadius: '8px', backgroundColor: '#f8fafc', textAlign: 'left' };
const addBlockButtonStyle = { padding: '8px 14px', fontSize: '13px', border: '1px solid #3b82f6', borderRadius: '6px', backgroundColor: '#eff6ff', color: '#1d4ed8', fontWeight: '500', transition: 'all 0.15s ease' };

const formGridStyle = { marginTop: '24px', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: '24px', textAlign: 'left' };
const featureCardStyle = { border: '1px solid #e2e8f0', borderRadius: '8px', padding: '18px', backgroundColor: '#fff', boxShadow: '0 2px 5px rgba(0,0,0,0.02)' };

const cardHeaderStyle = { display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: '1px solid #f1f5f9', paddingBottom: '12px', marginBottom: '16px' };
const instanceBadgeStyle = { backgroundColor: '#f1f5f9', color: '#475569', padding: '2px 8px', borderRadius: '4px', fontSize: '11px', fontWeight: 'bold' };
const deleteBlockButtonStyle = { border: 'none', background: 'none', color: '#ef4444', fontSize: '12px', fontWeight: '600', padding: 0 };

const fieldGroupStyle = { display: 'flex', flexDirection: 'column', gap: '6px' };
const fieldLabelStyle = { fontSize: '12px', fontWeight: '500', color: '#334155' };
const inputStyle = { padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '13px', outline: 'none', width: '100%', boxSizing: 'border-box', color: '#0f172a' };

const emptyWorkspaceStyle = { marginTop: '40px', padding: '40px 24px', color: '#94a3b8', border: '1px dashed #e2e8f0', backgroundColor: '#f8fafc', borderRadius: '8px', fontSize: '13px', textAlign: 'center', lineHeight: '1.6' };

const trainingActionContainerStyle = { marginTop: '32px', padding: '20px 0 0 0', borderTop: '1px solid #e2e8f0', display: 'flex', justifyContent: 'flex-end' };
const trainButtonStyle = { padding: '12px 32px', fontSize: '14px', fontWeight: 'bold', color: '#fff', border: 'none', borderRadius: '6px', transition: 'all 0.2s ease', boxShadow: '0 2px 4px rgba(0,0,0,0.05)' };

const diagnosticContainerStyle = { marginTop: '32px', textAlign: 'left', borderTop: '1px dashed #cbd5e1', paddingTop: '24px' };
const jsonCodeStyle = { backgroundColor: '#0f172a', color: '#f8fafc', padding: '18px', borderRadius: '8px', fontSize: '12px', overflowX: 'auto', marginTop: '12px', fontFamily: 'monospace', lineHeight: '1.5' };

export default MlModelManager;