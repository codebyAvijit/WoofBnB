import { Link } from "react-router-dom";

import Button from "../Button";
import { ROUTES } from "../../../constants/routes";

function RouteError() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-100 px-6">
      <div className="max-w-lg rounded-xl bg-white p-8 text-center shadow-xl">
        <h1 className="mb-4 text-5xl font-bold text-red-600">404</h1>

        <h2 className="mb-3 text-2xl font-semibold text-slate-800">
          Page Not Found
        </h2>

        <p className="mb-8 text-slate-600">
          The page you're looking for doesn't exist or may have been moved.
        </p>

        <Link to={ROUTES.PUBLIC.HOME}>
          <Button>Go Back Home</Button>
        </Link>
      </div>
    </div>
  );
}

export default RouteError;
