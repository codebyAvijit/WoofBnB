const PetSitter = require("./petsitter.model");

const createPetSitter = async (petSitterData) =>
  PetSitter.create(petSitterData);

const findPetSitterByEmail = async (email) => PetSitter.findOne({ email });

const findAllPetSitters = async () => PetSitter.find().sort({ createdAt: -1 });

const findPetSitterById = async (petSitterId) =>
  PetSitter.findById(petSitterId);

module.exports = {
  createPetSitter,
  findPetSitterByEmail,
  findAllPetSitters,
  findPetSitterById,
};
