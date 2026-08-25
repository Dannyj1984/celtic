using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Celtic.Api.Models;

namespace Celtic.Api.Data;

public class CelticDbContext : IdentityDbContext<ApplicationUser>
{
    public CelticDbContext(DbContextOptions<CelticDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerParent> PlayerParents => Set<PlayerParent>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventResponse> EventResponses => Set<EventResponse>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchAppearance> MatchAppearances => Set<MatchAppearance>();
    public DbSet<SubPayment> SubPayments => Set<SubPayment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<ClubSettings> ClubSettings => Set<ClubSettings>();
    public DbSet<UserPushSubscription> UserPushSubscriptions => Set<UserPushSubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // PlayerParent — composite key
        builder.Entity<PlayerParent>()
            .HasKey(pp => new { pp.PlayerId, pp.UserId });

        builder.Entity<PlayerParent>()
            .HasOne(pp => pp.Player)
            .WithMany(p => p.ParentLinks)
            .HasForeignKey(pp => pp.PlayerId);

        builder.Entity<PlayerParent>()
            .HasOne(pp => pp.User)
            .WithMany(u => u.PlayerLinks)
            .HasForeignKey(pp => pp.UserId);

        // Event → Season
        builder.Entity<Event>()
            .HasOne(e => e.Season)
            .WithMany(s => s.Events)
            .HasForeignKey(e => e.SeasonId);

        // Event ↔ Match (one-to-one optional)
        builder.Entity<Event>()
            .HasOne(e => e.Match)
            .WithOne(m => m.Event)
            .HasForeignKey<Event>(e => e.MatchId);

        // Event → Captains
        builder.Entity<Event>()
            .HasOne(e => e.Captain1Player)
            .WithMany()
            .HasForeignKey(e => e.Captain1PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Event>()
            .HasOne(e => e.Captain2Player)
            .WithMany()
            .HasForeignKey(e => e.Captain2PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Match → Season
        builder.Entity<Match>()
            .HasOne(m => m.Season)
            .WithMany(s => s.Matches)
            .HasForeignKey(m => m.SeasonId);

        // Player of the Match
        builder.Entity<Match>()
            .HasOne(m => m.PlayerOfTheMatch)
            .WithMany()
            .HasForeignKey(m => m.PlayerOfTheMatchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Match.Result is computed, ignore it for EF
        builder.Entity<Match>()
            .Ignore(m => m.Result);

        // Player.FullName is computed, ignore it for EF
        builder.Entity<Player>()
            .Ignore(p => p.FullName);

        // EventResponse
        builder.Entity<EventResponse>()
            .HasOne(er => er.Event)
            .WithMany(e => e.Responses)
            .HasForeignKey(er => er.EventId);

        builder.Entity<EventResponse>()
            .HasOne(er => er.Player)
            .WithMany(p => p.EventResponses)
            .HasForeignKey(er => er.PlayerId);

        // MatchAppearance
        builder.Entity<MatchAppearance>()
            .HasOne(ma => ma.Match)
            .WithMany(m => m.Appearances)
            .HasForeignKey(ma => ma.MatchId);

        builder.Entity<MatchAppearance>()
            .HasOne(ma => ma.Player)
            .WithMany(p => p.MatchAppearances)
            .HasForeignKey(ma => ma.PlayerId);

        // SubPayment
        builder.Entity<SubPayment>()
            .HasOne(sp => sp.Player)
            .WithMany(p => p.Payments)
            .HasForeignKey(sp => sp.PlayerId);

        builder.Entity<SubPayment>()
            .HasOne(sp => sp.Season)
            .WithMany(s => s.Payments)
            .HasForeignKey(sp => sp.SeasonId);

        builder.Entity<SubPayment>()
            .Property(sp => sp.Amount)
            .HasColumnType("decimal(10,2)");

        // Expense
        builder.Entity<Expense>()
            .HasOne(e => e.Season)
            .WithMany(s => s.Expenses)
            .HasForeignKey(e => e.SeasonId);

        builder.Entity<Expense>()
            .Property(e => e.Amount)
            .HasColumnType("decimal(10,2)");

        // Season
        builder.Entity<Season>()
            .Property(s => s.SubAmount)
            .HasColumnType("decimal(10,2)");

        // Announcement
        builder.Entity<Announcement>()
            .HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId);
    }
}
