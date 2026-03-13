using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditProductFieldsAndIssueSeverityHigh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "boundind_box_y",
                table: "audit_products",
                newName: "bounding_box_y");

            migrationBuilder.RenameColumn(
                name: "boundind_box_x",
                table: "audit_products",
                newName: "bounding_box_x");

            migrationBuilder.RenameColumn(
                name: "boundind_box_width",
                table: "audit_products",
                newName: "bounding_box_width");

            migrationBuilder.RenameColumn(
                name: "boundind_box_height",
                table: "audit_products",
                newName: "bounding_box_height");

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "audit_products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_eye_level",
                table: "audit_products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "shelf_position",
                table: "audit_products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "volume",
                table: "audit_products",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            // IssueSeverity HIGH için SQL gerekmez —
            // severity kolonu TEXT olarak saklanıyor (AppDbContext HasConversion<string>)
            // Mevcut "CRITICAL" kayıtlar olduğu gibi kalır, yeni HIGH kayıtlar "HIGH" yazar.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "audit_products");

            migrationBuilder.DropColumn(
                name: "is_eye_level",
                table: "audit_products");

            migrationBuilder.DropColumn(
                name: "shelf_position",
                table: "audit_products");

            migrationBuilder.DropColumn(
                name: "volume",
                table: "audit_products");

            migrationBuilder.RenameColumn(
                name: "bounding_box_y",
                table: "audit_products",
                newName: "boundind_box_y");

            migrationBuilder.RenameColumn(
                name: "bounding_box_x",
                table: "audit_products",
                newName: "boundind_box_x");

            migrationBuilder.RenameColumn(
                name: "bounding_box_width",
                table: "audit_products",
                newName: "boundind_box_width");

            migrationBuilder.RenameColumn(
                name: "bounding_box_height",
                table: "audit_products",
                newName: "boundind_box_height");
        }
    }
}