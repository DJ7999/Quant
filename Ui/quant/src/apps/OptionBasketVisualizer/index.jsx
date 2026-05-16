import React, { useState } from 'react';
import { theme } from '../../core/theme';
import { commonStyles } from '../../components/common';
import Basket from './Component/Basket';

/**
 * OptionBasketVisualizer: Acts as a high-level manager for strategy baskets.
 * Its only job is to create, destroy, and arrange the layout.
 */
const OptionBasketVisualizer = () => {
  const [baskets, setBaskets] = useState([]);

  // Create a new basket instance (limit of 3)
  const addBasket = () => {
    if (baskets.length >= 3) return;

    const newBasket = {
      id: Date.now(),
      title: `Basket ${baskets.length + 1}`,
      isOpen: true
    };
    setBaskets([...baskets, newBasket]);
  };

  // Remove a basket from the visualizer
  const deleteBasket = (id) => {
    setBaskets(baskets.filter(b => b.id !== id));
  };

  // Handle the collapsible state for layout purposes
  const toggleBasket = (id) => {
    setBaskets(baskets.map(b => 
      b.id === id ? { ...b, isOpen: !b.isOpen } : b
    ));
  };

  return (
    <div style={commonStyles.appContainer}>
      <header style={{ 
        marginBottom: theme.spacing.lg, 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center' 
      }}>
        <div>
          <h2 style={{ color: theme.colors.textMain, margin: 0 }}>Option Basket Visualizer</h2>
          <p style={{ color: theme.colors.textSecondary, fontSize: '14px', marginTop: '4px' }}>
            Manage up to 3 strategy baskets to compare risk profiles.
          </p>
        </div>
        
        <button 
          onClick={addBasket}
          disabled={baskets.length >= 3}
          style={commonStyles.button(baskets.length >= 3)}
        >
          {baskets.length >= 3 ? 'Max Baskets Reached' : '+ Add Basket'}
        </button>
      </header>

      <div className="visualizer-grid">
        {/* Left Column: List of Baskets */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          {baskets.map((basket) => (
            <Basket 
              key={basket.id}
              basket={basket}
              onDelete={() => deleteBasket(basket.id)}
              onToggle={() => toggleBasket(basket.id)}
            />
          ))}
          
          {baskets.length === 0 && (
            <div style={{ 
              padding: '48px 20px', 
              textAlign: 'center', 
              color: theme.colors.textSecondary, 
              border: `2px dashed ${theme.colors.border}`, 
              borderRadius: theme.radius.md 
            }}>
              <div style={{ fontSize: '32px', marginBottom: '12px' }}>📊</div>
              <p style={{ fontSize: '14px' }}>Click "+ Add Basket" to start building a strategy.</p>
            </div>
          )}
        </div>

        {/* Right Column: Visualization/Chart Area (Placeholder) */}
        <div style={{ 
          ...commonStyles.surface, 
          minHeight: '500px', 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center',
          backgroundColor: theme.colors.bgLight 
        }}>
          <div style={{ textAlign: 'center', color: theme.colors.textSecondary }}>
            <p>Strategy Payoff Visualization Surface</p>
            <span style={{ fontSize: '12px' }}>Driven by active baskets in the left column.</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OptionBasketVisualizer;