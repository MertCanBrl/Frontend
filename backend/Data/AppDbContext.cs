using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ProgramOutcomeGroup> ProgramOutcomeGroups => Set<ProgramOutcomeGroup>();
    public DbSet<ProgramOutcome> ProgramOutcomes => Set<ProgramOutcome>();
    public DbSet<CourseTopic> CourseTopics => Set<CourseTopic>();
    public DbSet<LearningOutcome> LearningOutcomes => Set<LearningOutcome>();
    public DbSet<LOPOMapping> LOPOMappings => Set<LOPOMapping>();
    public DbSet<CourseSurveyQuestion> CourseSurveyQuestions => Set<CourseSurveyQuestion>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
    public DbSet<ExamQuestionLearningOutcome> ExamQuestionLearningOutcomes => Set<ExamQuestionLearningOutcome>();
    public DbSet<ExamStudentGrade> ExamStudentGrades => Set<ExamStudentGrade>();
    public DbSet<ExamQuestionStudentScore> ExamQuestionStudentScores => Set<ExamQuestionStudentScore>();
    public DbSet<AssessmentComponent> AssessmentComponents => Set<AssessmentComponent>();
    public DbSet<AssessmentComponentLearningOutcome> AssessmentComponentLearningOutcomes => Set<AssessmentComponentLearningOutcome>();
    public DbSet<AssessmentComponentStudentGrade> AssessmentComponentStudentGrades => Set<AssessmentComponentStudentGrade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany(u => u.Courses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.ReviewedBy)
            .WithMany()
            .HasForeignKey(c => c.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<LOPOMapping>()
            .HasIndex(m => new { m.LearningOutcomeId, m.ProgramOutcomeId })
            .IsUnique();

        modelBuilder.Entity<LOPOMapping>()
            .HasOne(m => m.LearningOutcome)
            .WithMany(lo => lo.LOPOMappings)
            .HasForeignKey(m => m.LearningOutcomeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LOPOMapping>()
            .HasOne(m => m.ProgramOutcome)
            .WithMany(po => po.LOPOMappings)
            .HasForeignKey(m => m.ProgramOutcomeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamQuestion>()
            .HasOne(q => q.Exam)
            .WithMany(e => e.Questions)
            .HasForeignKey(q => q.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamQuestionLearningOutcome>()
            .HasKey(m => new { m.ExamQuestionId, m.LearningOutcomeId });

        modelBuilder.Entity<ExamQuestionLearningOutcome>()
            .HasOne(m => m.ExamQuestion)
            .WithMany(q => q.LearningOutcomeMappings)
            .HasForeignKey(m => m.ExamQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamQuestionLearningOutcome>()
            .HasOne(m => m.LearningOutcome)
            .WithMany(lo => lo.ExamQuestionMappings)
            .HasForeignKey(m => m.LearningOutcomeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamStudentGrade>()
            .HasOne(g => g.Exam)
            .WithMany()
            .HasForeignKey(g => g.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamStudentGrade>()
            .HasOne(g => g.Student)
            .WithMany()
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamStudentGrade>()
            .HasIndex(g => new { g.ExamId, g.StudentId })
            .IsUnique();

        modelBuilder.Entity<ExamQuestionStudentScore>()
            .HasOne(s => s.ExamStudentGrade)
            .WithMany(g => g.QuestionScores)
            .HasForeignKey(s => s.ExamStudentGradeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamQuestionStudentScore>()
            .HasOne(s => s.ExamQuestion)
            .WithMany()
            .HasForeignKey(s => s.ExamQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ExamQuestionStudentScore>()
            .HasIndex(s => new { s.ExamStudentGradeId, s.ExamQuestionId })
            .IsUnique();

        // ── AssessmentComponent ilişkileri ───────────────────
        modelBuilder.Entity<AssessmentComponentLearningOutcome>()
            .HasKey(m => new { m.AssessmentComponentId, m.LearningOutcomeId });

        modelBuilder.Entity<AssessmentComponentLearningOutcome>()
            .HasOne(m => m.AssessmentComponent)
            .WithMany(c => c.LearningOutcomeMappings)
            .HasForeignKey(m => m.AssessmentComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssessmentComponentLearningOutcome>()
            .HasOne(m => m.LearningOutcome)
            .WithMany()
            .HasForeignKey(m => m.LearningOutcomeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssessmentComponentStudentGrade>()
            .HasOne(g => g.AssessmentComponent)
            .WithMany(c => c.StudentGrades)
            .HasForeignKey(g => g.AssessmentComponentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssessmentComponentStudentGrade>()
            .HasOne(g => g.Student)
            .WithMany()
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssessmentComponentStudentGrade>()
            .HasIndex(g => new { g.AssessmentComponentId, g.StudentId })
            .IsUnique();

        // ── Seed Data ────────────────────────────────────────
        // TODO: Bu seed verileri EF migration ile production DB'ye de eklenir.
        // Production'da demo hesapların (admin@mudek.edu.tr, ali.vural@mudek.edu.tr)
        // şifrelerini ilk deploy sonrası mutlaka değiştirin veya devre dışı bırakın.

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, FullName = "Sistem Yöneticisi", Email = "admin@mudek.edu.tr", PasswordHash = "$2a$11$nKSgs23VvGfIptin60wqB.z/b7RyNWK8ixzrKy6gd4fAPc2ON1bee", Role = "Admin" },
            new User { Id = 2, FullName = "Dr. Ali Vural", Email = "ali.vural@mudek.edu.tr", PasswordHash = "$2a$11$McaBkQ1M1QZPn7fl3OzYS.PpxfDWLJgRx2x2ZBcfq6WzhtTsqHB1S", Role = "Instructor" }
        );

        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, StudentNumber = "2021001", FirstName = "Ahmet", LastName = "Yılmaz", Email = "ahmet.yilmaz@ogr.edu.tr", EnrollmentDate = new DateTime(2021, 9, 15, 0, 0, 0, DateTimeKind.Utc), IsActive = true },
            new Student { Id = 2, StudentNumber = "2021002", FirstName = "Ayşe", LastName = "Kaya", Email = "ayse.kaya@ogr.edu.tr", EnrollmentDate = new DateTime(2021, 9, 15, 0, 0, 0, DateTimeKind.Utc), IsActive = true },
            new Student { Id = 3, StudentNumber = "2022015", FirstName = "Mehmet", LastName = "Demir", Email = "mehmet.demir@ogr.edu.tr", EnrollmentDate = new DateTime(2022, 9, 12, 0, 0, 0, DateTimeKind.Utc), IsActive = true }
        );

        modelBuilder.Entity<Course>().HasData(
            new Course { Id = 1, Code = "BIL101", Name = "Programlamaya Giriş", Semester = "2024-Güz", Credit = 4, IsMandatory = true, InstructorId = 2, Akts = 6, WeeklyHours = 4, Department = "Bilgisayar Mühendisliği", ClassYear = 1, CourseType = "Teorik+Lab" },
            new Course { Id = 2, Code = "BIL202", Name = "Veri Yapıları", Semester = "2024-Güz", Credit = 3, IsMandatory = true, InstructorId = 2, Akts = 5, WeeklyHours = 3, Department = "Bilgisayar Mühendisliği", ClassYear = 2, CourseType = "Teorik" }
        );

        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment { Id = 1, StudentId = 1, CourseId = 1, Midterm = 70, Final = 80, MakeUp = null },
            new Enrollment { Id = 2, StudentId = 2, CourseId = 1, Midterm = 60, Final = 55, MakeUp = 65 },
            new Enrollment { Id = 3, StudentId = 3, CourseId = 1, Midterm = null, Final = null, MakeUp = null },
            new Enrollment { Id = 4, StudentId = 1, CourseId = 2, Midterm = 90, Final = 85, MakeUp = null }
        );

        // Program Çıktısı Grubu ve Çıktıları (MÜDEK Mühendislik)
        modelBuilder.Entity<ProgramOutcomeGroup>().HasData(
            new ProgramOutcomeGroup { Id = 1, Code = "GRP1", Name = "Teknik Yeterlilikler" },
            new ProgramOutcomeGroup { Id = 2, Code = "GRP2", Name = "Mesleki ve Etik Sorumluluklar" }
        );

        modelBuilder.Entity<ProgramOutcome>().HasData(
            new ProgramOutcome { Id = 1, Code = "PÇ1", Description = "Matematik, fen bilimleri ve bilgisayar mühendisliği konularında yeterli bilgi birikimi", GroupId = 1 },
            new ProgramOutcome { Id = 2, Code = "PÇ2", Description = "Bilgisayar mühendisliği problemlerini saptama, tanımlama, formüle etme ve çözme becerisi", GroupId = 1 },
            new ProgramOutcome { Id = 3, Code = "PÇ3", Description = "Karmaşık sistemleri, süreçleri veya ürünleri tasarlama becerisi", GroupId = 1 },
            new ProgramOutcome { Id = 4, Code = "PÇ4", Description = "Karmaşık mühendislik problemlerini araştırma becerisi", GroupId = 1 },
            new ProgramOutcome { Id = 5, Code = "PÇ5", Description = "Modern araç ve teknikleri kullanma becerisi", GroupId = 1 },
            new ProgramOutcome { Id = 6, Code = "PÇ6", Description = "Mühendislik uygulamalarının toplumsal ve küresel boyutlarını anlama", GroupId = 2 },
            new ProgramOutcome { Id = 7, Code = "PÇ7", Description = "Mesleki ve etik sorumluluk bilinci", GroupId = 2 },
            new ProgramOutcome { Id = 8, Code = "PÇ8", Description = "Etkin iletişim kurma becerisi", GroupId = 2 },
            new ProgramOutcome { Id = 9, Code = "PÇ9", Description = "Yaşam boyu öğrenmenin gerekliliği bilinci", GroupId = 2 },
            new ProgramOutcome { Id = 10, Code = "PÇ10", Description = "Proje yönetimi, risk yönetimi ve değişiklik yönetimi becerisi", GroupId = 2 }
        );
    }
}
