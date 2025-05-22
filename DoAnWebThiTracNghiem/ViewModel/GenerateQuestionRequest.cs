namespace DoAnWebThiTracNghiem.ViewModel
{
    public class GenerateQuestionRequest
    {
        public int NumberOfQuestions { get; set; }
        public string? Note { get; set; }
        public int SubjectId { get; set; }
        public int LevelId { get; set; }
        public int QuestionTypeId { get; set; }
    }
    public class GeneratedQuestionMultipleChoice
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
    }
    public class GeneratedQuestionTrueFalse
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
    }
    public class GeneratedQuestionFillInTheBlank
    {
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
