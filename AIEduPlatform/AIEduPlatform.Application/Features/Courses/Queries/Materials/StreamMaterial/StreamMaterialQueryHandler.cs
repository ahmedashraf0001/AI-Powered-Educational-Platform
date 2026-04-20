using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Materials.StreamMaterial
{
    public class StreamMaterialQueryHandler : IRequestHandler<StreamMaterialQuery, StreamMaterialResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;
        private readonly ILogger<StreamMaterialQueryHandler> _logger;

        private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".mp4"] = "video/mp4",
            [".webm"] = "video/webm",
            [".mp3"] = "audio/mpeg",
            [".wav"] = "audio/wav",
            [".ogg"] = "audio/ogg",
            [".pdf"] = "application/pdf",
            [".doc"] = "application/msword",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            [".ppt"] = "application/vnd.ms-powerpoint",
            [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".gif"] = "image/gif",
            [".svg"] = "image/svg+xml"
        };

        public StreamMaterialQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFileService fileService,
            ILogger<StreamMaterialQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<StreamMaterialResult> Handle(StreamMaterialQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to access materials.");

            // Get material with lecture info
            var material = await _unitOfWork.Materials.GetMaterialByIdAsync(
                request.MaterialId, includeChunks: false, cancellationToken);

            if (material == null)
                throw new NotFoundException(nameof(Material), request.MaterialId);

            // Get lecture to find course
            var lecture = await _unitOfWork.Lectures.GetByIdAsync(material.LectureId, cancellationToken);
            if (lecture == null)
                throw new NotFoundException(nameof(Lecture), material.LectureId);

            // Authorization: must be enrolled or instructor
            var course = await _unitOfWork.Courses.GetByIdAsync(lecture.CourseId, cancellationToken);
            if (course == null)
                throw new NotFoundException(nameof(Course), lecture.CourseId);

            var isInstructor = course.TeacherId == userId.Value;
            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(
                userId.Value, lecture.CourseId, cancellationToken);

            if (!isInstructor && !isEnrolled)
                throw new ForbiddenException("You must be enrolled in this course or be the instructor to access materials.");

            // Resolve physical file path via shared file service so stream behavior matches indexing/upload flows.
            // Supports both relative URLs (/uploads/...) and absolute URLs (https://host/uploads/...).
            var fileUrl = material.FileUrl;
            if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var absoluteUri))
            {
                fileUrl = absoluteUri.AbsolutePath;
            }

            var queryIndex = fileUrl.IndexOf('?');
            if (queryIndex >= 0)
            {
                fileUrl = fileUrl[..queryIndex];
            }

            var hashIndex = fileUrl.IndexOf('#');
            if (hashIndex >= 0)
            {
                fileUrl = fileUrl[..hashIndex];
            }

            fileUrl = Uri.UnescapeDataString(fileUrl);
            var physicalPath = _fileService.ResolvePhysicalPath(fileUrl);

            if (!File.Exists(physicalPath))
            {
                _logger.LogError(
                    "Physical file not found for material {MaterialId}. FileUrl: {FileUrl}, Path: {Path}",
                    request.MaterialId,
                    material.FileUrl,
                    physicalPath);
                throw new NotFoundException("File", request.MaterialId);
            }

            // Determine content type from file extension
            var extension = Path.GetExtension(physicalPath);
            var contentType = MimeTypes.GetValueOrDefault(extension, "application/octet-stream");
            var fileName = Path.GetFileName(physicalPath);

            // Strip the GUID prefix from the filename for a clean download name
            var underscoreIndex = fileName.IndexOf('_');
            var cleanFileName = underscoreIndex > 0 ? fileName[(underscoreIndex + 1)..] : fileName;

            _logger.LogInformation(
                "Streaming material {MaterialId} ({Type}) to user {UserId}",
                request.MaterialId, material.Type, userId.Value);

            return new StreamMaterialResult
            {
                FilePath = physicalPath,
                ContentType = contentType,
                FileName = cleanFileName,
                Type = material.Type
            };
        }
    }
}
