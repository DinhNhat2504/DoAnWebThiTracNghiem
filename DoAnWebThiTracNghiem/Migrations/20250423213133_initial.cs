using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnWebThiTracNghiem.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    User_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ResetPasswordToken= table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetPasswordTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.User_Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Subject_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatorUser_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Subject_Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Users_CreatorUser_Id",
                        column: x => x.CreatorUser_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassTn",
                columns: table => new
                {
                    Class_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InviteCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    
                    CreatorUser_Id = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTn", x => x.Class_Id);
                    table.ForeignKey(
                        name: "FK_ClassTn_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Subject_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassTn_Users_CreatorUser_Id",
                        column: x => x.CreatorUser_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    Exam_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Exam_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    PassScore = table.Column<double>(type: "float", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exam_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    
                    CreatorUser_Id = table.Column<int>(type: "int", nullable: false),
                    Subject_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.Exam_ID);
                    table.ForeignKey(
                        name: "FK_Exams_Subjects_Subject_ID",
                        column: x => x.Subject_ID,
                        principalTable: "Subjects",
                        principalColumn: "Subject_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exams_Users_CreatorUser_Id",
                        column: x => x.CreatorUser_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    Question_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question_Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Option_A = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Option_B = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Option_C = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Option_D = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Correct_Option = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject_ID = table.Column<int>(type: "int", nullable: false),
                    Level_ID = table.Column<int>(type: "int", nullable: false),
                    
                    CreatorUser_Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.Question_ID);
                    table.ForeignKey(
                        name: "FK_Question_Levels_Level_ID",
                        column: x => x.Level_ID,
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Question_Subjects_Subject_ID",
                        column: x => x.Subject_ID,
                        principalTable: "Subjects",
                        principalColumn: "Subject_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Question_Users_CreatorUser_Id",
                        column: x => x.CreatorUser_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassStudents",
                columns: table => new
                {
                    SC_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    Class_ID = table.Column<int>(type: "int", nullable: false),
                    
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassStudents", x => x.SC_ID);
                    table.ForeignKey(
                        name: "FK_ClassStudents_ClassTn_Class_ID",
                        column: x => x.Class_ID,
                        principalTable: "ClassTn",
                        principalColumn: "Class_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassStudents_Users_User_Id",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Announcement_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    
                    
                    CreatorUser_Id = table.Column<int>(type: "int", nullable: false),
                    ClassTNClass_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Announcement_ID);
                    table.ForeignKey(
                        name: "FK_Notifications_ClassTn_ClassTNClass_Id",
                        column: x => x.ClassTNClass_Id,
                        principalTable: "ClassTn",
                        principalColumn: "Class_Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Users_CreatorUser_Id",
                        column: x => x.CreatorUser_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassExams",
                columns: table => new
                {
                    EC_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Exam_ID = table.Column<int>(type: "int", nullable: false),
                    
                    ClassTNClass_Id = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassExams", x => x.EC_ID);
                    table.ForeignKey(
                        name: "FK_ClassExams_ClassTn_ClassTNClass_Id",
                        column: x => x.ClassTNClass_Id,
                        principalTable: "ClassTn",
                        principalColumn: "Class_Id");
                    table.ForeignKey(
                        name: "FK_ClassExams_Exams_Exam_ID",
                        column: x => x.Exam_ID,
                        principalTable: "Exams",
                        principalColumn: "Exam_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamResult",
                columns: table => new
                {
                    Result_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Exam_ID = table.Column<int>(type: "int", nullable: false),
                    User_ID = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswers = table.Column<int>(type: "int", nullable: false),
                    WrongAnswers = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Start_Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End_Time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamResult", x => x.Result_ID);
                    table.ForeignKey(
                        name: "FK_ExamResult_Exams_Exam_ID",
                        column: x => x.Exam_ID,
                        principalTable: "Exams",
                        principalColumn: "Exam_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamResult_Users_User_ID",
                        column: x => x.User_ID,
                        principalTable: "Users",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestions",
                columns: table => new
                {
                    EQ_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Exam_ID = table.Column<int>(type: "int", nullable: false),
                    Question_ID = table.Column<int>(type: "int", nullable: false),
                    Question_Order = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestions", x => x.EQ_ID);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Exams_Exam_ID",
                        column: x => x.Exam_ID,
                        principalTable: "Exams",
                        principalColumn: "Exam_ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ExamQuestions_Question_Question_ID",
                        column: x => x.Question_ID,
                        principalTable: "Question",
                        principalColumn: "Question_ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    SA_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    
                    Question_ID = table.Column<int>(type: "int", nullable: false),
                    Result_ID1 = table.Column<int>(type: "int", nullable: false),
                    Selected_Option = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Is_Correct = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.SA_ID);
                    table.ForeignKey(
                        name: "FK_Answers_ExamResult_Result_ID1",
                        column: x => x.Result_ID1,
                        principalTable: "ExamResult",
                        principalColumn: "Result_ID");
                    table.ForeignKey(
                        name: "FK_Answers_Question_Question_ID",
                        column: x => x.Question_ID,
                        principalTable: "Question",
                        principalColumn: "Question_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_Question_ID",
                table: "Answers",
                column: "Question_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_Result_ID1",
                table: "Answers",
                column: "Result_ID1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassExams_ClassTNClass_Id",
                table: "ClassExams",
                column: "ClassTNClass_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClassExams_Exam_ID",
                table: "ClassExams",
                column: "Exam_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudents_Class_ID",
                table: "ClassStudents",
                column: "Class_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassStudents_User_Id",
                table: "ClassStudents",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTn_CreatorUser_Id",
                table: "ClassTn",
                column: "CreatorUser_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTn_SubjectId",
                table: "ClassTn",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_Exam_ID",
                table: "ExamQuestions",
                column: "Exam_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestions_Question_ID",
                table: "ExamQuestions",
                column: "Question_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResult_Exam_ID",
                table: "ExamResult",
                column: "Exam_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamResult_User_ID",
                table: "ExamResult",
                column: "User_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CreatorUser_Id",
                table: "Exams",
                column: "CreatorUser_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_Subject_ID",
                table: "Exams",
                column: "Subject_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ClassTNClass_Id",
                table: "Notifications",
                column: "ClassTNClass_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatorUser_Id",
                table: "Notifications",
                column: "CreatorUser_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Question_CreatorUser_Id",
                table: "Question",
                column: "CreatorUser_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Question_Level_ID",
                table: "Question",
                column: "Level_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Question_Subject_ID",
                table: "Question",
                column: "Subject_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_CreatorUser_Id",
                table: "Subjects",
                column: "CreatorUser_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "ClassExams");

            migrationBuilder.DropTable(
                name: "ClassStudents");

            migrationBuilder.DropTable(
                name: "ExamQuestions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ExamResult");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "ClassTn");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "Levels");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
