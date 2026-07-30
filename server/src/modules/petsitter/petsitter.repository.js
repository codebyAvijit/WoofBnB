const PetSitter = require("./petsitter.model");

const createPetSitter = async (petSitterData) =>
  PetSitter.create(petSitterData);

const findPetSitterByEmail = async (email) => PetSitter.findOne({ email });

const findAllPetSitters = async () => PetSitter.find().sort({ createdAt: -1 });

const findPetSitterById = async (petSitterId) =>
  PetSitter.findById(petSitterId);

const findNearbyPetSitters = async (longitude, latitude, radius) =>
  PetSitter.find({
    location: {
      $near: {
        $geometry: {
          type: "Point",
          coordinates: [longitude, latitude],
        },
        $maxDistance: radius,
      },
    },
  });

module.exports = {
  createPetSitter,
  findPetSitterByEmail,
  findAllPetSitters,
  findPetSitterById,
  findNearbyPetSitters,
};
