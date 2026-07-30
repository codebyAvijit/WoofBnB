require("dotenv").config();

const mongoose = require("mongoose");

const connectDB = require("../config/connectDB");

const authRepository = require("../modules/auth/auth.repository");
const { hashPassword } = require("../utils/crypto/password");

const seedAdmin = async () => {
  try {
    await connectDB();

    const existingAdmin = await authRepository.findUserByEmail(
      process.env.ADMIN_EMAIL,
    );

    if (existingAdmin) {
      console.log("Admin already exists.");
      process.exit(0);
    }

    const hashedPassword = await hashPassword(process.env.ADMIN_PASSWORD);

    await authRepository.createUser({
      name: process.env.ADMIN_NAME,
      email: process.env.ADMIN_EMAIL,
      password: hashedPassword,
      role: "admin",
    });

    console.log("Admin seeded successfully.");

    process.exit(0);
  } catch (error) {
    console.error("Failed to seed admin:", error);

    process.exit(1);
  } finally {
    await mongoose.connection.close();
  }
};

seedAdmin();
