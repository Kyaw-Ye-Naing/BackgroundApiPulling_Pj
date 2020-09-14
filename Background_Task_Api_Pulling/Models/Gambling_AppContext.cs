using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Background_Task_Api_Pulling.Models
{
    public partial class Gambling_AppContext : DbContext
    {
        public Gambling_AppContext()
        {
        }

        public Gambling_AppContext(DbContextOptions<Gambling_AppContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TblConfirmLeague> TblConfirmLeague { get; set; }
        public virtual DbSet<TblFootballTeam> TblFootballTeam { get; set; }
        public virtual DbSet<TblGoalResult> TblGoalResult { get; set; }
        public virtual DbSet<TblHandicap> TblHandicap { get; set; }
        public virtual DbSet<TblLeague> TblLeague { get; set; }
        public virtual DbSet<TblPreUpcomingEvent> TblPreUpcomingEvent { get; set; }
        public virtual DbSet<TblRole> TblRole { get; set; }
        public virtual DbSet<TblUpcomingEvent> TblUpcomingEvent { get; set; }
        public virtual DbSet<TblUser> TblUser { get; set; }
        public virtual DbSet<ViewUpcomingEventDetails> ViewUpcomingEventDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=172.105.121.13;Initial Catalog=Gambling_App;user id=sa;password=209851@ung");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblConfirmLeague>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tbl_confirmLeague");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.ConfirmLeagueId)
                    .HasColumnName("confirmLeagueId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.LeagueId)
                    .HasColumnName("leagueId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RapidLeagueId)
                    .HasColumnName("rapidLeagueId")
                    .HasColumnType("decimal(18, 0)");
            });

            modelBuilder.Entity<TblFootballTeam>(entity =>
            {
                entity.HasKey(e => e.FootballTeamId);

                entity.ToTable("tbl_footballTeam");

                entity.Property(e => e.FootballTeamId)
                    .HasColumnName("footballTeamId")
                    .HasColumnType("decimal(18, 0)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("createdBy")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedDate)
                    .HasColumnName("createdDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.FootballTeam)
                    .HasColumnName("footballTeam")
                    .HasMaxLength(50);

                entity.Property(e => e.FootballTeamMyan)
                    .HasColumnName("footballTeamMyan")
                    .HasMaxLength(50);

                entity.Property(e => e.LeagueId)
                    .HasColumnName("leagueId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RapidTeamId)
                    .HasColumnName("rapidTeamId")
                    .HasColumnType("decimal(18, 0)");
            });

            modelBuilder.Entity<TblGoalResult>(entity =>
            {
                entity.HasKey(e => e.GoalResultId);

                entity.ToTable("tbl_goalResult");

                entity.Property(e => e.GoalResultId)
                    .HasColumnName("goalResultId")
                    .HasColumnType("decimal(18, 0)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.AwayResult).HasColumnName("awayResult");

                entity.Property(e => e.HomeResult).HasColumnName("homeResult");

                entity.Property(e => e.RapidEventId)
                    .HasColumnName("rapidEventId")
                    .HasMaxLength(10)
                    .IsFixedLength();
            });

            modelBuilder.Entity<TblHandicap>(entity =>
            {
                entity.HasKey(e => e.HandicapId);

                entity.ToTable("tbl_handicap");

                entity.Property(e => e.HandicapId)
                    .HasColumnName("handicapId")
                    .HasColumnType("decimal(18, 0)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.AwayHandicap)
                    .HasColumnName("awayHandicap")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.AwayOdd)
                    .HasColumnName("awayOdd")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.EventDatetime)
                    .HasColumnName("eventDatetime")
                    .HasColumnType("datetime");

                entity.Property(e => e.HomeHandicap)
                    .HasColumnName("homeHandicap")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.HomeOdd)
                    .HasColumnName("homeOdd")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.RapidEventId)
                    .HasColumnName("rapidEventId")
                    .HasColumnType("decimal(18, 0)");
            });

            modelBuilder.Entity<TblLeague>(entity =>
            {
                entity.HasKey(e => e.LeagueId);

                entity.ToTable("tbl_league");

                entity.Property(e => e.LeagueId)
                    .HasColumnName("leagueId")
                    .HasColumnType("decimal(18, 0)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.LeagueName)
                    .HasColumnName("leagueName")
                    .HasMaxLength(50);

                entity.Property(e => e.RapidLeagueId)
                    .HasColumnName("rapidLeagueId")
                    .HasColumnType("decimal(18, 0)");
            });

            modelBuilder.Entity<TblPreUpcomingEvent>(entity =>
            {
                entity.HasKey(e => e.PreUpcommingEventId);

                entity.ToTable("tbl_preUpcomingEvent");

                entity.Property(e => e.PreUpcommingEventId)
                    .HasColumnName("preUpcommingEventId")
                    .HasColumnType("decimal(18, 0)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.AwayTeamId)
                    .HasColumnName("awayTeamId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.EventDate)
                    .HasColumnName("eventDate")
                    .HasColumnType("date");

                entity.Property(e => e.EventTime)
                    .HasColumnName("eventTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.HomeTeamId)
                    .HasColumnName("homeTeamId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.LeagueId)
                    .HasColumnName("leagueId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RapidEventId)
                    .HasColumnName("rapidEventId")
                    .HasColumnType("decimal(18, 0)");
            });

            modelBuilder.Entity<TblRole>(entity =>
            {
                entity.HasKey(e => e.RoleId);

                entity.ToTable("tbl_role");

                entity.Property(e => e.RoleId).HasColumnName("roleId");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("createdBy")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedDate)
                    .HasColumnName("createdDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Discription)
                    .HasColumnName("discription")
                    .HasMaxLength(50);

                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TblUpcomingEvent>(entity =>
            {
                entity.HasKey(e => e.UpcomingEventId)
                    .HasName("PK_tbl_upComingEvent");

                entity.ToTable("tbl_upcomingEvent");

                entity.Property(e => e.UpcomingEventId)
                    .HasColumnName("upcomingEventId")
                    .HasColumnType("decimal(18, 0)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.AwayTeamId)
                    .HasColumnName("awayTeamId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.EventDate)
                    .HasColumnName("eventDate")
                    .HasColumnType("date");

                entity.Property(e => e.EventTime)
                    .HasColumnName("eventTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.HomeTeamId)
                    .HasColumnName("homeTeamId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.LeagueId)
                    .HasColumnName("leagueId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RapidEventId)
                    .HasColumnName("rapidEventId")
                    .HasColumnType("decimal(18, 0)");
            });

            modelBuilder.Entity<TblUser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("tbl_user");

                entity.Property(e => e.UserId)
                    .HasColumnName("userId")
                    .HasColumnType("decimal(18, 0)")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("createdBy")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreatedDate)
                    .HasColumnName("createdDate")
                    .HasColumnType("datetime");

                entity.Property(e => e.Lock).HasColumnName("lock");

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(250);

                entity.Property(e => e.RoleId).HasColumnName("roleId");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ViewUpcomingEventDetails>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_UpcomingEventDetails");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.AwayFootballTeam)
                    .HasColumnName("awayFootballTeam")
                    .HasMaxLength(50);

                entity.Property(e => e.AwayFootballTeamMyan)
                    .HasColumnName("awayFootballTeamMyan")
                    .HasMaxLength(50);

                entity.Property(e => e.AwayHandicap)
                    .HasColumnName("awayHandicap")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.AwayOdd)
                    .HasColumnName("awayOdd")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.AwayRapidTeamId)
                    .HasColumnName("awayRapidTeamId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.AwayTeamId)
                    .HasColumnName("awayTeamId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.EventDate)
                    .HasColumnName("eventDate")
                    .HasColumnType("date");

                entity.Property(e => e.EventTime)
                    .HasColumnName("eventTime")
                    .HasColumnType("datetime");

                entity.Property(e => e.HomeFootballTeam)
                    .HasColumnName("homeFootballTeam")
                    .HasMaxLength(50);

                entity.Property(e => e.HomeFootballTeamMyan)
                    .HasColumnName("homeFootballTeamMyan")
                    .HasMaxLength(50);

                entity.Property(e => e.HomeHandicap)
                    .HasColumnName("homeHandicap")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.HomeOdd)
                    .HasColumnName("homeOdd")
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.HomeRapidTeamId)
                    .HasColumnName("homeRapidTeamId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.HomeTeamId)
                    .HasColumnName("homeTeamId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.LeagueId)
                    .HasColumnName("leagueId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.LeagueName)
                    .HasColumnName("leagueName")
                    .HasMaxLength(50);

                entity.Property(e => e.RapidLeagueId)
                    .HasColumnName("rapidLeagueId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.RapideventId)
                    .HasColumnName("rapideventId")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.UpcomingEventId)
                    .HasColumnName("upcomingEventId")
                    .HasColumnType("decimal(18, 0)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
