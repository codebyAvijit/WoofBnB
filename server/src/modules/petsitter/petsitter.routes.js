const express = require("express");

const petSitterController = require("./petsitter.controller");

const validate = require("../../middlewares/validate.middleware");
const authenticate = require("../../middlewares/auth.middleware");
const {
  createPetSitterSchema,
  nearbyPetSitterSchema,
} = require("./petsitter.validation");

const router = express.Router();

router.post(
  "/",
  validate(createPetSitterSchema),
  petSitterController.registerPetSitter,
);

router.get(
  "/nearby",
  validate(nearbyPetSitterSchema, "query"),
  petSitterController.getNearbyPetSitters,
);

router.get("/", petSitterController.getAllPetSitters);

module.exports = router;
