import { Link } from "react-router-dom";

import Button from "../../common/Button";

function PublicNavbar() {
  return (
    <header className="sticky top-0 z-50 border-b bg-white">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-6">
        <Link to="/" className="text-2xl font-bold text-blue-600">
          WoofBnB
        </Link>

        <nav className="hidden items-center gap-8 md:flex">
          <Link
            to="/"
            className="font-medium text-slate-700 hover:text-blue-600"
          >
            Home
          </Link>

          <Link
            to="/register"
            className="font-medium text-slate-700 hover:text-blue-600"
          >
            Become a Pet Sitter
          </Link>

          <Link to="/login">
            <Button>Admin Login</Button>
          </Link>
        </nav>
      </div>
    </header>
  );
}

export default PublicNavbar;
