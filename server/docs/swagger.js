const swaggerJsdoc = require("swagger-jsdoc");
const swaggerUi = require("swagger-ui-express");
const path = require("path");

const options = {
  definition: {
    openapi: "3.0.0",

    info: {
      title: "WoofBnB API",
      version: "1.0.0",
      description: "WoofBnB Backend API Documentation",
    },

    // servers: [
    //   {
    //     url: "http://localhost:5000/api",
    //     description: "Local Development Server",
    //   },
    // ],

    servers: [
      {
        url: "/api",
        description: "Current Server",
      },
    ],

    tags: [
      {
        name: "Authentication",
        description: "Authentication related APIs",
      },
      {
        name: "Pet Sitters",
        description: "Pet sitter management APIs",
      },
    ],

    components: {
      securitySchemes: {
        bearerAuth: {
          type: "http",
          scheme: "bearer",
          bearerFormat: "JWT",
        },
      },

      schemas: {
        LoginRequest: {
          type: "object",
          required: ["email", "password"],
          properties: {
            email: {
              type: "string",
              format: "email",
              example: "admin@example.com",
            },
            password: {
              type: "string",
              example: "password123",
            },
          },
        },

        LoginResponse: {
          type: "object",
          properties: {
            success: {
              type: "boolean",
              example: true,
            },
            message: {
              type: "string",
              example: "Login successful",
            },
            data: {
              type: "object",
              properties: {
                accessToken: {
                  type: "string",
                  example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                },
                user: {
                  type: "object",
                  properties: {
                    id: {
                      type: "string",
                      example: "6894fef0a7ab123456789abc",
                    },
                    email: {
                      type: "string",
                      example: "admin@example.com",
                    },
                  },
                },
              },
            },
          },
        },

        PetSitterRequest: {
          type: "object",

          required: [
            "name",
            "email",
            "phone",
            "bio",
            "address",
            "location",
            "workingHours",
            "amenities",
          ],

          properties: {
            name: {
              type: "string",
              minLength: 2,
              maxLength: 50,
              example: "John Doe",
            },

            email: {
              type: "string",
              format: "email",
              example: "john@example.com",
            },

            phone: {
              type: "string",
              minLength: 10,
              maxLength: 15,
              example: "9876543210",
            },

            bio: {
              type: "string",
              minLength: 20,
              maxLength: 1000,
              example:
                "Professional pet sitter with 5 years of experience caring for dogs and cats.",
            },

            address: {
              type: "string",
              minLength: 5,
              example: "Connaught Place, New Delhi",
            },

            location: {
              type: "object",

              properties: {
                type: {
                  type: "string",
                  enum: ["Point"],
                  example: "Point",
                },

                coordinates: {
                  type: "array",

                  minItems: 2,
                  maxItems: 2,

                  items: {
                    type: "number",
                  },

                  example: [77.209, 28.6139],
                },
              },
            },

            workingHours: {
              type: "object",

              properties: {
                start: {
                  type: "string",
                  example: "09:00",
                },

                end: {
                  type: "string",
                  example: "18:00",
                },
              },
            },

            amenities: {
              type: "array",

              items: {
                type: "string",

                enum: [
                  "Dog Walking",
                  "Medication",
                  "24x7 Care",
                  "Training",
                  "Vet Nearby",
                  "Indoor Stay",
                  "Outdoor Play",
                  "CCTV",
                  "Pickup Drop",
                  "Large Yard",
                  "Small Pets",
                  "Cats",
                  "Dogs",
                  "Birds",
                ],
              },

              example: ["Dog Walking", "Medication", "Indoor Stay"],
            },

            profileImage: {
              type: "string",
              nullable: true,
              example: "https://example.com/profile.jpg",
            },
          },
        },

        PetSitterResponse: {
          allOf: [
            {
              $ref: "#/components/schemas/PetSitterRequest",
            },
            {
              type: "object",

              properties: {
                id: {
                  type: "string",
                  example: "6894fef0a7ab123456789abc",
                },

                createdAt: {
                  type: "string",
                  format: "date-time",
                },

                updatedAt: {
                  type: "string",
                  format: "date-time",
                },
              },
            },
          ],
        },

        ApiResponse: {
          type: "object",

          properties: {
            success: {
              type: "boolean",
              example: true,
            },

            statusCode: {
              type: "integer",
              example: 201,
            },

            message: {
              type: "string",
              example: "Operation completed successfully",
            },

            data: {
              type: "object",
              description:
                "Response payload. The structure depends on the endpoint.",
            },

            timestamp: {
              type: "string",
              format: "date-time",
              example: "2026-07-31T12:30:15.123Z",
            },
          },
        },

        ApiError: {
          type: "object",
          properties: {
            success: {
              type: "boolean",
              example: false,
            },

            message: {
              type: "string",
              example: "Validation failed",
            },

            errors: {
              type: "array",
              items: {
                type: "string",
              },
            },
          },
        },
      },
    },
  },

  //   apis: ["./src/modules/**/*.routes.js"],
  //   apis: [path.resolve(__dirname, "../src/modules/**/*.routes.js")],
  apis: ["src/modules/**/*.routes.js"],
};

const swaggerSpec = swaggerJsdoc(options);

module.exports = {
  swaggerUi,
  swaggerSpec,
};
