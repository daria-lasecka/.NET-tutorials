namespace GameStore.Api.Models;

public class Publisher
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public List<Game> Games { get; set; } = [];

}
