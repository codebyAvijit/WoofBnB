import { z } from "zod";

const phoneRegex = /^[6-9]\d{9}$/;

const nameRegex = /^[A-Za-z\s'-]+$/;

export const petSitterSchema = z
  .object({
    name: z
      .string()
      .trim()
      .min(2, "Name must be at least 2 characters")
      .max(50, "Name cannot exceed 50 characters")
      .regex(
        nameRegex,
        "Name can only contain letters, spaces, apostrophes and hyphens",
      ),

    email: z
      .string()
      .trim()
      .email("Enter a valid email address")
      .max(100, "Email is too long"),

    phone: z
      .string()
      .trim()
      .regex(phoneRegex, "Enter a valid 10-digit mobile number"),

    address: z
      .string()
      .trim()
      .min(5, "Address must be at least 5 characters")
      .max(200, "Address cannot exceed 200 characters"),

    bio: z
      .string()
      .trim()
      .min(20, "Bio must be at least 20 characters")
      .max(1000, "Bio cannot exceed 1000 characters"),

    startTime: z.string().min(1, "Start time is required"),

    endTime: z.string().min(1, "End time is required"),

    amenities: z.array(z.string()).min(1, "Select at least one amenity"),
  })
  .refine((data) => data.startTime < data.endTime, {
    message: "End time must be later than start time",
    path: ["endTime"],
  });
