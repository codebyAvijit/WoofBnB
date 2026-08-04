import Button from "../Button";

function ErrorFallback({ error, resetErrorBoundary }) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-100 px-6">
      <div className="max-w-lg rounded-xl bg-white p-8 text-center shadow-xl">
        <h1 className="mb-4 text-3xl font-bold text-red-600">
          Something went wrong
        </h1>

        <p className="mb-6 text-slate-600">
          An unexpected error occurred while rendering this page.
        </p>

        {import.meta.env.DEV && error && (
          <pre className="mb-6 overflow-auto rounded-lg bg-slate-900 p-4 text-left text-sm text-red-300">
            {error.message}
          </pre>
        )}

        <Button onClick={resetErrorBoundary}>Try Again</Button>
      </div>
    </div>
  );
}

export default ErrorFallback;
