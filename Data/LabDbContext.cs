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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfigurasi Kartu - SESUAIKAN DENGAN MODEL
            modelBuilder.Entity<Kartu>(entity =>
            {
                // CreatedAt adalah DateTime? di model, jadi gunakan HasDefaultValueSql
                entity.Property(k => k.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(k => k.Status)
                    .HasDefaultValue("AKTIF");

                entity.HasIndex(k => k.Uid)
                    .IsUnique();

                // Relasi dengan User
                entity.HasOne(k => k.User)
                    .WithMany(u => u.Kartu)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relasi dengan Kelas
                entity.HasOne(k => k.Kelas)
                    .WithMany(k => k.Kartu)
                    .HasForeignKey(k => k.KelasId)
                    .OnDelete(DeleteBehavior.SetNull);

                // HAPUS CHECK CONSTRAINT UNTUK KOMPATIBILITAS
                // entity.ToTable(t => t.HasCheckConstraint("CK_Kartu_SingleOwner", ...));
            });

            // Konfigurasi User - SESUAIKAN DENGAN MODEL
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.HasIndex(u => u.Username)
                    .IsUnique();
            });

            // Konfigurasi AksesLog - SESUAIKAN DENGAN MODEL
            modelBuilder.Entity<AksesLog>(entity =>
            {
                // GUNAKAN PROPERTY YANG ADA DI MODEL
                entity.Property(a => a.TimestampMasuk)
                    .HasDefaultValueSql("NOW()");

                // TimestampKeluar tidak perlu default value karena nullable

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