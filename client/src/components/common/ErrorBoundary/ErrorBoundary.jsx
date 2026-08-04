import { ErrorBoundary as ReactErrorBoundary } from "react-error-boundary";

import ErrorFallback from "./ErrorFallback";

function ErrorBoundary({ children }) {
  return (
    <ReactErrorBoundary
      FallbackComponent={ErrorFallback}
      onError={(error, info) => {
        console.error("Global Error Boundary", error, info);
      }}
    >
      {children}
    </ReactErrorBoundary>
  );
}

export default ErrorBoundary;
