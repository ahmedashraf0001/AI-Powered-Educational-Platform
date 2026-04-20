$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Application\Features\Courses\Commands\Courses\CreateCourse\CreateCourseCommandHandler.cs"
$content = Get-Content $filePath -Raw

$ctorOld = @"
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
"@

$ctorNew = @"
        private readonly AIEduPlatform.Application.Common.Services.ITagExtractionQueue _tagExtractionQueue;

        public CreateCourseCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IFileService fileService,
            ILogger<CreateCourseCommandHandler> logger,
            AIEduPlatform.Application.Common.Services.ITagExtractionQueue tagExtractionQueue)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _fileService = fileService;
            _logger = logger;
            _tagExtractionQueue = tagExtractionQueue;
        }
"@

$triggerOld = @"
            return course.Id;
        }
"@

$triggerNew = @"
            _logger.LogInformation("Enqueuing tag extraction job for course {CourseId}", course.Id);
            await _tagExtractionQueue.EnqueueAsync(
                new AIEduPlatform.Application.Common.Services.TagExtractionRequest(course.Id, userId.Value),
                cancellationToken);

            return course.Id;
        }
"@

$content = $content -replace [regex]::Escape($ctorOld), $ctorNew
$content = $content -replace [regex]::Escape($triggerOld), $triggerNew

Set-Content -Path $filePath -Value $content
