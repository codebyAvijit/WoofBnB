import Button from "../../common/Button";

import { useAuth } from "../../../context/AuthContext";
import useLogout from "../../../features/auth/hooks/useLogout";

function AdminNavbar() {
  const { user } = useAuth();

  const logout = useLogout();

  return (
    <header className="flex h-16 items-center justify-between border-b bg-white px-6 shadow-sm">
      <h1 className="text-2xl font-bold text-blue-600">WoofBnB</h1>

      <div className="flex items-center gap-5">
        <div className="text-right">
          <p className="font-semibold text-slate-700">{user?.name}</p>

          <p className="text-sm text-slate-500">{user?.email}</p>
        </div>

        <Button variant="secondary" onClick={logout}>
          Logout
        </Button>
      </div>
    </header>
  );
}

export default AdminNavbar;
