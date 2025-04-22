using DoAnWebThiTracNghiem.Models;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAnWebThiTracNghiem.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine);
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Exam_Question> ExamQuestions { get; set; }
        public DbSet<Exam_Result> ExamResult { get; set; }
        public DbSet<Student_Answers> Answers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ClassTn> ClassTn { get; set; }
        public DbSet<Student_Class> ClassStudents { get; set; }
        public DbSet<Exam_Class> ClassExams { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student_Class>(entity =>
            {
                entity.HasKey(e => e.SC_ID);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Student_Class)
                    .HasForeignKey(e => e.User_ID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ClassTn)
                    .WithMany(c => c.Student_Classes)
                    .HasForeignKey(e => e.Class_ID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }


    }
}
       
        
    
    

