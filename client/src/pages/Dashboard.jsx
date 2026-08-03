import Card from "../components/common/Card";

import { useAuth } from "../context/AuthContext";

function Dashboard() {
  const { user } = useAuth();

  return (
    <div className="space-y-6">
      <Card>
        <h1 className="text-3xl font-bold">
          Welcome back,
          <span className="text-blue-600"> {user?.name}</span>
        </h1>

        <p className="mt-2 text-slate-500">
          Manage your pet sitters from the dashboard.
        </p>
      </Card>

      <div className="grid gap-6 md:grid-cols-3">
        <Card>
          <h2 className="text-xl font-semibold">Total Pet Sitters</h2>

          <p className="mt-4 text-5xl font-bold">--</p>
        </Card>

        <Card>
          <h2 className="text-xl font-semibold">Nearby Searches</h2>

          <p className="mt-4 text-5xl font-bold">--</p>
        </Card>

        <Card>
          <h2 className="text-xl font-semibold">Active Users</h2>

          <p className="mt-4 text-5xl font-bold">--</p>
        </Card>
      </div>
    </div>
  );
}

export default Dashboard;
