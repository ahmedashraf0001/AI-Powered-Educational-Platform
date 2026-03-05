using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.CreateCourse
{
    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;
        private readonly ILogger<CreateCourseCommandHandler> _logger;

        public CreateCourseCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFileService fileService,
            ILogger<CreateCourseCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to create a course.");
            }

            _logger.LogInformation(
                "Creating course with title: {Title} for teacher: {TeacherId}",
                request.Title,
                userId.Value);

            try
            {
                var course = new Course
                {
                    Title = request.Title,
                    Description = request.Description,
                    TeacherId = userId.Value,
                    IsPublished = false,
                    Price = request.Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Upload thumbnail if provided
                if (request.ThumbnailStream != null && !string.IsNullOrEmpty(request.ThumbnailFileName))
                {
                    var uploadResult = await _fileService.UploadFileAsync(
                        request.ThumbnailStream,
                        request.ThumbnailFileName,
                        request.ThumbnailContentType ?? "image/jpeg",
                        "thumbnails/courses",
                        cancellationToken);

                    if (uploadResult.Success)
                        course.ThumbnailUrl = uploadResult.FileUrl;
                    else
                        _logger.LogWarning("Failed to upload course thumbnail: {Error}", uploadResult.ErrorMessage);
                }

                var createdCourse = await _unitOfWork.Courses.AddAsync(course);

                // Associate with category if provided
                if (request.CategoryId.HasValue)
                {
                    var courseCategory = new CourseCategory
                    {
                        CourseId = createdCourse.Id,
                        CategoryId = request.CategoryId.Value
                    };
                    await _unitOfWork.CourseCategories.AddAsync(courseCategory);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully created course with ID: {CourseId}, Title: {Title}",
                    createdCourse.Id,
                    createdCourse.Title);

                return createdCourse.Id;
            }
            catch (Exception ex) when (ex is not UnauthorizedException)
            {
                _logger.LogError(
                    ex,
                    "Error creating course with title: {Title} for teacher: {TeacherId}",
                    request.Title,
                    userId.Value);

                throw;
            }
        }
    }
}