using Microsoft.EntityFrameworkCore;
using PRN232.LMS.StudentService.Entities;

namespace PRN232.LMS.StudentService.Data;

public class StudentDbContext : DbContext
{
    public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
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

        // Seed 50 Students
        var students = Enumerable.Range(1, 50)
            .Select(studentId => new Student
            {
                StudentId = studentId,
                FullName = $"Student {studentId:00}",
                Email = $"student{studentId:000}@lms.local",
                DateOfBirth = new DateTime(2000, 1, 1).AddDays(studentId * 30)
            }).ToArray();

        modelBuilder.Entity<Student>().HasData(students);
    }
}
