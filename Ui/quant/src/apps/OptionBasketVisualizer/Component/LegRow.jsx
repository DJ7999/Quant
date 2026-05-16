import React, { useState } from 'react';
import { theme } from '../../../core/theme';
import UnderlyingSelector from '../../../components/UnderlyingSelector';
import OptionTypeSelector from '../../../components/OptionTypeSelector';
import ExpirySelector from '../../../components/ExpirySelector';
import StrikeSelector from '../../../components/StrikeSelector';

const LegRow = ({ onAddLeg, onCancel }) => {
  const [leg, setLeg] = useState({
    underlying: '',
    type: '',
    expiry: '',
    strike: '',
    quantity: 1 // Default quantity
  });

  const updateField = (field, value) => {
    setLeg(prev => ({ ...prev, [field]: value }));
  };

  const handleAdd = () => {
    // Validate that all fields and quantity are present
    if (leg.underlying && leg.type && leg.expiry && leg.strike && leg.quantity !== 0) {
      onAddLeg(leg);
      // Reset local state for next entry
      setLeg({ underlying: '', type: '', expiry: '', strike: '', quantity: 1 });
    }
  };

  return (
    <div style={rowContainerStyle}>
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '12px', alignItems: 'flex-end', flex: 1 }}>
        
        <UnderlyingSelector 
          selectedValue={leg.underlying} 
          onSelect={(val) => updateField('underlying', val)} 
          onClear={() => updateField('underlying', '')}
          disabled={!!leg.type}
        />

        {leg.underlying && (
          <OptionTypeSelector 
            value={leg.type} 
            onSelect={(val) => updateField('type', val)} 
            onClear={() => updateField('type', '')}
            disabled={!!leg.expiry}
          />
        )}

        {leg.type && (
          <ExpirySelector 
            underlying={leg.underlying} 
            optionType={leg.type} 
            value={leg.expiry} 
            onSelect={(val) => updateField('expiry', val)} 
            onClear={() => updateField('expiry', '')}
            disabled={!!leg.strike}
          />
        )}

        {leg.expiry && (
          <StrikeSelector 
            underlying={leg.underlying} 
            optionType={leg.type} 
            expiry={leg.expiry} 
            value={leg.strike} 
            onSelect={(val) => updateField('strike', val)} 
            onClear={() => updateField('strike', '')}
          />
        )}

        {/* Step 5: Quantity - Only shows once contract is defined */}
        {leg.strike && (
          <div style={qtyContainerStyle}>
            <label style={labelStyle}>QUANTITY</label>
            <input 
              type="number"
              value={leg.quantity}
              onChange={(e) => updateField('quantity', parseInt(e.target.value) || 0)}
              style={qtyInputStyle}
              min="-1000"
              max="1000"
            />
          </div>
        )}
      </div>

      <div style={{ display: 'flex', gap: '8px', paddingBottom: '4px' }}>
        {leg.strike && (
          <button onClick={handleAdd} style={saveBtnStyle}>Add to Basket</button>
        )}
        <button onClick={onCancel} style={cancelBtnStyle}>Cancel</button>
      </div>
    </div>
  );
};

// Styles
const rowContainerStyle = { display: 'flex', gap: '16px', alignItems: 'flex-end', padding: '16px', backgroundColor: '#f9f9f9', borderRadius: '8px', border: '1px dashed #ccc', marginBottom: '16px' };
const labelStyle = { fontSize: '11px', fontWeight: 'bold', color: '#666', marginBottom: '4px' };
const qtyContainerStyle = { display: 'flex', flexDirection: 'column' };
const qtyInputStyle = { padding: '8px', borderRadius: '4px', border: '1px solid #ddd', width: '80px', fontSize: '14px', outline: 'none' };
const saveBtnStyle = { padding: '8px 16px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' };
const cancelBtnStyle = { padding: '8px 16px', background: 'none', border: '1px solid #ccc', borderRadius: '4px', cursor: 'pointer' };

export default LegRow;