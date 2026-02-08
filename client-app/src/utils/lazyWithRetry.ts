import { lazy, ComponentType } from 'react';

/**
 * Lazy loading with retry mechanism
 *
 * Solves the "Failed to fetch dynamically imported module" error
 * that occurs when:
 * 1. User has old version cached
 * 2. New deployment with different chunk hashes
 * 3. Browser tries to load old chunk that no longer exists
 *
 * This function retries loading and if it fails, forces a page reload
 * to get the new version.
 */
export function lazyWithRetry<T extends ComponentType<any>>(
  componentImport: () => Promise<{ default: T }>
): React.LazyExoticComponent<T> {
  return lazy(async () => {
    const pageHasAlreadyBeenForceRefreshed = JSON.parse(
      window.sessionStorage.getItem('page-has-been-force-refreshed') || 'false'
    );

    try {
      const component = await componentImport();

      // If successful, clear the refresh flag
      window.sessionStorage.setItem('page-has-been-force-refreshed', 'false');

      return component;
    } catch (error) {
      if (!pageHasAlreadyBeenForceRefreshed) {
        // Mark that we are going to refresh
        window.sessionStorage.setItem('page-has-been-force-refreshed', 'true');

        // Reload the page to get the new chunks
        console.log('Failed to load chunk, reloading page to get new version...');
        window.location.reload();

        // Return a placeholder to satisfy TypeScript
        // (page will reload before this is used)
        return new Promise<{ default: T }>(() => {});
      }

      // If we already tried refreshing and it still fails, throw the error
      throw error;
    }
  });
}
