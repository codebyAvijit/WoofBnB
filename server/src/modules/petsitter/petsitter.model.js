const mongoose = require("mongoose");

const { PET_SITTER_AMENITIES } = require("../petsitter/petsitter.constants");

const createPetSitterSchema = new mongoose.Schema(
  {
    name: {
      type: String,
      required: true,
      trim: true,
    },

    email: {
      type: String,
      required: true,
      unique: true,
      lowercase: true,
      trim: true,
    },

    phone: {
      type: String,
      required: true,
      trim: true,
    },

    bio: {
      type: String,
      required: true,
      trim: true,
    },

    address: {
      type: String,
      required: true,
      trim: true,
    },

    location: {
      type: {
        type: String,
        enum: ["Point"],
        default: "Point",
        required: true,
      },

      coordinates: {
        type: [Number],
        required: true,
      },
    },

    workingHours: {
      start: {
        type: String,
        required: true,
      },

      end: {
        type: String,
        required: true,
      },
    },

    amenities: [
      {
        type: String,
        enum: PET_SITTER_AMENITIES,
      },
    ],

    profileImage: {
      type: String,
      default: null,
    },
  },
  {
    timestamps: true,
    versionKey: false,
  },
);

createPetSitterSchema.index({
  location: "2dsphere",
});

module.exports = mongoose.model("PetSitter", createPetSitterSchema);
