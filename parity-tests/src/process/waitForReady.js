/**
 * Polls a health endpoint until it responds (any HTTP status counts as "the process is
 * alive and answering"), or throws once the timeout elapses. Used identically for both
 * backends so readiness detection can't itself be a source of asymmetry between them.
 */
export async function waitForReady(url, { timeoutMs, pollIntervalMs }, label) {
  const deadline = Date.now() + timeoutMs;
  let lastError;

  while (Date.now() < deadline) {
    try {
      const response = await fetch(url, { signal: AbortSignal.timeout(2000) });
      if (response.ok || response.status < 500) {
        return;
      }
      lastError = new Error(`${label} health check returned ${response.status}`);
    } catch (error) {
      lastError = error;
    }

    await new Promise((resolve) => setTimeout(resolve, pollIntervalMs));
  }

  throw new Error(`${label} did not become ready within ${timeoutMs}ms: ${lastError?.message}`);
}
