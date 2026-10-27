using System.ComponentModel.DataAnnotations.Schema;

namespace AdventApi.Models
{
    public class Answer
    {
        public int QuestionId { get; set; }
        public int PlayerId { get; set; }
        public DateTime Submitted { get; set; }
        public string? Text { get; set; }
        
        [ForeignKey(nameof(QuestionId))]
        public required Question Question { get; set; }

        [ForeignKey(nameof(PlayerId))]
        public required Player Player { get; set; }
    }
}
