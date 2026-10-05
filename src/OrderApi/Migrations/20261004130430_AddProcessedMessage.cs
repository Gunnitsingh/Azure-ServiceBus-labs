using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderApi.Migrations
{
    public partial class AddProcessedMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ProcessedMessages already exists in the database.
            // This migration brings the existing table under EF migration history.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do not drop the existing table because EF did not create it.
        }
    }
}