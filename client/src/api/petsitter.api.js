import api from "./axios";

export const registerPetSitter = (data) => api.post("/petsitters", data);

export const getAllPetSitters = () => api.get("/petsitters");

export const getNearbyPetSitters = (params) =>
  api.get("/petsitters/nearby", {
    params,
  });
