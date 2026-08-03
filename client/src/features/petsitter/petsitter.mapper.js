export function mapCreatePetSitterPayload(formData) {
  return {
    name: formData.name.trim(),

    email: formData.email.trim().toLowerCase(),

    phone: formData.phone.trim(),

    bio: formData.bio.trim(),

    address: formData.address.trim(),

    location: {
      type: "Point",

      // Temporary coordinates
      // Will later come from Google Maps
      coordinates: [77.209, 28.6139],
    },

    workingHours: {
      start: formData.startTime,
      end: formData.endTime,
    },

    amenities: formData.amenities,

    profileImage: "",
  };
}
