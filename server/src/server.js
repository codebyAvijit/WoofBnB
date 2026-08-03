const dotenv = require("dotenv");

// Load environment variables FIRST
dotenv.config();

const app = require("./app");
const connectDB = require("./config/connectDB");

const PORT = process.env.PORT || 5000;

const startServer = async () => {
  await connectDB();

  app.listen(PORT, "0.0.0.0", () => {
    console.log(`Server running on port ${PORT}`);
  });
};

startServer();
