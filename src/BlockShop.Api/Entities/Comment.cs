namespace BlockShop.Api.Entities;

public class Comment
{
    public required Guid Id { get; set; }
    public required Guid CreatorId { get; set; }
    public required Guid BlockId { get; set; }
    public required string Content { get; set; }
    public required Block Block { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}