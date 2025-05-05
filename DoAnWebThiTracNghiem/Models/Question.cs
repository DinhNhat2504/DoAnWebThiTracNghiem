using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.Build.Framework;
using DoAnWebThiTracNghiem.Attributes;

namespace DoAnWebThiTracNghiem.Models
{
    public class Question
    {
        [Key]
        public int Question_ID { get; set; }
        public string? Question_Content { get; set; }
        [ForeignKey("QuestionType")]
        public int QuestionTypeId { get; set; }
        public QuestionType? QuestionType { get; set; }
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonIgnore]
        public List<string>? Options { get; set; }
        
        public string? Correct_Option { get; set; }
        [ForeignKey("Subject")]
        public int Subject_ID { get; set; }
        public Subject? Subject { get; set; }
        [ForeignKey("Level")]
        public int Level_ID { get; set; }
        public Level? Level { get; set; }
        [ForeignKey("Users")]
        public int CreatorUser_Id { get; set; }
        public Users? Creator { get; set; }
        public DateTime CreatedAt { get; set; }

        //Điều hướng n-n

        public ICollection<Student_Answers>? Student_Answers { get; set; }
        [JsonIgnore]
        public ICollection<Exam_Question>? Exam_Questions { get; set; }
    }
}
