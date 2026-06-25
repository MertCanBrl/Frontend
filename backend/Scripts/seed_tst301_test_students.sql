-- ============================================================================
-- TST301 "Test Onay Dersi A" (CourseId = 3) için test öğrencileri seed scripti
-- ----------------------------------------------------------------------------
-- Amaç   : Devamsızlık / not girişi ekranlarını test edebilmek için derse 10
--          test öğrencisi ekler.
-- Özellik: İDEMPOTENT — iki kez çalıştırılırsa öğrenciler/kayıtlar tekrar EKLENMEZ
--          (öğrenci StudentNumber'a, kayıt (StudentId, CourseId) çiftine göre
--          kontrol edilir).
-- Kapsam : Yalnızca yerel/test veritabanı. Üretim verisine dokunmaz.
-- Çalıştır: docker exec -i app-postgres psql -U appuser -d appdb < bu_dosya.sql
-- ============================================================================

SET client_encoding TO 'UTF8';

BEGIN;

-- 1) Test öğrencileri (öğrenci no 2025001–2025010; mevcut 2021/2022 no'larıyla çakışmaz)
INSERT INTO "Students" ("StudentNumber", "FirstName", "LastName", "Email", "EnrollmentDate", "IsActive")
SELECT v."StudentNumber", v."FirstName", v."LastName", v."Email",
       TIMESTAMPTZ '2025-09-15 00:00:00+00', TRUE
FROM (VALUES
    ('2025001', 'Zeynep',     'Arslan',  'zeynep.arslan@ogr.edu.tr'),
    ('2025002', 'Mehmet Can', 'Yıldız',  'mehmetcan.yildiz@ogr.edu.tr'),
    ('2025003', 'Elif',       'Şahin',   'elif.sahin@ogr.edu.tr'),
    ('2025004', 'Burak',      'Doğan',   'burak.dogan@ogr.edu.tr'),
    ('2025005', 'Ayşe Nur',   'Çelik',   'aysenur.celik@ogr.edu.tr'),
    ('2025006', 'Emre',       'Koç',     'emre.koc@ogr.edu.tr'),
    ('2025007', 'Merve',      'Aydın',   'merve.aydin@ogr.edu.tr'),
    ('2025008', 'Ahmet',      'Kara',    'ahmet.kara@ogr.edu.tr'),
    ('2025009', 'Selin',      'Yılmaz',  'selin.yilmaz@ogr.edu.tr'),
    ('2025010', 'Berke',      'Öztürk',  'berke.ozturk@ogr.edu.tr')
) AS v("StudentNumber", "FirstName", "LastName", "Email")
WHERE NOT EXISTS (
    SELECT 1 FROM "Students" s WHERE s."StudentNumber" = v."StudentNumber"
);

-- 2) Bu öğrencileri TST301 (CourseId = 3) dersine kaydet (notlar boş bırakılır)
INSERT INTO "Enrollments" ("StudentId", "CourseId", "Midterm", "Final", "MakeUp")
SELECT s."Id", 3, NULL, NULL, NULL
FROM "Students" s
WHERE s."StudentNumber" IN
    ('2025001','2025002','2025003','2025004','2025005',
     '2025006','2025007','2025008','2025009','2025010')
  AND NOT EXISTS (
    SELECT 1 FROM "Enrollments" e WHERE e."StudentId" = s."Id" AND e."CourseId" = 3
);

COMMIT;

-- Doğrulama özeti
SELECT (SELECT COUNT(*) FROM "Students" WHERE "StudentNumber" LIKE '2025%')            AS test_ogrenci_sayisi,
       (SELECT COUNT(*) FROM "Enrollments" WHERE "CourseId" = 3)                       AS tst301_kayit_sayisi;
