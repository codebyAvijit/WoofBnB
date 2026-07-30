const User = require("./auth.model");

const createUser = (userData) => {
  return User.create(userData);
};

const findUserByEmail = (email) => {
  return User.findOne({ email });
};

const findUserById = (id) => {
  return User.findById(id);
};

const updateUserLastLogin = (id) => {
  return User.findByIdAndUpdate(
    id,
    {
      lastLogin: new Date(),
    },
    {
      new: true,
    },
  );
};

const authRepository = {
  createUser,
  findUserByEmail,
  findUserById,
  updateUserLastLogin,
};

module.exports = authRepository;
