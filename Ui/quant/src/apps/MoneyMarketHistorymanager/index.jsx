import React, { useState, useRef } from 'react';
import { api } from '../../core/api'; 
import { commonStyles } from '../../components/common';

const MoneyMarketHistoryManager = () => {
  const [status, setStatus] = useState('idle');
  const [message, setMessage] = useState('');
  const fileInputRef = useRef(null);

  const handleFileUpload = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    setStatus('uploading');
    setMessage('Processing CSV data...');

    try {
      // API call using our configurable client
      await api.upload('/api/money-market-history/upload-csv', formData);
      
      setStatus('success');
      setMessage(`Successfully imported records.`);
    } catch (error) {
      setStatus('error');
      setMessage(error.message || 'Error uploading file.');
    } finally {
      // Clear the input so you can upload the same file again if needed
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  return (
    <div style={commonStyles.appContainer}>
      <div style={commonStyles.surface}>
        {/* Icon and Title */}
        <div style={{ fontSize: '48px', marginBottom: '16px' }}>📂</div>
        <h2 style={{ marginBottom: '8px' }}>Money Market History Manager</h2>
        <p style={{ color: '#666', marginBottom: '32px' }}>
          Select a CSV file to upload historical Money Market data to the Quant database.
        </p>

        {/* FIX: The hidden input MUST be present in the DOM 
            and linked via the 'ref' attribute.
        */}
        <input 
          type="file" 
          accept=".csv" 
          ref={fileInputRef} 
          onChange={handleFileUpload} 
          style={{ display: 'none' }} 
        />

        <button 
          onClick={() => fileInputRef.current?.click()}
          disabled={status === 'uploading'}
          style={commonStyles.button(status === 'uploading')}
        >
          {status === 'uploading' ? 'Uploading...' : 'Upload CSV Data'}
        </button>

        {message && (
          <div style={commonStyles.alert(status === 'error' ? 'error' : 'success')}>
            {message}
          </div>
        )}
      </div>
    </div>
  );
};

export default MoneyMarketHistoryManager;