using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Nine.Identities.Domain.Users.Entities;

namespace Nine.Identities.Infrastructure.Identity;

public sealed class IdentitiesDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public IdentitiesDbContext(DbContextOptions<IdentitiesDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("identities");
        base.OnModelCreating(builder);
        builder.UseOpenIddict();

        builder.Entity<User>()
            .HasIndex(user => user.PhoneNumber)
            .IsUnique()
            .HasFilter("\"PhoneNumber\" IS NOT NULL");
    }
}
