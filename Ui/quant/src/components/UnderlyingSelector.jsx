import React, { useState, useEffect } from 'react';
import { theme } from '../core/theme';
import { api } from '../core/api';

/**
 * UnderlyingSelector: Fetches and displays available assets.
 * Supports locking when subsequent fields are selected.
 */
const UnderlyingSelector = ({ onSelect, selectedValue, onClear, disabled }) => {
  const [underlyings, setUnderlyings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    const loadUnderlyings = async () => {
      setLoading(true);
      try {
        const data = await api.get('/api/option-history/get-underlyings');
        setUnderlyings(data);
      } catch (err) {
        setError("Failed to load");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    loadUnderlyings();
  }, []);

  return (
    <div style={containerStyle}>
      <label style={labelStyle}>
        UNDERLYING ASSET {loading && "..."}
      </label>
      
      <div style={inputGroupStyle}>
        <select 
          value={selectedValue || ""}
          onChange={(e) => onSelect(e.target.value)}
          disabled={disabled || loading}
          style={{
            ...dropdownStyle,
            backgroundColor: disabled ? '#f5f5f5' : theme.colors.white,
            cursor: disabled ? 'not-allowed' : 'pointer',
            color: disabled ? '#999' : '#000',
            border: error ? `1px solid ${theme.colors.error}` : `1px solid ${theme.colors.border}`,
          }}
        >
          <option value="" disabled>-- Select Ticker --</option>
          {underlyings.map((symbol) => (
            <option key={symbol} value={symbol}>{symbol}</option>
          ))}
        </select>

        {/* Clear button: Only visible if a value is selected and NOT locked by the next field */}
        {selectedValue && !disabled && (
          <button 
            onClick={onClear}
            title="Clear Selection"
            style={clearButtonStyle}
          >
            ×
          </button>
        )}
      </div>
      {error && <span style={{ fontSize: '10px', color: theme.colors.error }}>{error}</span>}
    </div>
  );
};

// Internal Styles
const containerStyle = {
  display: 'flex',
  flexDirection: 'column',
  gap: '4px',
  flex: 1,
  minWidth: '150px'
};

const labelStyle = {
  fontSize: '11px',
  color: theme.colors.textSecondary,
  fontWeight: 'bold',
  textTransform: 'uppercase'
};

const inputGroupStyle = {
  display: 'flex',
  alignItems: 'center',
  gap: '6px'
};

const dropdownStyle = {
  padding: '8px',
  borderRadius: '4px',
  outline: 'none',
  fontSize: '14px',
  width: '100%',
  transition: 'all 0.2s ease'
};

const clearButtonStyle = {
  border: 'none',
  background: 'none',
  color: '#ff4d4f',
  cursor: 'pointer',
  fontSize: '22px',
  lineHeight: '1',
  padding: '0 4px',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center'
};

export default UnderlyingSelector;