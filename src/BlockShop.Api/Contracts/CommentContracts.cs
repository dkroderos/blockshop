// ReSharper disable All

namespace BlockShop.Api.Contracts;

public record CreateCommentRequest(Guid BlockId, string Content);

public record CommentResponse(
    Guid Id,
    Guid CreatorId,
    Guid BlockId,
    string Content,
    DateTimeOffset CreatedAt);