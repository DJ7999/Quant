import React from 'react';

const OptionTypeSelector = ({ value, onSelect, onClear, disabled }) => (
  <div style={containerStyle}>
    <label style={labelStyle}>TYPE</label>
    <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
      <select 
        value={value} 
        onChange={(e) => onSelect(e.target.value)}
        disabled={disabled}
        style={{ ...dropdownStyle, backgroundColor: disabled ? '#f5f5f5' : 'white' }}
      >
        <option value="">-- Type --</option>
        <option value="CALL">Call</option>
        <option value="PUT">Put</option>
      </select>
      {value && !disabled && (
        <button onClick={onClear} style={clearButtonStyle}>×</button>
      )}
    </div>
  </div>
);

const containerStyle = { flex: 1, display: 'flex', flexDirection: 'column', gap: '4px', minWidth: '100px' };
const labelStyle = { fontSize: '11px', fontWeight: 'bold', color: '#666' };
const dropdownStyle = { padding: '8px', borderRadius: '4px', border: '1px solid #ddd', width: '100%' };
const clearButtonStyle = { border: 'none', background: 'none', color: '#ff4d4f', cursor: 'pointer', fontSize: '20px', lineHeight: '1' };

export default OptionTypeSelector;