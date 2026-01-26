using AIEduPlatform.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AIEduPlatform.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<StudySession> StudySessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<GeneratedQuiz> GeneratedQuizzes { get; set; }
        public DbSet<Flashcard> Flashcards { get; set; }
        public DbSet<MindMap> MindMaps { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
