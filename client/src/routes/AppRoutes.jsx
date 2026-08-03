import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import PublicLayout from "../layouts/PublicLayout";
import AuthLayout from "../layouts/AuthLayout";
import MainLayout from "../layouts/MainLayout";

import Home from "../pages/Home";
import Dashboard from "../pages/Dashboard";
import NotFound from "../pages/NotFound";
import Unauthorized from "../pages/Unauthorized";

import Login from "../features/auth/pages/Login";
import RegisterPetSitter from "../features/petsitter/pages/RegisterPetSitter";
import PetSitters from "../features/petsitter/pages/PetSitters";

import ProtectedRoute from "./ProtectedRoute";

function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        {/* ---------- Public Routes ---------- */}

        <Route element={<PublicLayout />}>
          <Route path="/" element={<Home />} />

          <Route path="/register" element={<RegisterPetSitter />} />
        </Route>

        {/* ---------- Authentication ---------- */}

        <Route element={<AuthLayout />}>
          <Route path="/login" element={<Login />} />
        </Route>

        {/* ---------- Protected Admin Routes ---------- */}

        <Route
          element={
            <ProtectedRoute>
              <MainLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/dashboard" element={<Dashboard />} />

          <Route path="/dashboard/petsitters" element={<PetSitters />} />
        </Route>

        <Route path="/unauthorized" element={<Unauthorized />} />

        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
}

export default AppRoutes;
