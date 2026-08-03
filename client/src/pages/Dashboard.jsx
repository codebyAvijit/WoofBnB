import Button from "../components/common/Button";
import Card from "../components/common/Card";

import { useAuth } from "../context/AuthContext";
import useLogout from "../features/auth/hooks/useLogout";

function Dashboard() {
  const { user } = useAuth();

  const logout = useLogout();

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-100 p-6">
      <Card className="w-full max-w-xl space-y-6 text-center">
        <h1 className="text-4xl font-bold text-slate-800">
          Welcome to WoofBnB
        </h1>

        <div className="space-y-2">
          <p className="text-lg font-medium text-slate-700">{user?.name}</p>

          <p className="text-slate-500">{user?.email}</p>

          <p className="text-sm text-slate-400">Role: {user?.role}</p>
        </div>

        <div className="flex justify-center">
          <Button onClick={logout}>Logout</Button>
        </div>
      </Card>
    </div>
  );
}

export default Dashboard;
