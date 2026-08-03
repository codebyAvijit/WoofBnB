import axios from "axios";
import { toast } from "react-toastify";

import { API_BASE_URL } from "../utils/constants";
import { storage } from "../utils/storage";

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use(
  (config) => {
    const token = storage.getAccessToken();

    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => Promise.reject(error),
);

api.interceptors.response.use(
  (response) => response,

  (error) => {
    const status = error?.response?.status;

    if (status === 401) {
      storage.clear();

      window.location.href = "/login";
    }

    if (status === 403) {
      toast.error("You are not authorized to perform this action.");
    }

    if (status === 500) {
      toast.error("Something went wrong. Please try again.");
    }

    return Promise.reject(error);
  },
);

export default api;
