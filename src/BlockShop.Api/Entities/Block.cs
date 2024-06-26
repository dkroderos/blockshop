﻿namespace BlockShop.Api.Entities;

public class Block
{
    public required Guid Id { get; set; }
    public required Guid CreatorId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Image { get; set; }
    public required decimal Price { get; set; }
    public required int NumberOfBuys { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset LastUpdatedAt { get; set; }
    public ICollection<Comment> Comments { get; set; } = [];
}