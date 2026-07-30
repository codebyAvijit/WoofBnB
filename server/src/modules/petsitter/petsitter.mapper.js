const toPetSitterDto = (petSitter) => ({
  id: petSitter._id.toString(),
  name: petSitter.name,
  email: petSitter.email,
  phone: petSitter.phone,
  bio: petSitter.bio,
  address: petSitter.address,
  location: petSitter.location,
  workingHours: petSitter.workingHours,
  amenities: petSitter.amenities,
  profileImage: petSitter.profileImage,
  createdAt: petSitter.createdAt,
  updatedAt: petSitter.updatedAt,
});

module.exports = {
  toPetSitterDto,
};
