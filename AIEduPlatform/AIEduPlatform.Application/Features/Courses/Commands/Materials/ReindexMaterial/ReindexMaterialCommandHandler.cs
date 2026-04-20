using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.ReindexMaterial;

public class ReindexMaterialCommandHandler : IRequestHandler<ReindexMaterialCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMaterialIndexingQueue _indexingQueue;
    private readonly ICurrentUserService _currentUserService;

    public ReindexMaterialCommandHandler(
        IUnitOfWork unitOfWork,
        IMaterialIndexingQueue indexingQueue,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _indexingQueue = indexingQueue;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ReindexMaterialCommand request, CancellationToken cancellationToken)
    {
        var material = await _unitOfWork.Materials.GetMaterialByIdAsync(request.MaterialId, includeChunks: false, cancellationToken);
        if (material == null)
            throw new NotFoundException("Material not found.");

        var lecture = await _unitOfWork.Lectures.GetLectureByIdAsync(material.LectureId, false, cancellationToken);
        if (lecture == null)
            throw new NotFoundException("Lecture not found.");

        var course = await _unitOfWork.Courses.GetCourseByIdAsync(lecture.CourseId, new CourseIncludeOptions { IncludeMaterials = false }, cancellationToken);
        if (course == null)
            throw new NotFoundException("Course not found.");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException("Not logged in");

        // Set to unindexed so RAG service will pick it up
        material.Indexed = false;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Queue course for indexing (which will index all unindexed materials for the course)
        await _indexingQueue.EnqueueAsync(
            new MaterialIndexingRequest(course.Id, userId), cancellationToken);

        return Unit.Value;
    }
}
