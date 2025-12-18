CREATE DATABASE IF NOT EXISTS akses_lab;
USE akses_lab;
DROP TABLE IF EXISTS "AksesLog";
DROP TABLE IF EXISTS "Kartu";
DROP TABLE IF EXISTS "Kelas";
DROP TABLE IF EXISTS "Ruangan";
DROP TABLE IF EXISTS "Users";


CREATE TABLE "Periode" (
    "Id" SERIAL PRIMARY KEY,
    "Nama" VARCHAR(20) NOT NULL,
    "IsAktif" BOOLEAN DEFAULT FALSE,
    "StartDate" DATE,
    "EndDate" DATE
);

CREATE TABLE "Ruangan" (
    "Id" SERIAL PRIMARY KEY,
    "Nama" VARCHAR(100) NOT NULL
);

CREATE TABLE "Kelas" (
    "Id" SERIAL PRIMARY KEY,
    "Nama" VARCHAR(100) NOT NULL,
    "PeriodeId" INTEGER,
    FOREIGN KEY ("PeriodeId") REFERENCES "Periode"("Id") ON DELETE SET NULL
);

CREATE TABLE "Users" (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(50) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(20) NOT NULL,
    "CreatedAt" TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE "Kartu" (
    "Id" SERIAL PRIMARY KEY,
    "Uid" VARCHAR(50) NOT NULL UNIQUE,
    "Status" VARCHAR(20) DEFAULT 'AKTIF',
    "Keterangan" TEXT,
    "UserId" INTEGER NULL,
    "KelasId" INTEGER NULL,  
    "CreatedAt" TIMESTAMPTZ DEFAULT NOW(),
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE SET NULL,
    FOREIGN KEY ("KelasId") REFERENCES "Kelas"("Id") ON DELETE SET NULL,
    CONSTRAINT "CK_Kartu_SingleOwner" CHECK (
        ("UserId" IS NULL AND "KelasId" IS NOT NULL) OR 
        ("UserId" IS NOT NULL AND "KelasId" IS NULL) OR 
        ("UserId" IS NULL AND "KelasId" IS NULL)
    )
);

CREATE TABLE "AksesLog" (
    "Id" SERIAL PRIMARY KEY,
    "KartuId" INTEGER NOT NULL,
    "RuanganId" INTEGER NOT NULL,
    "TimestampMasuk" TIMESTAMPTZ NOT NULL, 
    "TimestampKeluar" TIMESTAMPTZ NULL,     
    "Status" VARCHAR(20) NOT NULL,
    "Keterangan" TEXT,
    FOREIGN KEY ("KartuId") REFERENCES "Kartu"("Id"),
    FOREIGN KEY ("RuanganId") REFERENCES "Ruangan"("Id")
);

INSERT INTO "Periode" ("Id", "Nama", "IsAktif") VALUES
(1, '2024/2025 Ganjil', FALSE),
(2, '2024/2025 Genap', TRUE)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Ruangan" ("Id", "Nama") VALUES
(1, 'Pemrograman'),
(2, 'PBO'),
(3, 'Teori')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Kelas" ("Id", "Nama", "PeriodeId") VALUES
(1, '10PPLG1', 2),
(2, '10PPLG2', 2),
(3, '11RPL1', 2)
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Users" ("Id", "Username", "PasswordHash", "Role") VALUES
(1, 'admin', '$2y$10$mRStzQtIE0JQIuKnb/dLsO9f9Rldk0aGx0Mt8ka2jjjDBYJHznRym', 'admin'),
(2, 'guru', '$2y$10$GNEyoNdStrqThg2sym3A9uZNTkb/0JpFbJBOKh/qksMslst29BqSS', 'guru'),
(3, 'operator', '$2a$11$r5oYCJ.k/0bE.uU6aKJb0.FT1V8J5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5', 'operator'),
(4, 'rehan', '$2a$11$r5oYCJ.k/0bE.uU6aKJb0.FT1V8J5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5Q5', 'siswa')
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO "Kartu" ("Id", "Uid", "Status", "Keterangan", "UserId", "KelasId") VALUES
(1, 'FC:4D:35:03', 'AKTIF', 'Kartu untuk Admin', 1, NULL),
(2, '8E:FF:D4:05', 'AKTIF', 'Kartu untuk Guru', 2, NULL),  
(3, '43:FF:0D:28', 'AKTIF', 'Kartu Kelas 10PPLG1', NULL, 1),
(4, 'A1:B2:C3:D4', 'AKTIF', 'Kartu Kelas 10PPLG2', NULL, 2),
(5, 'E5:F6:G7:H8', 'AKTIF', 'Kartu Cadangan', NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

-- Buat index
CREATE INDEX IF NOT EXISTS "IX_Kartu_Uid" ON "Kartu" ("Uid");
CREATE INDEX IF NOT EXISTS "IX_Kartu_UserId" ON "Kartu" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_Kartu_KelasId" ON "Kartu" ("KelasId");
CREATE INDEX IF NOT EXISTS "IX_Users_Username" ON "Users" ("Username");
CREATE INDEX IF NOT EXISTS "IX_AksesLog_KartuId" ON "AksesLog" ("KartuId");
CREATE INDEX IF NOT EXISTS "IX_AksesLog_RuanganId" ON "AksesLog" ("RuanganId");
CREATE INDEX IF NOT EXISTS "IX_AksesLog_TimestampMasuk" ON "AksesLog" ("TimestampMasuk");

-- Set sequence
SELECT setval('"Ruangan_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Ruangan"), 1));
SELECT setval('"Kelas_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Kelas"), 1));
SELECT setval('"Kartu_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Kartu"), 1));
SELECT setval('"Users_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Users"), 1));
SELECT setval('"AksesLog_Id_seq"', COALESCE((SELECT MAX("Id") FROM "AksesLog"), 1));