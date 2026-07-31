const express = require("express");

const petSitterController = require("./petsitter.controller");

const validate = require("../../middlewares/validate.middleware");
const {
  createPetSitterSchema,
  nearbyPetSitterSchema,
} = require("./petsitter.validation");

const router = express.Router();

/**
 * @swagger
 * /petsitters:
 *   post:
 *     tags:
 *       - Pet Sitters
 *     summary: Register Pet Sitter
 *     description: Creates a new pet sitter in the system.
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             $ref: '#/components/schemas/PetSitterRequest'
 *     responses:
 *       201:
 *         description: Pet sitter registered successfully
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/ApiResponse'
 *             example:
 *               success: true
 *               statusCode: 201
 *               message: Pet sitter registered successfully
 *               data:
 *                 id: "6894fef0a7ab123456789abc"
 *                 name: "John Doe"
 *                 email: "john@example.com"
 *                 phone: "9876543210"
 *                 bio: "Professional pet sitter with 5 years of experience."
 *                 address: "Connaught Place, New Delhi"
 *                 location:
 *                   type: "Point"
 *                   coordinates:
 *                     - 77.209
 *                     - 28.6139
 *                 workingHours:
 *                   start: "09:00"
 *                   end: "18:00"
 *                 amenities:
 *                   - "Dog Walking"
 *                   - "Indoor Stay"
 *                 profileImage: null
 *                 createdAt: "2026-07-31T09:30:00.000Z"
 *                 updatedAt: "2026-07-31T09:30:00.000Z"
 *               timestamp: "2026-07-31T09:30:00.000Z"
 *       400:
 *         description: Validation failed
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/ApiError'
 *       500:
 *         description: Internal Server Error
 */
router.post(
  "/",
  validate(createPetSitterSchema),
  petSitterController.registerPetSitter,
);

/**
 * @swagger
 * /petsitters:
 *   get:
 *     tags:
 *       - Pet Sitters
 *     summary: Get All Pet Sitters
 *     description: Returns a list of all registered pet sitters.
 *     responses:
 *       200:
 *         description: List of pet sitters retrieved successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: array
 *               items:
 *                 $ref: '#/components/schemas/PetSitterResponse'
 *       500:
 *         description: Internal Server Error
 */
router.get("/", petSitterController.getAllPetSitters);

/**
 * @swagger
 * /petsitters/nearby:
 *   get:
 *     tags:
 *       - Pet Sitters
 *     summary: Get Nearby Pet Sitters
 *     description: Returns pet sitters within the specified search radius.
 *     parameters:
 *       - in: query
 *         name: lat
 *         required: true
 *         schema:
 *           type: number
 *           minimum: -90
 *           maximum: 90
 *         example: 28.6139
 *         description: Latitude of the search location.
 *       - in: query
 *         name: lng
 *         required: true
 *         schema:
 *           type: number
 *           minimum: -180
 *           maximum: 180
 *         example: 77.209
 *         description: Longitude of the search location.
 *       - in: query
 *         name: radius
 *         required: false
 *         schema:
 *           type: number
 *           default: 5000
 *         example: 5000
 *         description: Search radius in meters.
 *     responses:
 *       200:
 *         description: Nearby pet sitters retrieved successfully
 *         content:
 *           application/json:
 *             schema:
 *               type: array
 *               items:
 *                 $ref: '#/components/schemas/PetSitterResponse'
 *       400:
 *         description: Invalid query parameters
 *         content:
 *           application/json:
 *             schema:
 *               $ref: '#/components/schemas/ApiError'
 *       500:
 *         description: Internal Server Error
 */
router.get(
  "/nearby",
  validate(nearbyPetSitterSchema, "query"),
  petSitterController.getNearbyPetSitters,
);

module.exports = router;
