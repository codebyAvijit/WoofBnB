const HTTP_STATUS = require("../constants/httpStatus");
const ApiError = require("../utils/ApiError");

const validate = (schema, source = "body") => {
  return (req, res, next) => {
    const result = schema.safeParse(req[source]);

    if (!result.success) {
      const errors = result.error.issues.map((issue) => ({
        field: issue.path.join("."),
        message: issue.message,
      }));

      return res
        .status(HTTP_STATUS.BAD_REQUEST)
        .json(
          new ApiError(HTTP_STATUS.BAD_REQUEST, "Validation Failed", errors),
        );
    }

    req[source] = result.data;

    next();
  };
};

module.exports = validate;
