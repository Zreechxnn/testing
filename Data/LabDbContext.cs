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
        // 1. TAMBAHKAN DBSET PERIODE
        public DbSet<Periode> Periode => Set<Periode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfigurasi Periode
            modelBuilder.Entity<Periode>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nama).IsRequired();
                entity.Property(p => p.IsAktif).HasDefaultValue(false);
            });

            // Konfigurasi Kelas
            modelBuilder.Entity<Kelas>(entity =>
            {
                entity.HasKey(k => k.Id);

                // 2. TAMBAHKAN RELASI KELAS -> PERIODE
                entity.HasOne(k => k.Periode)
                    .WithMany(p => p.Kelas)
                    .HasForeignKey(k => k.PeriodeId)
                    .OnDelete(DeleteBehavior.Restrict); // Mencegah hapus periode jika masih ada kelas
            });

            // Konfigurasi Kartu
            modelBuilder.Entity<Kartu>(entity =>
            {
                entity.Property(k => k.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(k => k.Status)
                    .HasDefaultValue("AKTIF");

                entity.HasIndex(k => k.Uid)
                    .IsUnique();

                entity.HasOne(k => k.User)
                    .WithMany(u => u.Kartu)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(k => k.Kelas)
                    .WithMany(k => k.Kartu)
                    .HasForeignKey(k => k.KelasId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Konfigurasi User
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasIndex(u => u.Username)
                    .IsUnique();
            });

            // Konfigurasi AksesLog
            modelBuilder.Entity<AksesLog>(entity =>
            {
                entity.Property(a => a.TimestampMasuk)
                    .HasDefaultValueSql("NOW()");

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