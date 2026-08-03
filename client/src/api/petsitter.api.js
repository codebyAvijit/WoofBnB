import { api } from ".";

export const getPetSitters = () => api.get("/petsitters");

export const createPetSitter = (payload) => api.post("/petsitters", payload);

export const getNearbyPetSitters = (params) =>
  api.get("/petsitters/nearby", {
    params,
  });
