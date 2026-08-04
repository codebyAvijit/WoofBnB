import { Link } from "react-router-dom";

import Button from "../common/Button";

function HeroSection() {
  return (
    <section className="bg-gradient-to-br from-sky-50 via-white to-blue-50">
      <div className="mx-auto flex min-h-[85vh] max-w-7xl items-center px-6">
        <div className="max-w-3xl">
          <span className="rounded-full bg-blue-100 px-4 py-2 text-sm font-semibold text-blue-700">
            🐾 Trusted Pet Care Platform
          </span>

          <h1 className="mt-6 text-5xl font-bold leading-tight text-slate-900 md:text-6xl">
            Find the Perfect
            <span className="text-blue-600"> Pet Sitter </span>
            Near You
          </h1>

          <p className="mt-6 text-lg leading-8 text-slate-600">
            Search verified pet sitters, compare amenities, check working hours,
            and keep your furry friends safe while you're away.
          </p>

          <div className="mt-10 flex flex-wrap gap-4">
            <Button>Search Nearby</Button>

            <Link to="/register">
              <Button variant="secondary">Become a Pet Sitter</Button>
            </Link>
          </div>

          <div className="mt-10 flex flex-wrap gap-10">
            <div>
              <h2 className="text-3xl font-bold text-slate-900">500+</h2>

              <p className="text-slate-500">Verified Sitters</p>
            </div>

            <div>
              <h2 className="text-3xl font-bold text-slate-900">10K+</h2>

              <p className="text-slate-500">Happy Pets</p>
            </div>

            <div>
              <h2 className="text-3xl font-bold text-slate-900">24/7</h2>

              <p className="text-slate-500">Customer Support</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

export default HeroSection;
