import { theme } from '../core/theme';

export const commonStyles = {
  // The main container for any tool within an app
  appContainer: {
    padding: theme.spacing.xl,
    maxWidth: '800px',
    margin: '0 auto',
  },
  // The white "App Surface" card
  surface: {
    backgroundColor: theme.white,
    border: `1px solid ${theme.border}`,
    borderRadius: theme.radius.lg,
    padding: theme.spacing.xl,
    textAlign: 'center',
    boxShadow: theme.shadows.card,
  },
  // Standard Quant Button
  button: (disabled, isError) => ({
    padding: '12px 24px',
    fontSize: '16px',
    fontWeight: '600',
    color: theme.white,
    backgroundColor: disabled ? '#ccc' : isError ? theme.error : theme.primary,
    border: 'none',
    borderRadius: theme.radius.md,
    cursor: disabled ? 'not-allowed' : 'pointer',
    transition: 'all 0.2s',
  }),
  // Status Alerts
  alert: (type) => ({
    marginTop: theme.spacing.lg,
    padding: '12px',
    borderRadius: theme.radius.sm,
    backgroundColor: type === 'error' ? '#fff1f0' : '#f6ffed',
    border: `1px solid ${type === 'error' ? '#ffa39e' : '#b7eb8f'}`,
    color: type === 'error' ? theme.error : theme.success,
    fontSize: '14px',
  })
};