const petSitterService = require("./petsitter.service");

const asyncHandler = require("../../utils/asyncHandler");
const ApiResponse = require("../../utils/ApiResponse");
const HTTP_STATUS = require("../../constants/httpStatus");

const registerPetSitter = asyncHandler(async (req, res) => {
  const petSitter = await petSitterService.registerPetSitter(req.body);

  return res
    .status(HTTP_STATUS.CREATED)
    .json(
      new ApiResponse(
        HTTP_STATUS.CREATED,
        "Pet sitter registered successfully",
        petSitter,
      ),
    );
});

const getAllPetSitters = asyncHandler(async (req, res) => {
  const petSitters = await petSitterService.getAllPetSitters();

  return res
    .status(HTTP_STATUS.OK)
    .json(
      new ApiResponse(
        HTTP_STATUS.OK,
        "Pet sitters fetched successfully",
        petSitters,
      ),
    );
});

const getNearbyPetSitters = asyncHandler(async (req, res) => {
  const petSitters = await petSitterService.getNearbyPetSitters(req.query);

  return res
    .status(HTTP_STATUS.OK)
    .json(
      new ApiResponse(
        HTTP_STATUS.OK,
        "Nearby pet sitters fetched successfully",
        petSitters,
      ),
    );
});

module.exports = {
  registerPetSitter,
  getAllPetSitters,
  getNearbyPetSitters,
};
