import { Navigate } from "react-router-dom";

import Card from "../../../components/common/Card";

import LoginForm from "../components/LoginForm";

import { storage } from "../../../utils/storage";

function Login() {
  if (storage.isAuthenticated()) {
    return <Navigate to="/" replace />;
  }

  return (
    <Card className="w-full max-w-md">
      <div className="mb-8 text-center">
        <h1 className="text-3xl font-bold text-slate-800">Welcome Back</h1>

        <p className="mt-2 text-slate-500">Login to continue to WoofBnB</p>
      </div>

      <LoginForm />
    </Card>
  );
}

export default Login;
