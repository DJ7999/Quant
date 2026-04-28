import React from 'react';
import { theme } from '../core/theme';

const PortalShell = ({ children, title, onBack }) => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
      <header style={{ 
        height: '60px', padding: '0 24px', display: 'flex', alignItems: 'center', 
        justifyContent: 'space-between', borderBottom: `1px solid ${theme.colors.border}`,
        backgroundColor: theme.colors.white 
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
          <span style={{ fontWeight: '800', fontSize: '20px', letterSpacing: '-0.5px' }}>QUANT</span>
          {title && <span style={{ color: theme.colors.textSecondary }}>/ {title}</span>}
        </div>
        {onBack && (
          <button onClick={onBack} style={{ cursor: 'pointer', padding: '6px 12px', borderRadius: '4px', border: '1px solid #ccc', background: 'none' }}>
            Back to Dashboard
          </button>
        )}
      </header>
      <main style={{ flex: 1, overflow: 'auto', backgroundColor: theme.colors.bgLight }}>
        {children}
      </main>
    </div>
  );
};

export default PortalShell;