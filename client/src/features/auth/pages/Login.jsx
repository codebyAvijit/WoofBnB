import { Navigate } from "react-router-dom";

import Card from "../../../components/common/Card";
import LoginForm from "../components/LoginForm";

import { useAuth } from "../../../context/AuthContext";
import Loader from "../../../components/common/Loader";

function Login() {
  const { user, loading } = useAuth();

  if (loading) {
    return <Loader />;
  }

  if (user) {
    return <Navigate to="/dashboard/petsitters" replace />;
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
