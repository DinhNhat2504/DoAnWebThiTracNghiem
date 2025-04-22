using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnWebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Question_Question_ID",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassExams_ClassTn_ClassTNClass_Id",
                table: "ClassExams");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTn_Users_CreatorUser_Id",
                table: "ClassTn");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Exams_Exam_ID",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Question_Question_ID",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Users_CreatorUser_Id",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ClassTn_ClassTNClass_Id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_CreatorUser_Id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Question_Users_CreatorUser_Id",
                table: "Question");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Users_CreatorUser_Id",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "Class_ID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ClassTn");

            migrationBuilder.DropColumn(
                name: "Class_ID",
                table: "ClassExams");

            migrationBuilder.RenameColumn(
                name: "Question_ID",
                table: "Answers",
                newName: "Question_ID1");

            migrationBuilder.RenameIndex(
                name: "IX_Answers_Question_ID",
                table: "Answers",
                newName: "IX_Answers_Question_ID1");

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "Subjects",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "Question",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClassTNClass_Id",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "ClassTn",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ClassTNClass_Id",
                table: "ClassExams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Question_Question_ID1",
                table: "Answers",
                column: "Question_ID1",
                principalTable: "Question",
                principalColumn: "Question_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassExams_ClassTn_ClassTNClass_Id",
                table: "ClassExams",
                column: "ClassTNClass_Id",
                principalTable: "ClassTn",
                principalColumn: "Class_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTn_Users_CreatorUser_Id",
                table: "ClassTn",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Exams_Exam_ID",
                table: "ExamQuestions",
                column: "Exam_ID",
                principalTable: "Exams",
                principalColumn: "Exam_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Question_Question_ID",
                table: "ExamQuestions",
                column: "Question_ID",
                principalTable: "Question",
                principalColumn: "Question_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Users_CreatorUser_Id",
                table: "Exams",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ClassTn_ClassTNClass_Id",
                table: "Notifications",
                column: "ClassTNClass_Id",
                principalTable: "ClassTn",
                principalColumn: "Class_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_CreatorUser_Id",
                table: "Notifications",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Users_CreatorUser_Id",
                table: "Question",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Users_CreatorUser_Id",
                table: "Subjects",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Question_Question_ID1",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassExams_ClassTn_ClassTNClass_Id",
                table: "ClassExams");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTn_Users_CreatorUser_Id",
                table: "ClassTn");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Exams_Exam_ID",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestions_Question_Question_ID",
                table: "ExamQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Users_CreatorUser_Id",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ClassTn_ClassTNClass_Id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_CreatorUser_Id",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Question_Users_CreatorUser_Id",
                table: "Question");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Users_CreatorUser_Id",
                table: "Subjects");

            migrationBuilder.RenameColumn(
                name: "Question_ID1",
                table: "Answers",
                newName: "Question_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Answers_Question_ID1",
                table: "Answers",
                newName: "IX_Answers_Question_ID");

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "Subjects",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Subjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "Question",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Question",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "Notifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ClassTNClass_Id",
                table: "Notifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Class_ID",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "Exams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUser_Id",
                table: "ClassTn",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "ClassTn",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ClassTNClass_Id",
                table: "ClassExams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Class_ID",
                table: "ClassExams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Question_Question_ID",
                table: "Answers",
                column: "Question_ID",
                principalTable: "Question",
                principalColumn: "Question_ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassExams_ClassTn_ClassTNClass_Id",
                table: "ClassExams",
                column: "ClassTNClass_Id",
                principalTable: "ClassTn",
                principalColumn: "Class_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTn_Users_CreatorUser_Id",
                table: "ClassTn",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Exams_Exam_ID",
                table: "ExamQuestions",
                column: "Exam_ID",
                principalTable: "Exams",
                principalColumn: "Exam_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestions_Question_Question_ID",
                table: "ExamQuestions",
                column: "Question_ID",
                principalTable: "Question",
                principalColumn: "Question_ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Users_CreatorUser_Id",
                table: "Exams",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ClassTn_ClassTNClass_Id",
                table: "Notifications",
                column: "ClassTNClass_Id",
                principalTable: "ClassTn",
                principalColumn: "Class_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_CreatorUser_Id",
                table: "Notifications",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Users_CreatorUser_Id",
                table: "Question",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Users_CreatorUser_Id",
                table: "Subjects",
                column: "CreatorUser_Id",
                principalTable: "Users",
                principalColumn: "User_Id");
        }
    }
}
