using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial
{
    public class UploadMaterialCommandHandler : IRequestHandler<UploadMaterialCommand, List<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMaterialIndexingQueue _indexingQueue;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UploadMaterialCommandHandler> _logger;

        private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

        public UploadMaterialCommandHandler(
            IUnitOfWork unitOfWork,
            IFileService fileService,
            ICurrentUserService currentUserService,
            IMaterialIndexingQueue indexingQueue,
            INotificationService notificationService,
            ILogger<UploadMaterialCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _currentUserService = currentUserService;
            _indexingQueue = indexingQueue;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<Guid>> Handle(UploadMaterialCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to upload materials.");

            if (request.Files == null || request.Files.Count == 0)
                throw new BadRequestException("At least one file must be provided.");

            _logger.LogInformation(
                "Uploading {Count} materials to lecture. LectureId: {LectureId}, UserId: {UserId}",
                request.Files.Count,
                request.LectureId,
                userId.Value);

            try
            {
                var lecture = await _unitOfWork.Lectures.GetLectureByIdAsync(
                    request.LectureId,
                    includeMaterials: false,
                    cancellationToken);

                if (lecture == null)
                    throw new NotFoundException(nameof(Lecture), request.LectureId);

                var course = await _unitOfWork.Courses.GetByIdAsync(lecture.CourseId, cancellationToken);

                if (course == null || course.TeacherId != userId.Value)
                    throw new ForbiddenException("You are not authorized to upload materials to this lecture.");

                var materialIds = new List<Guid>();

                foreach (var file in request.Files)
                {
                    var fileUrl = await UploadFileAsync(file, request.LectureId, cancellationToken);
                    var materialType = FileExtensionConfiguration.GetMaterialType(file.FileName);

                    var material = new Material
                    {
                        LectureId = request.LectureId,
                        Type = materialType,
                        Title = file.Title,
                        FileUrl = fileUrl,
                        Indexed = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var created = await _unitOfWork.Materials.AddAsync(material, cancellationToken);
                    materialIds.Add(created.Id);
                }

                course.NeedsTagRebuild = true;
                course.PendingContentChanges += request.Files.Count;
                await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _indexingQueue.EnqueueAsync(
                    new MaterialIndexingRequest(course.Id, userId.Value), cancellationToken);

                var materialTitles = string.Join(", ", request.Files.Select(f => f.Title));
                await _notificationService.NotifyNewMaterialUploadedAsync(
                    course.Id, request.LectureId, course.Title, materialTitles, cancellationToken);

                _logger.LogInformation(
                    "Successfully uploaded {Count} materials to lecture {LectureId}",
                    materialIds.Count,
                    request.LectureId);

                return materialIds;
            }
            catch (Exception ex) when (ex is not NotFoundException and not BadRequestException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error uploading materials to lecture. LectureId: {LectureId}", request.LectureId);
                throw;
            }
        }

        private async Task<string> UploadFileAsync(UploadMaterialFile file, Guid lectureId, CancellationToken cancellationToken)
        {
            // Note: File validation (type and size) should be done at the endpoint/API layer
            // This provides defense in depth validation
            if (!FileExtensionConfiguration.IsSupported(file.FileName))
            {
                throw new BadRequestException(
                    $"Invalid file type for '{file.FileName}'. Allowed types: {FileExtensionConfiguration.GetSupportedExtensionsString()}");
            }

            if (file.FileStream.Length > MaxFileSize)
                throw new BadRequestException($"File '{file.FileName}' exceeds the maximum allowed size of 100 MB.");

            var uploadResult = await _fileService.UploadFileAsync(
                file.FileStream,
                file.FileName,
                file.ContentType,
                $"materials/{lectureId}",
                cancellationToken);

            if (!uploadResult.Success)
                throw new BadRequestException(uploadResult.ErrorMessage ?? $"Failed to upload file '{file.FileName}'.");

            return uploadResult.FileUrl!;
        }
    }
}

