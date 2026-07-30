const express = require("express");

const authController = require("./auth.controller");
const { loginSchema } = require("./auth.validation");

const validate = require("../../middlewares/validate.middleware");

const router = express.Router();

router.post("/login", validate(loginSchema), authController.login);

module.exports = router;
