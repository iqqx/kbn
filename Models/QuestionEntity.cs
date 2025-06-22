namespace kaban.Models;

public class QuestionEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required string Question { get; set; }
    public bool Answered { get; set; } = false;
    public required DateTime Date { get; set; }
}