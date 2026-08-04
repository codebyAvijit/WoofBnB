import { Link } from "react-router-dom";

import Button from "../Button";
import { ROUTES } from "../../../constants/routes";

function Unauthorized() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-100 px-6">
      <div className="max-w-lg rounded-xl bg-white p-8 text-center shadow-xl">
        <h1 className="mb-4 text-5xl font-bold text-blue-600">401</h1>

        <h2 className="mb-3 text-2xl font-semibold text-slate-800">
          Unauthorized
        </h2>

        <p className="mb-8 text-slate-600">Please sign in to continue.</p>

        <Link to={ROUTES.PUBLIC.LOGIN}>
          <Button>Login</Button>
        </Link>
      </div>
    </div>
  );
}

export default Unauthorized;
