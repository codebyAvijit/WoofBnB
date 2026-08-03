import { z } from "zod";

export const loginSchema = z.object({
  email: z.string().trim().email("Please enter a valid email address."),

  password: z.string().trim().min(6, "Password must be at least 6 characters."),
});

export default loginSchema;
