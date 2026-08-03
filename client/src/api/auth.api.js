import { api } from ".";

export const login = (credentials) => api.post("/auth/login", credentials);

export const getCurrentUser = () => api.get("/auth/me");
