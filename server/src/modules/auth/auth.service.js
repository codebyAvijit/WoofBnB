const { comparePassword } = require("../../utils/crypto/password");

const authRepository = require("./auth.repository");
const authTokenService = require("./auth.token.service");
const { toUserDto } = require("./auth.mapper");

const AppError = require("../../utils/AppError");
const HTTP_STATUS = require("../../constants/httpStatus");

const login = async ({ email, password }) => {
  const user = await authRepository.findUserByEmail(email);

  if (!user) {
    throw new AppError("Invalid email or password", HTTP_STATUS.UNAUTHORIZED);
  }

  if (!user.isActive) {
    throw new AppError("Your account has been disabled", HTTP_STATUS.FORBIDDEN);
  }

  const isPasswordValid = await comparePassword(password, user.password);

  if (!isPasswordValid) {
    throw new AppError("Invalid email or password", HTTP_STATUS.UNAUTHORIZED);
  }

  const updatedUser = await authRepository.updateUserLastLogin(user._id);

  const accessToken = authTokenService.generateAccessToken(updatedUser);

  return {
    user: toUserDto(updatedUser),
    accessToken,
  };
};

const getCurrentUser = async (userId) => {
  const user = await authRepository.findUserById(userId);

  if (!user) {
    throw new AppError("User not found", HTTP_STATUS.NOT_FOUND);
  }

  return toUserDto(user);
};

module.exports = {
  login,
  getCurrentUser,
};
