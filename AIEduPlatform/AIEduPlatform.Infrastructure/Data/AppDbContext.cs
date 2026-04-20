using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.RAG.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

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
        public DbSet<MaterialChunk> Chunks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Concept> Concepts { get; set; }
        public DbSet<ConceptRelation> ConceptRelations { get; set; }
        public DbSet<ConceptChunkMap> ConceptChunkMaps { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<MaterialProgress> MaterialProgresses { get; set; }
        public DbSet<SemanticSection> SemanticSections { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserVoiceSettings> UserVoiceSettings { get; set; }
        public DbSet<ExamAttempt> ExamAttempts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<CourseTag> CourseTags { get; set; }
        public DbSet<UserTag> UserTags { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added) entry.Entity.CreatedAt = DateTime.UtcNow;
                if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            return base.SaveChangesAsync(ct);
        }
    }
}
