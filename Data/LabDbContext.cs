using Microsoft.EntityFrameworkCore;
using testing.Models;

namespace testing.Data
{
    public class LabDbContext : DbContext
    {
        public LabDbContext(DbContextOptions<LabDbContext> options) : base(options) { }

        public DbSet<Kartu> Kartu => Set<Kartu>();
        public DbSet<Kelas> Kelas => Set<Kelas>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Ruangan> Ruangan => Set<Ruangan>();
        public DbSet<AksesLog> AksesLog => Set<AksesLog>();
        public DbSet<Periode> Periode => Set<Periode>();

        public DbSet<AnggotaKelas> AnggotaKelas => Set<AnggotaKelas>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Periode>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nama).IsRequired();
                entity.Property(p => p.IsAktif).HasDefaultValue(false);
            });

            modelBuilder.Entity<Kelas>(entity =>
            {
                entity.HasKey(k => k.Id);

                entity.HasOne(k => k.Periode)
                    .WithMany(p => p.Kelas)
                    .HasForeignKey(k => k.PeriodeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AnggotaKelas>(entity =>
            {
                entity.HasKey(ak => ak.Id);

                entity.HasIndex(ak => new { ak.UserId, ak.KelasId }).IsUnique();

                entity.HasOne(ak => ak.User)
                    .WithMany(u => u.AnggotaKelas)
                    .HasForeignKey(ak => ak.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ak => ak.Kelas)
                    .WithMany(k => k.AnggotaKelas)
                    .HasForeignKey(ak => ak.KelasId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Kartu>(entity =>
            {
                entity.Property(k => k.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(k => k.Status).HasDefaultValue("AKTIF");
                entity.HasIndex(k => k.Uid).IsUnique();

                entity.HasOne(k => k.User)
                    .WithMany(u => u.Kartu)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(k => k.Kelas)
                    .WithMany(k => k.Kartu)
                    .HasForeignKey(k => k.KelasId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
                entity.HasIndex(u => u.Username).IsUnique();
            });

            modelBuilder.Entity<AksesLog>(entity =>
            {
                entity.Property(a => a.TimestampMasuk).HasDefaultValueSql("NOW()");

                entity.HasOne(a => a.Kartu)
                    .WithMany(k => k.AksesLogs)
                    .HasForeignKey(a => a.KartuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Ruangan)
                    .WithMany(r => r.AksesLogs)
                    .HasForeignKey(a => a.RuanganId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}