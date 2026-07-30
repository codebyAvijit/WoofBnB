const petSitterRepository = require("./petsitter.repository");
const { toPetSitterDto } = require("./petsitter.mapper");

const AppError = require("../../utils/AppError");
const HTTP_STATUS = require("../../constants/httpStatus");

const registerPetSitter = async (petSitterData) => {
  const existingPetSitter = await petSitterRepository.findPetSitterByEmail(
    petSitterData.email,
  );

  if (existingPetSitter) {
    throw new AppError(
      "A pet sitter with this email already exists",
      HTTP_STATUS.CONFLICT,
    );
  }

  const petSitter = await petSitterRepository.createPetSitter(petSitterData);

  return toPetSitterDto(petSitter);
};

const getAllPetSitters = async () => {
  const petSitters = await petSitterRepository.findAllPetSitters();

  return petSitters.map(toPetSitterDto);
};

module.exports = {
  registerPetSitter,
  getAllPetSitters,
};
