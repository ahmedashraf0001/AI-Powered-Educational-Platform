using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial
{
    public class UploadMaterialCommandHandler : IRequestHandler<UploadMaterialCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UploadMaterialCommandHandler> _logger;

        private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx", ".ppt", ".pptx", ".mp4", ".mp3", ".jpg", ".png"];
        private const long MaxFileSize = 100 * 1024 * 1024;

        public UploadMaterialCommandHandler(
            IUnitOfWork unitOfWork,
            IFileService fileService,
            ICurrentUserService currentUserService,
            ILogger<UploadMaterialCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Guid> Handle(UploadMaterialCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to upload materials.");
            }

            _logger.LogInformation(
                "Uploading material to lecture. LectureId: {LectureId}, Title: {Title}, UserId: {UserId}",
                request.LectureId,
                request.Title,
                userId.Value);

            try
            {
                var lecture = await _unitOfWork.Lectures.GetLectureByIdAsync(
                    request.LectureId,
                    includeMaterials: false,
                    cancellationToken);

                if (lecture == null)
                {
                    _logger.LogWarning("Lecture not found. LectureId: {LectureId}", request.LectureId);
                    throw new NotFoundException(nameof(Lecture), request.LectureId);
                }

                var course = await _unitOfWork.Courses.GetByIdAsync(lecture.CourseId, cancellationToken);

                if (course == null || course.TeacherId != userId.Value)
                {
                    _logger.LogWarning(
                        "User {UserId} is not authorized to upload materials to lecture {LectureId}",
                        userId.Value,
                        request.LectureId);
                    throw new ForbiddenException("You are not authorized to upload materials to this lecture.");
                }

                var fileUrl = await ResolveFileUrlAsync(request, cancellationToken);

                var material = new Material
                {
                    LectureId = request.LectureId,
                    Type = request.Type,
                    Title = request.Title,
                    FileUrl = fileUrl,
                    Indexed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdMaterial = await _unitOfWork.Materials.AddAsync(material, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully uploaded material. MaterialId: {MaterialId}, LectureId: {LectureId}, Title: {Title}",
                    createdMaterial.Id,
                    request.LectureId,
                    material.Title);

                return createdMaterial.Id;
            }
            catch (Exception ex) when (ex is not NotFoundException and not BadRequestException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error uploading material to lecture. LectureId: {LectureId}", request.LectureId);
                throw;
            }
        }

        private async Task<string> ResolveFileUrlAsync(UploadMaterialCommand request, CancellationToken cancellationToken)
        {
            if (request.FileStream != null && !string.IsNullOrEmpty(request.FileName))
            {
                if (!_fileService.IsValidFileType(request.FileName, AllowedExtensions))
                {
                    throw new BadRequestException("Invalid file type. Allowed types: " + string.Join(", ", AllowedExtensions));
                }

                if (!_fileService.IsValidFileSize(request.FileStream.Length, MaxFileSize))
                {
                    throw new BadRequestException("File size exceeds the maximum allowed size of 100 MB.");
                }

                var uploadResult = await _fileService.UploadFileAsync(
                    request.FileStream,
                    request.FileName,
                    request.ContentType ?? "application/octet-stream",
                    $"materials/{request.LectureId}",
                    cancellationToken);

                if (!uploadResult.Success)
                {
                    throw new BadRequestException(uploadResult.ErrorMessage ?? "Failed to upload file.");
                }

                return uploadResult.FileUrl!;
            }

            if (!string.IsNullOrEmpty(request.FileUrl))
            {
                return request.FileUrl;
            }

            throw new BadRequestException("Either a file or file URL must be provided.");
        }
    }
}
