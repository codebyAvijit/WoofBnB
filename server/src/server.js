const dotenv = require("dotenv");
const app = require("./app.js");
const connectDB = require("./config/connectDB.js");

dotenv.config();

const PORT = process.env.PORT || 5000;

const startServer = async () => {
  await connectDB();

  app.listen(PORT, "0.0.0.0", () => {
    console.log(`Server running on port ${PORT}`);
  });
};

startServer();
