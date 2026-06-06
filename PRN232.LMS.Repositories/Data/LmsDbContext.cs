using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options)
    {
    }

    public DbSet<Semester> Semesters => Set<Semester>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureSemesters(modelBuilder);
        ConfigureCourses(modelBuilder);
        ConfigureSubjects(modelBuilder);
        ConfigureStudents(modelBuilder);
        ConfigureEnrollments(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureRefreshTokens(modelBuilder);

        SeedData(modelBuilder);
    }

    private static void ConfigureSemesters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Semester>(entity =>
        {
            entity.ToTable("Semester");
            entity.HasKey(semester => semester.SemesterId);

            entity.Property(semester => semester.SemesterName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(semester => semester.StartDate)
                .HasColumnType("datetime")
                .IsRequired();

            entity.Property(semester => semester.EndDate)
                .HasColumnType("datetime")
                .IsRequired();
        });
    }

    private static void ConfigureCourses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Course");
            entity.HasKey(course => course.CourseId);

            entity.Property(course => course.CourseName)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(course => course.Semester)
                .WithMany(semester => semester.Courses)
                .HasForeignKey(course => course.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSubjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");
            entity.HasKey(subject => subject.SubjectId);

            entity.Property(subject => subject.SubjectCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(subject => subject.SubjectName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(subject => subject.Credit)
                .IsRequired();

            entity.HasIndex(subject => subject.SubjectCode)
                .IsUnique();
        });
    }

    private static void ConfigureStudents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Student");
            entity.HasKey(student => student.StudentId);

            entity.Property(student => student.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(student => student.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(student => student.DateOfBirth)
                .HasColumnType("datetime")
                .IsRequired();

            entity.HasIndex(student => student.Email)
                .IsUnique();
        });
    }

    private static void ConfigureEnrollments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollment");
            entity.HasKey(enrollment => enrollment.EnrollmentId);

            entity.Property(enrollment => enrollment.EnrollDate)
                .HasColumnType("datetime")
                .IsRequired();

            entity.Property(enrollment => enrollment.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            entity.HasOne(enrollment => enrollment.Student)
                .WithMany(student => student.Enrollments)
                .HasForeignKey(enrollment => enrollment.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(enrollment => enrollment.Course)
                .WithMany(course => course.Enrollments)
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Semester>().HasData(CreateSemesters());
        modelBuilder.Entity<Subject>().HasData(CreateSubjects());
        modelBuilder.Entity<Course>().HasData(CreateCourses());
        modelBuilder.Entity<Student>().HasData(CreateStudents());
        modelBuilder.Entity<Enrollment>().HasData(CreateEnrollments());
        modelBuilder.Entity<User>().HasData(CreateUsers());
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(user => user.UserId);

            entity.Property(user => user.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            entity.HasIndex(user => user.Username)
                .IsUnique();
        });
    }

    private static void ConfigureRefreshTokens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshToken");
            entity.HasKey(refreshToken => refreshToken.RefreshTokenId);

            entity.Property(refreshToken => refreshToken.Token)
                .HasMaxLength(200)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(refreshToken => refreshToken.ReplacedByToken)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(refreshToken => refreshToken.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(refreshToken => refreshToken.Token)
                .IsUnique();
        });
    }

    private static IEnumerable<Semester> CreateSemesters()
    {
        return new[]
        {
            new Semester
            {
                SemesterId = 1,
                SemesterName = "Spring 2025",
                StartDate = new DateTime(2025, 1, 6),
                EndDate = new DateTime(2025, 4, 27)
            },
            new Semester
            {
                SemesterId = 2,
                SemesterName = "Summer 2025",
                StartDate = new DateTime(2025, 5, 5),
                EndDate = new DateTime(2025, 8, 24)
            },
            new Semester
            {
                SemesterId = 3,
                SemesterName = "Fall 2025",
                StartDate = new DateTime(2025, 9, 1),
                EndDate = new DateTime(2025, 12, 21)
            },
            new Semester
            {
                SemesterId = 4,
                SemesterName = "Spring 2026",
                StartDate = new DateTime(2026, 1, 5),
                EndDate = new DateTime(2026, 4, 26)
            },
            new Semester
            {
                SemesterId = 5,
                SemesterName = "Summer 2026",
                StartDate = new DateTime(2026, 5, 4),
                EndDate = new DateTime(2026, 8, 23)
            }
        };
    }

    private static IEnumerable<Subject> CreateSubjects()
    {
        return new[]
        {
            new Subject { SubjectId = 1, SubjectCode = "PRN232", SubjectName = "REST API Development", Credit = 3 },
            new Subject { SubjectId = 2, SubjectCode = "PRJ301", SubjectName = "Java Web Application", Credit = 3 },
            new Subject { SubjectId = 3, SubjectCode = "SWT301", SubjectName = "Software Testing", Credit = 3 },
            new Subject { SubjectId = 4, SubjectCode = "SWP391", SubjectName = "Software Project", Credit = 4 },
            new Subject { SubjectId = 5, SubjectCode = "DBI202", SubjectName = "Database Systems", Credit = 3 },
            new Subject { SubjectId = 6, SubjectCode = "CSD201", SubjectName = "Data Structures", Credit = 3 },
            new Subject { SubjectId = 7, SubjectCode = "PRO192", SubjectName = "Object Oriented Programming", Credit = 3 },
            new Subject { SubjectId = 8, SubjectCode = "MAS291", SubjectName = "Statistics and Probability", Credit = 3 },
            new Subject { SubjectId = 9, SubjectCode = "NWC203", SubjectName = "Computer Networking", Credit = 3 },
            new Subject { SubjectId = 10, SubjectCode = "PRM392", SubjectName = "Mobile Programming", Credit = 3 }
        };
    }

    private static IEnumerable<Course> CreateCourses()
    {
        return Enumerable.Range(1, 20)
            .Select(courseId => new Course
            {
                CourseId = courseId,
                CourseName = $"LMS Course {courseId:00}",
                SemesterId = ((courseId - 1) % 5) + 1
            });
    }

    private static IEnumerable<Student> CreateStudents()
    {
        return Enumerable.Range(1, 50)
            .Select(studentId => new Student
            {
                StudentId = studentId,
                FullName = $"Student {studentId:00}",
                Email = $"student{studentId:000}@lms.local",
                DateOfBirth = new DateTime(2000, 1, 1).AddDays(studentId * 30)
            });
    }

    private static IEnumerable<Enrollment> CreateEnrollments()
    {
        var statuses = new[] { "Active", "Completed", "Dropped" };

        return Enumerable.Range(1, 50)
            .SelectMany(studentId => Enumerable.Range(1, 10)
                .Select(sequence =>
                {
                    var enrollmentId = ((studentId - 1) * 10) + sequence;

                    return new Enrollment
                    {
                        EnrollmentId = enrollmentId,
                        StudentId = studentId,
                        CourseId = ((studentId + sequence - 2) % 20) + 1,
                        EnrollDate = new DateTime(2026, 1, 1).AddDays(enrollmentId % 120),
                        Status = statuses[(enrollmentId - 1) % statuses.Length]
                    };
                }));
    }

    private static IEnumerable<User> CreateUsers()
    {
        var user = new User
        {
            UserId = 1,
            Username = "admin",
            Role = "Admin"
        };
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, "123456");

        return new[] { user };
    }
}
