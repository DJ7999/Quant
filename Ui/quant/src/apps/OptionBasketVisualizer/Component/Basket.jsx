import React, { useState } from 'react';
import { theme } from '../../../core/theme';
import LegRow from './LegRow';
import Visualize from './Visualize';

const Basket = ({ basket, onDelete, onToggle }) => {
  const [committedLegs, setCommittedLegs] = useState([]);
  const [isAdding, setIsAdding] = useState(false);
  const [showVisualization, setShowVisualization] = useState(false);

  const handleCommitLeg = (legData) => {
    const newLeg = { ...legData, id: Date.now() };
    setCommittedLegs([...committedLegs, newLeg]);
    setIsAdding(false); 
  };

  const removeCommittedLeg = (id) => {
    setCommittedLegs(prev => prev.filter(leg => leg.id !== id));
    // Optionally hide chart if no legs are left
    if (committedLegs.length <= 1) setShowVisualization(false);
  };

  return (
    <div style={basketWrapperStyle}>
      {/* Header Section */}
      <div style={headerStyle} onClick={onToggle}>
        <div style={headerTitleGroup}>
          <button onClick={(e) => { e.stopPropagation(); onDelete(); }} style={deleteBasketBtn}>✕</button>
          <span style={{ fontWeight: '700' }}>{basket.title}</span>
        </div>
        
        <div style={{ display: 'flex', gap: '8px' }} onClick={(e) => e.stopPropagation()}>
          {committedLegs.length > 0 && (
            <button 
              onClick={() => setShowVisualization(!showVisualization)}
              style={{
                ...visualizeBtnStyle,
                backgroundColor: showVisualization ? '#dc3545' : '#6f42c1'
              }}
            >
              {showVisualization ? 'Close Chart' : '📊 Visualize'}
            </button>
          )}

          <button 
            onClick={() => setIsAdding(true)} 
            style={addBtnStyle}
            disabled={isAdding}
          >
            + Add Leg
          </button>
        </div>
      </div>

      {/* Body Section */}
      {basket.isOpen && (
        <div style={{ padding: '16px' }}>
          
          {/* 1. Strategy Builder (Always visible if open) */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '8px', marginBottom: '16px' }}>
            {committedLegs.map((leg) => (
              <div key={leg.id} style={summaryRowStyle}>
                <span style={{ 
                  ...qtyBadgeStyle, 
                  backgroundColor: leg.quantity >= 0 ? '#e6f4ea' : '#fce8e6',
                  color: leg.quantity >= 0 ? '#1e7e34' : '#d93025'
                }}>
                  {leg.quantity >= 0 ? `+${leg.quantity}` : leg.quantity}
                </span>
                <span style={tickerBadgeStyle}>{leg.underlying}</span>
                <span style={expiryTextStyle}>
                  {new Date(leg.expiry).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })}
                </span>
                <span style={{ color: leg.type === 'CALL' ? '#2ecc71' : '#e74c3c', fontWeight: 'bold', width: '40px' }}>
                  {leg.type}
                </span>
                <span style={{ fontWeight: 'bold', flex: 1 }}>@{leg.strike}</span>
                <button onClick={() => removeCommittedLeg(leg.id)} style={removeBtnStyle}>Remove</button>
              </div>
            ))}
          </div>

          {/* 2. Form for adding new legs */}
          {isAdding && (
            <LegRow onAddLeg={handleCommitLeg} onCancel={() => setIsAdding(false)} />
          )}

          {committedLegs.length === 0 && !isAdding && (
            <div style={emptyStateStyle}>No legs added. Click "+ Add Leg" to begin.</div>
          )}

          {/* 3. Visualization Section (Appears below legs) */}
          {showVisualization && committedLegs.length > 0 && (
            <div style={chartSectionStyle}>
              <div style={dividerStyle} />
              <Visualize 
                legs={committedLegs} 
                onBack={() => setShowVisualization(false)} 
              />
            </div>
          )}
        </div>
      )}
    </div>
  );
};

// --- Styles ---
const basketWrapperStyle = { marginBottom: '16px', border: `1px solid ${theme.colors.border}`, borderRadius: '8px', overflow: 'hidden', backgroundColor: 'white' };
const headerStyle = { padding: '12px 16px', backgroundColor: '#f8f9fa', display: 'flex', alignItems: 'center', justifyContent: 'space-between', cursor: 'pointer', borderBottom: '1px solid #eee' };
const headerTitleGroup = { display: 'flex', alignItems: 'center', gap: '10px' };
const deleteBasketBtn = { background: 'none', border: 'none', color: '#ff4d4f', cursor: 'pointer' };
const addBtnStyle = { padding: '6px 12px', backgroundColor: '#007bff', color: 'white', border: 'none', borderRadius: '4px', fontWeight: '600', fontSize: '12px' };
const visualizeBtnStyle = { padding: '6px 12px', color: 'white', border: 'none', borderRadius: '4px', fontWeight: '600', fontSize: '12px', cursor: 'pointer', transition: '0.2s' };
const summaryRowStyle = { display: 'flex', gap: '16px', alignItems: 'center', padding: '10px 12px', border: '1px solid #f0f0f0', borderRadius: '6px', fontSize: '14px' };
const qtyBadgeStyle = { padding: '2px 6px', borderRadius: '4px', fontWeight: 'bold', minWidth: '35px', textAlign: 'center' };
const tickerBadgeStyle = { backgroundColor: '#f0f2f5', padding: '2px 6px', borderRadius: '4px', fontWeight: 'bold' };
const expiryTextStyle = { fontSize: '13px', color: '#555', minWidth: '60px' };
const removeBtnStyle = { background: 'none', border: 'none', color: '#dc3545', fontSize: '12px', cursor: 'pointer' };
const emptyStateStyle = { textAlign: 'center', color: '#999', padding: '20px', fontStyle: 'italic' };

// New Styles for the Chart Section
const chartSectionStyle = { marginTop: '20px' };
const dividerStyle = { height: '1px', backgroundColor: '#eee', margin: '20px 0' };

export default Basket;