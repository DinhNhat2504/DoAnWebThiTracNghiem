using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnWebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentAnswersMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_ExamResult_Result_ID",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_Result_ID",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "Result_ID",
                table: "Answers");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_Result_ID1",
                table: "Answers",
                column: "Result_ID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_ExamResult_Result_ID1",
                table: "Answers",
                column: "Result_ID1",
                principalTable: "ExamResult",
                principalColumn: "Result_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_ExamResult_Result_ID1",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_Result_ID1",
                table: "Answers");

            migrationBuilder.AddColumn<int>(
                name: "Result_ID",
                table: "Answers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Answers_Result_ID",
                table: "Answers",
                column: "Result_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_ExamResult_Result_ID",
                table: "Answers",
                column: "Result_ID",
                principalTable: "ExamResult",
                principalColumn: "Result_ID");
        }
    }
}
