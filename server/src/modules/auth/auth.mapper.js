const toUserDto = (user) => ({
  id: user._id.toString(),
  name: user.name,
  email: user.email,
  role: user.role,
  lastLogin: user.lastLogin,
  createdAt: user.createdAt,
});

module.exports = {
  toUserDto,
};
