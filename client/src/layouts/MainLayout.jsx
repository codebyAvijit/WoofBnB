import { Outlet } from "react-router-dom";

function MainLayout() {
  return (
    <main className="min-h-screen bg-slate-100">
      <Outlet />
    </main>
  );
}

export default MainLayout;
