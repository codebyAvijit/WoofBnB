import { NavLink } from "react-router-dom";

function Sidebar() {
  const linkClass = ({ isActive }) =>
    `block rounded-lg px-4 py-3 transition ${
      isActive ? "bg-blue-600 text-white" : "text-slate-700 hover:bg-slate-100"
    }`;

  return (
    <aside className="w-64 border-r bg-white p-5">
      <nav className="space-y-2">
        <NavLink to="/" className={linkClass}>
          Dashboard
        </NavLink>

        <NavLink to="/petsitters" className={linkClass}>
          Pet Sitters
        </NavLink>
      </nav>
    </aside>
  );
}

export default Sidebar;
