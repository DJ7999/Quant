import React, { useState, useEffect } from 'react';
import { api } from '../core/api';

const StrikeSelector = ({ underlying, optionType, expiry, value, onSelect }) => {
  const [strikes, setStrikes] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (underlying && optionType && expiry) {
      const fetchStrikes = async () => {
        setLoading(true);
        try {
          // Matches: [HttpPost("get-contract-strikes")]
          const response = await api.post('/api/option-history/get-contract-strikes', {
            underlying: underlying,
            optionType: optionType,
            expirationDate: expiry // Map frontend 'expiry' to DTO 'ExpirationDate'
          });
          setStrikes(response);
        } catch (err) {
          console.error("Error fetching strikes:", err);
        } finally {
          setLoading(false);
        }
      };
      fetchStrikes();
    }
  }, [underlying, optionType, expiry]);

  return (
    <div style={containerStyle}>
      <label style={labelStyle}>STRIKE</label>
      <select 
        value={value} 
        onChange={(e) => onSelect(e.target.value)}
        disabled={loading || !expiry}
        style={dropdownStyle}
      >
        <option value="">{loading ? 'Loading...' : '-- Strike --'}</option>
        {strikes.sort((a, b) => a - b).map(strike => (
          <option key={strike} value={strike}>
            {strike}
          </option>
        ))}
      </select>
    </div>
  );
};

const containerStyle = { flex: 1, display: 'flex', flexDirection: 'column', gap: '4px' };
const labelStyle = { fontSize: '11px', fontWeight: 'bold', color: '#666' };
const dropdownStyle = { padding: '8px', borderRadius: '4px', border: '1px solid #ddd', fontSize: '13px' };

export default StrikeSelector;