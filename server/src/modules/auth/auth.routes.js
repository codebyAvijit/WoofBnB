const express = require("express");

const authController = require("./auth.controller");
const { loginSchema } = require("./auth.validation");
const authenticate = require("../../middlewares/auth.middleware");

const validate = require("../../middlewares/validate.middleware");

const router = express.Router();

router.post("/login", validate(loginSchema), authController.login);

router.get("/me", authenticate, authController.getCurrentUser);

module.exports = router;
