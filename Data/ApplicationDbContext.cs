using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamSchedule.Models;

namespace TeamSchedule.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<UserWeeklyAvailability> UserWeeklyAvailabilities => Set<UserWeeklyAvailability>();
    public DbSet<UserDateOverride> UserDateOverrides => Set<UserDateOverride>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamActivity> TeamActivities => Set<TeamActivity>();
    public DbSet<ActivityCandidateDate> ActivityCandidateDates => Set<ActivityCandidateDate>();
    public DbSet<ActivityResponse> ActivityResponses => Set<ActivityResponse>();
    public DbSet<ActivityParticipant> ActivityParticipants => Set<ActivityParticipant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Explicit Primary Keys
        builder.Entity<UserWeeklyAvailability>().HasKey(u => u.Id);
        builder.Entity<UserDateOverride>().HasKey(u => u.Id);
        builder.Entity<Team>().HasKey(t => t.TeamId);
        builder.Entity<TeamMember>().HasKey(tm => tm.TeamMemberId);
        builder.Entity<TeamActivity>().HasKey(ta => ta.ActivityId);
        builder.Entity<ActivityCandidateDate>().HasKey(c => c.CandidateDateId);
        builder.Entity<ActivityResponse>().HasKey(r => r.ResponseId);
        builder.Entity<ActivityParticipant>().HasKey(p => p.ActivityParticipantId);

        // UserWeeklyAvailability unique index: UserId + DayOfWeek
        builder.Entity<UserWeeklyAvailability>()
            .HasIndex(u => new { u.UserId, u.DayOfWeek })
            .IsUnique();

        // UserDateOverride unique index: UserId + TargetDate
        builder.Entity<UserDateOverride>()
            .HasIndex(u => new { u.UserId, u.TargetDate })
            .IsUnique();

        // Team InviteCode unique index
        builder.Entity<Team>()
            .HasIndex(t => t.InviteCode)
            .IsUnique();

        // TeamMember unique index: TeamId + UserId
        builder.Entity<TeamMember>()
            .HasIndex(tm => new { tm.TeamId, tm.UserId })
            .IsUnique();

        // ActivityCandidateDate unique index: ActivityId + CandidateDate + StartTime + EndTime
        // 同一日期可有多個不同時間段的候選；時間為 null 視為整天（Service 層負責去重）
        builder.Entity<ActivityCandidateDate>()
            .HasIndex(c => new { c.ActivityId, c.CandidateDate, c.StartTime, c.EndTime })
            .IsUnique();

        // ActivityResponse unique index: ActivityId + CandidateDateId + UserId
        builder.Entity<ActivityResponse>()
            .HasIndex(r => new { r.ActivityId, r.CandidateDateId, r.UserId })
            .IsUnique();

        // ActivityParticipant unique index: ActivityId + UserId
        builder.Entity<ActivityParticipant>()
            .HasIndex(p => new { p.ActivityId, p.UserId })
            .IsUnique();

        // Prevent multiple cascade paths in SQL Server
        builder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TeamMember>()
            .HasOne(tm => tm.User)
            .WithMany()
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TeamActivity>()
            .HasOne(ta => ta.Team)
            .WithMany(t => t.Activities)
            .HasForeignKey(ta => ta.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TeamActivity>()
            .HasOne(ta => ta.Creator)
            .WithMany()
            .HasForeignKey(ta => ta.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ActivityResponse>()
            .HasOne(ar => ar.Activity)
            .WithMany(ta => ta.Responses)
            .HasForeignKey(ar => ar.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ActivityResponse>()
            .HasOne(ar => ar.CandidateDate)
            .WithMany(cd => cd.Responses)
            .HasForeignKey(ar => ar.CandidateDateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ActivityResponse>()
            .HasOne(ar => ar.User)
            .WithMany()
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ActivityParticipant>()
            .HasOne(ap => ap.Activity)
            .WithMany(ta => ta.Participants)
            .HasForeignKey(ap => ap.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ActivityParticipant>()
            .HasOne(ap => ap.User)
            .WithMany()
            .HasForeignKey(ap => ap.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
