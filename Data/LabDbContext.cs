using Microsoft.EntityFrameworkCore;
using testing.Models;

namespace testing.Data
{
    public class LabDbContext : DbContext
    {
        public LabDbContext(DbContextOptions<LabDbContext> options) : base(options) { }

        public DbSet<Jurusan> Jurusan => Set<Jurusan>(); // BARU
        public DbSet<Kartu> Kartu => Set<Kartu>();
        public DbSet<Kelas> Kelas => Set<Kelas>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Ruangan> Ruangan => Set<Ruangan>();
        public DbSet<AksesLog> AksesLog => Set<AksesLog>();
        public DbSet<Periode> Periode => Set<Periode>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Konfigurasi Jurusan (BARU)
            modelBuilder.Entity<Jurusan>(entity =>
            {
                entity.ToTable("Jurusan");
                entity.HasKey(j => j.Id);
                entity.HasIndex(j => j.Kode).IsUnique(); // Kode jurusan harus unik
            });

            // 2. Konfigurasi Periode
            modelBuilder.Entity<Periode>(entity =>
            {
                entity.ToTable("Periode");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nama).IsRequired();
                entity.Property(p => p.IsAktif).HasDefaultValue(false);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
            });

            // 3. Konfigurasi Kelas (UPDATE RELASI)
            modelBuilder.Entity<Kelas>(entity =>
            {
                entity.ToTable("Kelas");
                entity.HasKey(k => k.Id);

                // Relasi Kelas -> Periode (CASCADE sesuai SQL)
                entity.HasOne(k => k.Periode)
                    .WithMany(p => p.Kelas)
                    .HasForeignKey(k => k.PeriodeId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relasi Kelas -> Jurusan (SET NULL sesuai SQL)
                entity.HasOne(k => k.Jurusan)
                    .WithMany(j => j.Kelas)
                    .HasForeignKey(k => k.JurusanId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 4. Konfigurasi User (UPDATE RELASI)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("now()");

                // Relasi User -> Kelas (SET NULL)
                entity.HasOne(u => u.Kelas)
                    .WithMany(k => k.Users)
                    .HasForeignKey(u => u.KelasId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 5. Konfigurasi Kartu
            modelBuilder.Entity<Kartu>(entity =>
            {
                entity.ToTable("Kartu");
                entity.Property(k => k.CreatedAt).HasDefaultValueSql("now()");
                entity.Property(k => k.Status).HasDefaultValue("AKTIF");
                entity.HasIndex(k => k.Uid).IsUnique();

                // Constraint Single Owner (User/Kelas logic ada di Database Check Constraint)
                // Disini kita definisikan relasinya saja

                entity.HasOne(k => k.User)
                    .WithMany(u => u.Kartu)
                    .HasForeignKey(k => k.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(k => k.Kelas)
                    .WithMany(k => k.Kartu)
                    .HasForeignKey(k => k.KelasId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 6. Konfigurasi AksesLog
            modelBuilder.Entity<AksesLog>(entity =>
            {
                entity.ToTable("AksesLog");
                entity.Property(a => a.TimestampMasuk).HasDefaultValueSql("now()");

                entity.HasOne(a => a.Kartu)
                    .WithMany(k => k.AksesLogs)
                    .HasForeignKey(a => a.KartuId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Ruangan)
                    .WithMany(r => r.AksesLogs)
                    .HasForeignKey(a => a.RuanganId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 7. Konfigurasi Ruangan
            modelBuilder.Entity<Ruangan>(entity =>
            {
                entity.ToTable("Ruangan");
                entity.HasKey(r => r.Id);
            });
        }
    }
}