using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Materials;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sections.GetSectionsByMaterial
{
    public class GetSectionsByMaterialQueryHandler : IRequestHandler<GetSectionsByMaterialQuery, List<SemanticSectionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSectionsByMaterialQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<SemanticSectionDto>> Handle(GetSectionsByMaterialQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var material = await _unitOfWork.Materials.GetByIdAsync(request.MaterialId, cancellationToken);
            if (material is null)
                throw new NotFoundException(nameof(Material), request.MaterialId);

            var sections = await _unitOfWork.SemanticSections.GetByMaterialIdAsync(request.MaterialId, cancellationToken);

            return sections.Select(s => new SemanticSectionDto
            {
                Id = s.Id,
                Title = s.Title,
                Summary = s.Summary,
                StartSeconds = s.StartSeconds,
                EndSeconds = s.EndSeconds,
                StartPage = s.StartPage,
                EndPage = s.EndPage,
                OrderIndex = s.OrderIndex
            }).ToList();
        }
    }
}
