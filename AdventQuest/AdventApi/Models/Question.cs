namespace AdventApi.Models
{
    public enum QuestionType
    {
        FreeText,
        MultipleChoice
    }

    public class Question
    {
        public int Id { get; set; }
        public QuestionType Type { get; set; }
        public required string Text { get; set; }
        public required string Correct { get; set; }
    }
}
