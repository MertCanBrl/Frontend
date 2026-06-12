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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aynı öğrenci aynı derse iki kez kaydolmasın
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        // Email benzersiz olsun (aynı email ile iki kullanıcı olmasın)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Öğretmen silinirse dersleri silinmesin, sadece atama boşalsın
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany(u => u.Courses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed: Kullanıcılar (1 yönetici + 1 öğretmen)
        // Seed: Kullanıcılar (1 yönetici + 1 öğretmen)
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, FullName = "Sistem Yöneticisi", Email = "admin@mudek.edu.tr", PasswordHash = "$2a$11$nKSgs23VvGfIptin60wqB.z/b7RyNWK8ixzrKy6gd4fAPc2ON1bee", Role = "Admin" },
            new User { Id = 2, FullName = "Dr. Ali Vural", Email = "ali.vural@mudek.edu.tr", PasswordHash = "$2a$11$McaBkQ1M1QZPn7fl3OzYS.PpxfDWLJgRx2x2ZBcfq6WzhtTsqHB1S", Role = "Instructor" }
        );

        // Seed: Öğrenciler
        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, StudentNumber = "2021001", FirstName = "Ahmet", LastName = "Yılmaz", Email = "ahmet.yilmaz@ogr.edu.tr", EnrollmentDate = new DateTime(2021, 9, 15, 0, 0, 0, DateTimeKind.Utc), IsActive = true },
            new Student { Id = 2, StudentNumber = "2021002", FirstName = "Ayşe", LastName = "Kaya", Email = "ayse.kaya@ogr.edu.tr", EnrollmentDate = new DateTime(2021, 9, 15, 0, 0, 0, DateTimeKind.Utc), IsActive = true },
            new Student { Id = 3, StudentNumber = "2022015", FirstName = "Mehmet", LastName = "Demir", Email = "mehmet.demir@ogr.edu.tr", EnrollmentDate = new DateTime(2022, 9, 12, 0, 0, 0, DateTimeKind.Utc), IsActive = true }
        );

        // Seed: Dersler (Dr. Ali Vural'a atanmış)
        modelBuilder.Entity<Course>().HasData(
            new Course { Id = 1, Code = "BIL101", Name = "Programlamaya Giriş", Semester = "2024-Güz", Credit = 4, IsMandatory = true, InstructorId = 2 },
            new Course { Id = 2, Code = "BIL202", Name = "Veri Yapıları", Semester = "2024-Güz", Credit = 3, IsMandatory = true, InstructorId = 2 }
        );

        // Seed: Kayıtlar (öğrenci-ders eşleşmesi + notlar)
        modelBuilder.Entity<Enrollment>().HasData(
            new Enrollment { Id = 1, StudentId = 1, CourseId = 1, Midterm = 70, Final = 80, MakeUp = null },
            new Enrollment { Id = 2, StudentId = 2, CourseId = 1, Midterm = 60, Final = 55, MakeUp = 65 },
            new Enrollment { Id = 3, StudentId = 3, CourseId = 1, Midterm = null, Final = null, MakeUp = null },
            new Enrollment { Id = 4, StudentId = 1, CourseId = 2, Midterm = 90, Final = 85, MakeUp = null }
        );
    }
}