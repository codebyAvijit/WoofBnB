const express = require("express");
const cors = require("cors");
const helmet = require("helmet");
const AppError = require("./utils/AppError");
const HTTP_STATUS = require("./constants/httpStatus");
const authRoutes = require("./modules/auth/auth.routes");
const petSitterRoutes = require("./modules/petsitter/petsitter.routes");
const errorMiddleware = require("./middlewares/error.middleware");

const app = express();

app.use(express.json());
app.use(helmet());

app.use(
  cors({
    origin: process.env.CLIENT_URL,
    credentials: true,
  }),
);

app.use("/api/auth", authRoutes);

app.use("/api/petsitters", petSitterRoutes);

app.get("/api/health", (req, res) => {
  res.status(200).json({
    success: true,
    message: "WoofBnB API is running",
  });
});

app.get("/test-error", (req, res) => {
  throw new AppError("Testing Global Error Handler", HTTP_STATUS.BAD_REQUEST);
});

app.use(errorMiddleware);

module.exports = app;
