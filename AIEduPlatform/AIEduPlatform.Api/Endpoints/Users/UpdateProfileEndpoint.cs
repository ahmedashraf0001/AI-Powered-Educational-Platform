using AIEduPlatform.Application.Features.Users.Commands.UpdateProfile;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AIEduPlatform.Api.Endpoints.Users;

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public IFormFile? Avatar { get; set; }
    public bool RemoveAvatar { get; set; }
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Location { get; set; }
    public string? Qualifications { get; set; }
    public string? ExpertiseAreas { get; set; }
}

public class UpdateProfileEndpoint : Endpoint<UpdateProfileRequest, object>
{
    private readonly IMediator _mediator;

    public UpdateProfileEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/users/me");
        AllowFormData();
        AllowFileUploads();
        Group<UsersGroup>();
        Summary(s =>
        {
            s.Summary = "Update my profile";
            s.Description = "Updates the authenticated user's profile. Supports avatar file upload. Send as multipart/form-data when uploading an avatar, or as JSON for text-only updates. Set RemoveAvatar=true to remove the current avatar.";
            s.Response<ApiResponse<object>>(200, "Profile updated");
            s.Response(400, "Username already taken or validation error");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
    {
        Stream? avatarStream = null;
        string? avatarFileName = null;
        string? avatarContentType = null;

        if (req.Avatar != null && req.Avatar.Length > 0)
        {
            var ms = new MemoryStream();
            await req.Avatar.CopyToAsync(ms, ct);
            ms.Position = 0;
            avatarStream = ms;
            avatarFileName = req.Avatar.FileName;
            avatarContentType = req.Avatar.ContentType;
        }

        await _mediator.Send(new UpdateProfileCommand
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            UserName = req.UserName,
            Bio = req.Bio,
            AvatarUrl = req.AvatarUrl,
            AvatarStream = avatarStream,
            AvatarFileName = avatarFileName,
            AvatarContentType = avatarContentType,
            RemoveAvatar = req.RemoveAvatar,
            Website = req.Website,
            LinkedInUrl = req.LinkedInUrl,
            Location = req.Location,
            Qualifications = req.Qualifications,
            ExpertiseAreas = req.ExpertiseAreas,
        }, ct);

        await SendOkAsync(new { Success = true, Message = "Profile Updated Successfully!" }, ct);
    }
}
