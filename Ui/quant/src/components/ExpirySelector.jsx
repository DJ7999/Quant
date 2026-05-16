import React, { useState, useEffect } from 'react';
import { api } from '../core/api';

const ExpirySelector = ({ underlying, optionType, value, onSelect, onClear, disabled }) => {
  const [expiries, setExpiries] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (underlying && optionType) {
      const fetchExpiries = async () => {
        setLoading(true);
        try {
          const response = await api.post('/api/option-history/get-contract-expiries', {
            underlying,
            optionType
          });
          // Sort Descending
          setExpiries(response.sort((a, b) => new Date(b) - new Date(a)));
        } catch (err) { console.error(err); } 
        finally { setLoading(false); }
      };
      fetchExpiries();
    }
  }, [underlying, optionType]);

  return (
    <div style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: '4px', minWidth: '120px' }}>
      <label style={{ fontSize: '11px', fontWeight: 'bold', color: '#666' }}>EXPIRY</label>
      <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
        <select 
          value={value} 
          onChange={(e) => onSelect(e.target.value)}
          disabled={disabled || loading}
          style={{ padding: '8px', borderRadius: '4px', border: '1px solid #ddd', flex: 1, backgroundColor: disabled ? '#f5f5f5' : 'white' }}
        >
          <option value="">{loading ? '...' : '-- Expiry --'}</option>
          {expiries.map(d => (
            <option key={d} value={d}>{new Date(d).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}</option>
          ))}
        </select>
        {value && !disabled && <button onClick={onClear} style={{ border: 'none', background: 'none', color: '#ff4d4f', cursor: 'pointer', fontSize: '20px' }}>×</button>}
      </div>
    </div>
  );
};
export default ExpirySelector;