import { Link } from "react-router-dom";

import Button from "../Button";
import { ROUTES } from "../../../constants/routes";

function ServerError() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-100 px-6">
      <div className="max-w-lg rounded-xl bg-white p-8 text-center shadow-xl">
        <h1 className="mb-4 text-5xl font-bold text-red-600">500</h1>

        <h2 className="mb-3 text-2xl font-semibold text-slate-800">
          Internal Server Error
        </h2>

        <p className="mb-8 text-slate-600">
          Something went wrong on our end. Please try again later.
        </p>

        <div className="flex justify-center gap-4">
          <Button onClick={() => window.location.reload()}>Retry</Button>

          <Link to={ROUTES.PUBLIC.HOME}>
            <Button variant="secondary">Home</Button>
          </Link>
        </div>
      </div>
    </div>
  );
}

export default ServerError;
