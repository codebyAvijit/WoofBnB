import { Link } from "react-router-dom";

import Button from "../components/common/Button";

function Home() {
  return (
    <div className="mx-auto flex min-h-screen max-w-7xl flex-col items-center justify-center px-6">
      <h1 className="mb-4 text-center text-5xl font-bold">WoofBnB</h1>

      <p className="mb-10 max-w-xl text-center text-slate-600">
        Find trusted pet sitters near you or become a pet sitter.
      </p>

      <div className="flex flex-wrap justify-center gap-4">
        <Link to="/register">
          <Button>Become a Pet Sitter</Button>
        </Link>

        <Link to="/login">
          <Button variant="secondary">Admin Login</Button>
        </Link>
      </div>
    </div>
  );
}

export default Home;
