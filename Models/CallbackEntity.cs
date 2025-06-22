namespace kaban.Models;

public class CallbackEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Phone { get; set; }
    public required DateTime Date { get; set; }
    public required int PlaceId { get; set; }
    public PlaceEntity Place { get; set; } = null!;
}