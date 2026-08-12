/**
 * Thin wrapper around fetch. Deliberately returns the raw parsed body alongside the
 * status code and a couple of headers relevant to comparison (content-type) — nothing
 * here interprets or judges the response, that's compare.js's job.
 */
export async function callApi(baseUrl, { method = "GET", path, query, headers = {}, body }) {
  const url = new URL(baseUrl + path);

  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null) {
        url.searchParams.set(key, String(value));
      }
    }
  }

  const response = await fetch(url, {
    method,
    headers: {
      ...(body !== undefined ? { "Content-Type": "application/json" } : {}),
      ...headers,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  const rawText = await response.text();
  let parsedBody = null;
  let isJson = false;

  try {
    parsedBody = rawText.length > 0 ? JSON.parse(rawText) : null;
    isJson = true;
  } catch {
    parsedBody = rawText;
  }

  return {
    status: response.status,
    contentType: response.headers.get("content-type"),
    isJson,
    body: parsedBody,
    rawText,
  };
}
