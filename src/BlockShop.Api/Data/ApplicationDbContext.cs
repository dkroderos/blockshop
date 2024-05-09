using BlockShop.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlockShop.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Comment>()
            .HasOne(c => c.Block)
            .WithMany(b => b.Comments)
            .HasForeignKey(c => c.BlockId);
    }

    public virtual DbSet<Block> Blocks { get; init; } = null!;
    public virtual DbSet<Comment> Comments { get; init; } = null!;
}