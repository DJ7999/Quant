import React from 'react';
import { theme } from '../core/theme';

const AppCard = ({ title, icon, category, onClick }) => {
  return (
    <div 
      onClick={onClick}
      style={{
        background: theme.colors.white, padding: '32px', borderRadius: theme.radius.lg,
        textAlign: 'center', cursor: 'pointer', border: `1px solid ${theme.colors.border}`,
        transition: 'transform 0.2s, box-shadow 0.2s'
      }}
      onMouseEnter={(e) => {
        e.currentTarget.style.transform = 'translateY(-4px)';
        e.currentTarget.style.boxShadow = '0 8px 20px rgba(0,0,0,0.08)';
      }}
      onMouseLeave={(e) => {
        e.currentTarget.style.transform = 'translateY(0)';
        e.currentTarget.style.boxShadow = 'none';
      }}
    >
      <div style={{ fontSize: '40px', marginBottom: '12px' }}>{icon}</div>
      <h3 style={{ margin: '0', fontSize: '16px', color: theme.colors.textMain }}>{title}</h3>
      <div style={{ fontSize: '11px', color: theme.colors.textSecondary, marginTop: '4px', fontWeight: 'bold' }}>{category}</div>
    </div>
  );
};

export default AppCard;