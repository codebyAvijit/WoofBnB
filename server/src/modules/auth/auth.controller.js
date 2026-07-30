const authService = require("./auth.service");

const asyncHandler = require("../../utils/asyncHandler");
const ApiResponse = require("../../utils/ApiResponse");
const HTTP_STATUS = require("../../constants/httpStatus");

const login = asyncHandler(async (req, res) => {
  const result = await authService.login(req.body);

  return res
    .status(HTTP_STATUS.OK)
    .json(new ApiResponse(HTTP_STATUS.OK, "Login successful", result));
});

module.exports = {
  login,
};
