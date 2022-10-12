using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Spine.Data.Migrations
{
    public partial class updatePlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "NotificationPathId",
            //    table: "SubscriberNotification");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Plan");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NotificationPathId",
                table: "SubscriberNotification",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Plan",
                type: "bit",
                nullable: true);
        }
    }
}
