using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalCoach.Infrastructure.Persistence.Migrations;

[Migration("20260517000200_AddNotifications")]
public partial class AddNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Notification",
            columns: table => new
            {
                id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                user_id = table.Column<int>(type: "int", nullable: false),
                type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                priority = table.Column<int>(type: "int", nullable: false),
                is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                scheduled_for = table.Column<DateTime>(type: "datetime2", nullable: true),
                read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                expires_at = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notification", x => x.id);
                table.CheckConstraint("CK_Notification_priority", "[priority] BETWEEN 1 AND 5");
                table.CheckConstraint("CK_Notification_type", "[type] IN ('habit', 'task', 'wellness', 'recommendation', 'burnout', 'reminder', 'system')");
                table.ForeignKey(
                    name: "FK_Notification_UserProfile",
                    column: x => x.user_id,
                    principalTable: "UserProfile",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Notification_user_id",
            table: "Notification",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_Notification_user_is_read_created_at",
            table: "Notification",
            columns: new[] { "user_id", "is_read", "created_at" });

        migrationBuilder.CreateIndex(
            name: "IX_Notification_user_type_created_at",
            table: "Notification",
            columns: new[] { "user_id", "type", "created_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Notification");
    }
}
