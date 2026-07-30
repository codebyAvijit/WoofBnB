const { z } = require("zod");

const { PET_SITTER_AMENITIES } = require("../petsitter/petsitter.constants");

const createPetSitterSchema = z.object({
  name: z.string().trim().min(2).max(50),

  email: z.string().trim().email(),

  phone: z.string().trim().min(10).max(15),

  bio: z.string().trim().min(20).max(1000),

  address: z.string().trim().min(5),

  location: z.object({
    type: z.literal("Point"),

    coordinates: z.array(z.number()).length(2),
  }),

  workingHours: z.object({
    start: z.string(),

    end: z.string(),
  }),

  amenities: z.array(z.enum(PET_SITTER_AMENITIES)),

  profileImage: z.string().optional(),
});

const nearbyPetSitterSchema = z.object({
  lat: z.coerce
    .number()
    .min(-90, "Latitude must be between -90 and 90")
    .max(90, "Latitude must be between -90 and 90"),

  lng: z.coerce
    .number()
    .min(-180, "Longitude must be between -180 and 180")
    .max(180, "Longitude must be between -180 and 180"),

  radius: z.coerce
    .number()
    .positive("Radius must be greater than 0")
    .default(5000),
});

module.exports = {
  createPetSitterSchema,
  nearbyPetSitterSchema,
};
