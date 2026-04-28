import React, { useState } from 'react';
import PortalShell from './components/PortalShell';
import AppCard from './components/AppCard';
import { QUANT_APPS } from './apps/registry';
import { theme } from './core/theme';

function App() {
  const [activeAppId, setActiveAppId] = useState(null);

  // Find the selected app from the registry
  const activeApp = QUANT_APPS.find((app) => app.id === activeAppId);

  // Render the App Launcher (Dashboard)
  const renderDashboard = () => (
    <div style={{ padding: '40px 20px', maxWidth: '1200px', margin: '0 auto' }}>
      <header style={{ marginBottom: '40px' }}>
        <h1 style={{ fontSize: '32px', fontWeight: '700', color: theme.colors.textMain }}>
          Quant Workspace
        </h1>
        <p style={{ color: theme.colors.textSecondary }}>
          Select a system module to begin data analysis or management.
        </p>
      </header>

      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', 
        gap: '24px' 
      }}>
        {QUANT_APPS.map((app) => (
          <AppCard 
            key={app.id} 
            {...app} 
            onClick={() => setActiveAppId(app.id)} 
          />
        ))}
      </div>
    </div>
  );

  return (
    <PortalShell 
      title={activeApp?.title} 
      onBack={activeAppId ? () => setActiveAppId(null) : null}
    >
      {activeApp ? (
        /* Render the actual App Component from the registry */
        <activeApp.component />
      ) : (
        /* Render the Tile Grid */
        renderDashboard()
      )}
    </PortalShell>
  );
}

export default App;