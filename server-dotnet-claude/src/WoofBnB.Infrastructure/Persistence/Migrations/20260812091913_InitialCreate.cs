using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace WoofBnB.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PetSitters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Location = table.Column<Point>(type: "geography", nullable: false),
                    ProfileImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkingHoursEnd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    WorkingHoursStart = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetSitters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(72)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "admin"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_Role", "[Role] IN ('admin', 'super_admin')");
                });

            migrationBuilder.CreateTable(
                name: "PetSitterAmenities",
                columns: table => new
                {
                    PetSitterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Amenity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetSitterAmenities", x => new { x.PetSitterId, x.SortOrder });
                    table.CheckConstraint("CK_PetSitterAmenities_Amenity", "[Amenity] IN ('Dog Walking', 'Medication', '24x7 Care', 'Training', 'Vet Nearby', 'Indoor Stay', 'Outdoor Play', 'CCTV', 'Pickup Drop', 'Large Yard', 'Small Pets', 'Cats', 'Dogs', 'Birds')");
                    table.ForeignKey(
                        name: "FK_PetSitterAmenities_PetSitters_PetSitterId",
                        column: x => x.PetSitterId,
                        principalTable: "PetSitters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PetSitters_Email",
                table: "PetSitters",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            // EF Core's SQL Server provider has no fluent API for CREATE SPATIAL INDEX
            // (decision D-4), so this is hand-written. GEOGRAPHY_AUTO_GRID needs no
            // bounding box (unlike the geometry type), which is why it stays correct
            // globally without extra tuning.
            migrationBuilder.Sql(
                """
                CREATE SPATIAL INDEX SX_PetSitters_Location
                ON PetSitters(Location)
                USING GEOGRAPHY_AUTO_GRID;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PetSitterAmenities");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PetSitters");
        }
    }
}
