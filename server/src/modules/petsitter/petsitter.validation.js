const { z } = require("zod");

const { PET_SITTER_AMENITIES } = require("../petsitter/petsitter.constants");

const createPetSitterSchema = z.object({
  name: z.string().trim().min(2).max(50),

  email: z.email(),

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

module.exports = {
  createPetSitterSchema,
};
