
SELECT 
    k."Id",
    k."Uid",
    k."Status",
    k."Keterangan",
    k."CreatedAt",
    u."Username" as "PemilikUser"
FROM "Kartu" k
LEFT JOIN "Users" u ON k."Uid" = u."KartuUid"
ORDER BY k."CreatedAt" DESC;

SELECT 
    al."Id",
    k."Uid" as "KartuUID",
    r."Nama" as "Ruangan",
    al."TimestampMasuk" as "Masuk",
    al."TimestampKeluar" as "Keluar",
    al."Status",
    CASE 
        WHEN al."TimestampKeluar" IS NULL THEN 'Masih aktif'
        ELSE ROUND(EXTRACT(EPOCH FROM (al."TimestampKeluar" - al."TimestampMasuk")) / 60, 1) || ' menit'
    END as "Durasi"
FROM "AksesLog" al
JOIN "Kartu" k ON al."KartuId" = k."Id"
JOIN "Ruangan" r ON al."RuanganId" = r."Id"
ORDER BY al."TimestampMasuk" DESC
LIMIT 50;

SELECT 
    COUNT(*) as "TotalAksesHariIni",
    COUNT(CASE WHEN "TimestampKeluar" IS NULL THEN 1 END) as "MasihAktif",
    COUNT(CASE WHEN "TimestampKeluar" IS NOT NULL THEN 1 END) as "SudahCheckout",
    r."Nama" as "Ruangan",
    COUNT(*) as "JumlahAkses"
FROM "AksesLog" al
JOIN "Ruangan" r ON al."RuanganId" = r."Id"
WHERE DATE("TimestampMasuk") = CURRENT_DATE
GROUP BY r."Id", r."Nama"
ORDER BY "JumlahAkses" DESC;

SELECT 
    k."Uid",
    k."Status",
    COUNT(al."Id") as "TotalAkses",
    MAX(al."TimestampMasuk") as "AksesTerakhir"
FROM "Kartu" k
LEFT JOIN "AksesLog" al ON k."Id" = al."KartuId"
GROUP BY k."Id", k."Uid", k."Status"
ORDER BY "TotalAkses" DESC;

SELECT 
    u."Username",
    u."Role",
    k."Uid" as "KartuUID",
    k."Status" as "StatusKartu",
    u."CreatedAt" as "UserDibuat"
FROM "Users" u
LEFT JOIN "Kartu" k ON u."KartuUid" = k."Uid"
WHERE u."KartuUid" IS NOT NULL;

SELECT 
    r."Nama" as "Ruangan",
    COUNT(al."Id") as "TotalAkses",
    COUNT(CASE WHEN al."TimestampKeluar" IS NULL THEN 1 END) as "SedangAktif"
FROM "Ruangan" r
LEFT JOIN "AksesLog" al ON r."Id" = al."RuanganId"
GROUP BY r."Id", r."Nama"
ORDER BY "TotalAkses" DESC;

UPDATE "AksesLog" 
SET 
    "TimestampKeluar" = NOW(),
    "Status" = 'AUTO_CHECKOUT'
WHERE 
    "TimestampKeluar" IS NULL 
    AND "TimestampMasuk" < NOW() - INTERVAL '24 hours';

SELECT 
    'Kartu' as "Tabel",
    COUNT(*) as "JumlahRecord"
FROM "Kartu"
UNION ALL
SELECT 
    'Users' as "Tabel",
    COUNT(*) 
FROM "Users"
UNION ALL
SELECT 
    'AksesLog' as "Tabel",
    COUNT(*) 
FROM "AksesLog"
UNION ALL
SELECT 
    'Ruangan' as "Tabel",
    COUNT(*) 
FROM "Ruangan"
UNION ALL
SELECT 
    'Kelas' as "Tabel",
    COUNT(*) 
FROM "Kelas";