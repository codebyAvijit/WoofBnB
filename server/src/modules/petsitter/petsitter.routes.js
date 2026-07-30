const express = require("express");

const petSitterController = require("./petsitter.controller");
const { createPetSitterSchema } = require("./petsitter.validation");

const validate = require("../../middlewares/validate.middleware");
const authenticate = require("../../middlewares/auth.middleware");

const router = express.Router();

router.post(
  "/",
  validate(createPetSitterSchema),
  petSitterController.registerPetSitter,
);

router.get("/", petSitterController.getAllPetSitters);

module.exports = router;
