using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BPV_tool.Server.Migrations
{
    /// <inheritdoc />
    public partial class addedUsersSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "RoleName" },
                values: new object[,]
                {
                    { new Guid("22bd8715-a157-451d-b568-b3ea3acce189"), "Student" },
                    { new Guid("b9d34df9-272a-43fc-86e1-41409fe287cd"), "Admin" },
                    { new Guid("e0898e39-2b02-498b-a973-20d601e84f8b"), "Teacher" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "MiddleName", "PasswordHash", "RoleId" },
                values: new object[] { new Guid("4fefaffc-acb3-4e2d-87ec-8cd634a0ff87"), "admin@bpv.local", "Admin", "User", null, "$2a$11$z4dra9wY8M88UW9YH1ObZOuQNdP4ifTbeX/1hwd1yDBGrzcAS3972", new Guid("b9d34df9-272a-43fc-86e1-41409fe287cd") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22bd8715-a157-451d-b568-b3ea3acce189"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e0898e39-2b02-498b-a973-20d601e84f8b"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4fefaffc-acb3-4e2d-87ec-8cd634a0ff87"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b9d34df9-272a-43fc-86e1-41409fe287cd"));
        }
    }
}
