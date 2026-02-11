using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreComplianceView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "brand_distrubution_json",
                table: "audits");

            migrationBuilder.AlterColumn<string>(
                name: "region",
                table: "stores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.Sql(
                    @"
                    CREATE VIEW vw_store_compliance AS 
                    SELECT 
                        s.id AS store_id, 
                        s.name AS store_name, 
                        s.chain_name AS chain_name, 
                        s.latitude AS latitude, 
                        s.longitude AS longitude, 
                        s.address AS address, 
                        s.region AS region, 
                        s.created_at AS created_at, 
                        AVG(a.complience_score) AS compliance,
                        AVG(a.shelf_share_percentage) AS shelf_share_percentage 
                    FROM stores s LEFT JOIN audits a ON a.store_id = s.id 
                    GROUP BY s.id, s.name, s.region;
                    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "region",
                table: "stores",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "brand_distrubution_json",
                table: "audits",
                type: "jsonb",
                nullable: true);

            migrationBuilder.Sql(@"DROP VIEW IF EXISTS vw_store_compliance;");
        }
    }
}
