import React from "react";
import ReactDOM from "react-dom/client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ToastContainer } from "react-toastify";
import ErrorBoundary from "./components/common/ErrorBoundary/ErrorBoundary";
import "react-toastify/dist/ReactToastify.css";
import { SearchProvider } from "./features/search/context/SearchContext";
import "./assets/styles/index.css";
// import "leaflet/dist/leaflet.css";
import App from "./App";
import { AuthProvider } from "./context/AuthContext";
import GoogleMapProvider from "./components/map/GoogleMapProvider";
const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <GoogleMapProvider>
        <AuthProvider>
          <SearchProvider>
            <ErrorBoundary>
              <App />
            </ErrorBoundary>
          </SearchProvider>

          <ToastContainer
            position="top-right"
            autoClose={2500}
            hideProgressBar={false}
            newestOnTop
            closeOnClick
            pauseOnHover
            draggable
            theme="colored"
          />
        </AuthProvider>
      </GoogleMapProvider>
    </QueryClientProvider>
  </React.StrictMode>,
);
