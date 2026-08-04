import { Outlet } from "react-router-dom";

import Footer from "../components/layout/Footer/Footer";
import PublicNavbar from "../components/layout/Navbar/PublicNavbar";

import { SearchProvider } from "../features/search/context/SearchContext";

function PublicLayout() {
  return (
    <SearchProvider>
      <div className="flex min-h-screen flex-col bg-slate-50">
        <PublicNavbar />

        <main className="flex-1">
          <Outlet />
        </main>

        <Footer />
      </div>
    </SearchProvider>
  );
}

export default PublicLayout;
