const ApiError = require("../utils/ApiError");
const HTTP_STATUS = require("../constants/httpStatus");

const errorMiddleware = (err, req, res, next) => {
  const statusCode = err.statusCode || HTTP_STATUS.INTERNAL_SERVER_ERROR;

  const response = new ApiError(
    statusCode,
    err.message || "Something went wrong",
  );

  if (process.env.NODE_ENV === "development") {
    response.stack = err.stack;
  }

  return res.status(statusCode).json(response);
};

module.exports = errorMiddleware;
