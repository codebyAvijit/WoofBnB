import { normalize } from "./normalize.js";

function isNormalizedSentinel(value) {
  return value !== null && typeof value === "object" && value.__normalized__ === true;
}

function canonicalSortKey(value) {
  return JSON.stringify(value);
}

/**
 * Compares two already-normalized JSON values, recording every structural or value
 * difference with a JSON-pointer-ish path. `unorderedArrayKeys` names arrays (by their
 * trailing key, e.g. "errors") whose element ORDER is not part of the contract —
 * FluentValidation and Zod evaluate independently-authored rule/field orderings when a
 * single request fails more than one validation rule at once, so requiring literal
 * array order to match there would test an implementation accident, not a real
 * behavioural contract. Every other array (petsitter lists, amenities, nearby results)
 * is compared in strict order, because Node's own contract depends on that order
 * (nearest-first, $near semantics, amenity insertion order).
 */
function diff(nodeValue, dotnetValue, path, differences, unorderedArrayKeys) {
  const bothNormalized = isNormalizedSentinel(nodeValue) && isNormalizedSentinel(dotnetValue);

  if (bothNormalized) {
    if (nodeValue.type !== dotnetValue.type) {
      differences.push({
        path,
        kind: "normalized-type-mismatch",
        node: nodeValue.type,
        dotnet: dotnetValue.type,
      });
    }
    return;
  }

  if (isNormalizedSentinel(nodeValue) !== isNormalizedSentinel(dotnetValue)) {
    differences.push({
      path,
      kind: "presence-mismatch",
      detail: "Field is volatile (normalized) on one side but not present as such on the other.",
      node: nodeValue,
      dotnet: dotnetValue,
    });
    return;
  }

  if (nodeValue === null || dotnetValue === null) {
    if (nodeValue !== dotnetValue) {
      differences.push({ path, kind: "null-mismatch", node: nodeValue, dotnet: dotnetValue });
    }
    return;
  }

  const nodeType = Array.isArray(nodeValue) ? "array" : typeof nodeValue;
  const dotnetType = Array.isArray(dotnetValue) ? "array" : typeof dotnetValue;

  if (nodeType !== dotnetType) {
    differences.push({ path, kind: "type-mismatch", node: nodeType, dotnet: dotnetType });
    return;
  }

  if (nodeType === "array") {
    if (nodeValue.length !== dotnetValue.length) {
      differences.push({
        path,
        kind: "array-length-mismatch",
        node: nodeValue.length,
        dotnet: dotnetValue.length,
      });
      return;
    }

    const trailingKey = path.split(/[.[]/).filter(Boolean).pop();
    const isUnordered = unorderedArrayKeys.has(trailingKey);

    const nodeArray = isUnordered ? [...nodeValue].sort((a, b) => (canonicalSortKey(a) < canonicalSortKey(b) ? -1 : 1)) : nodeValue;
    const dotnetArray = isUnordered ? [...dotnetValue].sort((a, b) => (canonicalSortKey(a) < canonicalSortKey(b) ? -1 : 1)) : dotnetValue;

    for (let i = 0; i < nodeArray.length; i++) {
      diff(nodeArray[i], dotnetArray[i], `${path}[${i}]`, differences, unorderedArrayKeys);
    }
    return;
  }

  if (nodeType === "object") {
    const keys = new Set([...Object.keys(nodeValue), ...Object.keys(dotnetValue)]);

    for (const key of keys) {
      const inNode = Object.hasOwn(nodeValue, key);
      const inDotnet = Object.hasOwn(dotnetValue, key);

      if (!inNode || !inDotnet) {
        differences.push({
          path: `${path}.${key}`,
          kind: "key-presence-mismatch",
          detail: !inNode ? "Present in ASP.NET response, absent in Node response." : "Present in Node response, absent in ASP.NET response.",
        });
        continue;
      }

      diff(nodeValue[key], dotnetValue[key], `${path}.${key}`, differences, unorderedArrayKeys);
    }
    return;
  }

  if (nodeValue !== dotnetValue) {
    differences.push({ path, kind: "value-mismatch", node: nodeValue, dotnet: dotnetValue });
  }
}

export function compareResponses(nodeResult, dotnetResult, { unorderedArrayKeys = [] } = {}) {
  const differences = [];

  if (nodeResult.status !== dotnetResult.status) {
    differences.push({
      path: "$.status",
      kind: "status-mismatch",
      node: nodeResult.status,
      dotnet: dotnetResult.status,
    });
  }

  const normalizedNode = normalize(nodeResult.body);
  const normalizedDotnet = normalize(dotnetResult.body);

  diff(normalizedNode, normalizedDotnet, "$", differences, new Set(unorderedArrayKeys));

  return { pass: differences.length === 0, differences };
}
