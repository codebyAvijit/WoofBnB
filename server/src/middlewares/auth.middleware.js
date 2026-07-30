const authRepository = require("../modules/auth/auth.repository");
const authTokenService = require("../modules/auth/auth.token.service");

const AppError = require("../utils/AppError");
const asyncHandler = require("../utils/asyncHandler");
const HTTP_STATUS = require("../constants/httpStatus");

const authenticate = asyncHandler(async (req, res, next) => {
  const authHeader = req.headers.authorization;

  if (!authHeader || !authHeader.startsWith("Bearer ")) {
    throw new AppError("Authentication required", HTTP_STATUS.UNAUTHORIZED);
  }

  const token = authHeader.split(" ")[1];

  const payload = authTokenService.verifyAccessToken(token);

  const user = await authRepository.findUserById(payload.id);

  if (!user) {
    throw new AppError("User not found", HTTP_STATUS.UNAUTHORIZED);
  }

  if (!user.isActive) {
    throw new AppError("Your account has been disabled", HTTP_STATUS.FORBIDDEN);
  }

  req.user = user;

  next();
});

module.exports = authenticate;
