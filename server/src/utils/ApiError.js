class ApiError {
  constructor(statusCode, message, errors = null) {
    this.success = false;
    this.statusCode = statusCode;
    this.message = message;
    this.errors = errors;
    this.timestamp = new Date().toISOString();
  }
}

module.exports = ApiError;
