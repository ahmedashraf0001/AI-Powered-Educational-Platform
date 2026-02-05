using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.Infrastructure.Data;
using AIEduPlatform.Infrastructure.Repositories;
using AIEduPlatform.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIEduPlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext with PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    o => o.UseVector()));
            // Add Identity
            services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Add Unit of Work (coordinates multiple repositories)
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add Repositories - Course Management
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<ILectureRepository, LectureRepository>();
            services.AddScoped<IMaterialRepository, MaterialRepository>();
            services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

            // Add Repositories - Study Session Features
            services.AddScoped<IStudySessionRepository, StudySessionRepository>();
            services.AddScoped<IFlashcardRepository, FlashcardRepository>();
            services.AddScoped<IGeneratedQuizRepository, GeneratedQuizRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IMindMapRepository, MindMapRepository>();

            // Add Repositories - Exam Management
            services.AddScoped<IExamRepository, ExamRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<ISubmissionRepository, SubmissionRepository>();
            services.AddScoped<IGradeRepository, GradeRepository>();

            // Add Repositories - User Management
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Add Services
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }
}
