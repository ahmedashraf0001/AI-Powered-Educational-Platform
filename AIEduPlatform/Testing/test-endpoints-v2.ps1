########################################################################
# Comprehensive API Endpoint Testing Script - FIXED VERSION
# Using correct routes from endpoint discovery
########################################################################

$ErrorActionPreference = "Continue"
$base = "http://localhost:5069"
$testMaterials = "c:\Users\lyr1csan\Documents\Projects\AI-Powered-Educational-Platform\AIEduPlatform\TestMaterials"

$global:results = @()
$global:testNum = 0
$global:passed = 0
$global:failed = 0

function Log-Result {
    param([string]$Name, [string]$Result, [int]$Status, [string]$Response = "")
    $global:testNum++
    $num = $global:testNum
    $color = if ($Result -eq "PASS") { "Green" } else { "Red" }
    if ($Result -eq "PASS") { $global:passed++ } else { $global:failed++ }
    Write-Host "[$Result] #$num $Name (HTTP $Status)" -ForegroundColor $color
    $global:results += [PSCustomObject]@{ Num=$num; Test=$Name; Result=$Result; Status=$Status; Detail=$Response }
}

function Call-Api {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [hashtable]$Headers = @{},
        [int]$ExpectedStatus = 200
    )
    try {
        $params = @{ Uri=$Url; Method=$Method; UseBasicParsing=$true }
        if ($Headers.Count -gt 0) { $params.Headers = $Headers }
        if ($Body) { $params.Body = $Body; $params.ContentType = "application/json" }
        
        $resp = Invoke-WebRequest @params
        $status = [int]$resp.StatusCode
        $content = $resp.Content
        
        if ($status -eq $ExpectedStatus) { Log-Result $Name "PASS" $status }
        else { Log-Result $Name "FAIL(Expected $ExpectedStatus)" $status $content }
        
        try { return ($content | ConvertFrom-Json) } catch { return $content }
    } catch {
        $status = [int]$_.Exception.Response.StatusCode.value__
        $errBody = $_.ErrorDetails.Message
        
        if ($status -eq $ExpectedStatus) { Log-Result $Name "PASS" $status $errBody }
        else { Log-Result $Name "FAIL(Expected $ExpectedStatus)" $status $errBody }
        
        try { return ($errBody | ConvertFrom-Json) } catch { return $errBody }
    }
}

########################################################################
# STEP 0: Get auth tokens with good rate-limit handling
########################################################################
Write-Host "`n===== STEP 0: OBTAIN AUTH TOKENS =====" -ForegroundColor Cyan

# Login teacher
Write-Host "Logging in as teacher..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
$r = Invoke-WebRequest -Uri "$base/api/auth/login" -Method POST -Body '{"email":"teacher@test.com","password":"Teacher@123"}' -ContentType "application/json" -UseBasicParsing
$d = ($r.Content | ConvertFrom-Json).data
$global:TT = $d.accessToken   # Teacher Token
$global:TR = $d.refreshToken   # Teacher Refresh
Write-Host "  Teacher token obtained." -ForegroundColor Green

# Login student
Write-Host "Logging in as student..." -ForegroundColor Yellow  
Start-Sleep -Seconds 8
$r2 = Invoke-WebRequest -Uri "$base/api/auth/login" -Method POST -Body '{"email":"student@test.com","password":"Student@123"}' -ContentType "application/json" -UseBasicParsing
$d2 = ($r2.Content | ConvertFrom-Json).data
$global:ST = $d2.accessToken   # Student Token
$global:SR = $d2.refreshToken   # Student Refresh
Write-Host "  Student token obtained." -ForegroundColor Green

$TH = @{ Authorization = "Bearer $($global:TT)" }  # Teacher Headers
$SH = @{ Authorization = "Bearer $($global:ST)" }  # Student Headers

# Extract IDs from JWT
function Get-JwtClaim($token, $claim) {
    $parts = $token.Split('.')
    $payload = $parts[1]
    $pad = $payload.Length % 4
    if ($pad -gt 0) { $payload += '=' * (4 - $pad) }
    $json = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payload))
    $obj = $json | ConvertFrom-Json
    return $obj.$claim
}
$global:teacherId = Get-JwtClaim $global:TT "sub"
$global:studentId = Get-JwtClaim $global:ST "sub"
Write-Host "  TeacherId: $($global:teacherId)" -ForegroundColor Gray
Write-Host "  StudentId: $($global:studentId)" -ForegroundColor Gray

########################################################################
Write-Host "`n===== PHASE 1: AUTH EDGE CASES =====" -ForegroundColor Cyan
########################################################################

# 1. Duplicate email
Call-Api "Auth: Duplicate email register" POST "$base/api/auth/register" `
    '{"firstName":"Dup","lastName":"User","userName":"dupuser","email":"teacher@test.com","password":"Teacher@123","confirmPassword":"Teacher@123"}' @{} 400

# 2. Missing fields
Call-Api "Auth: Missing fields" POST "$base/api/auth/register" '{"email":""}' @{} 400

# 3. Password mismatch
Call-Api "Auth: Password mismatch" POST "$base/api/auth/register" `
    '{"firstName":"T","lastName":"U","userName":"mis","email":"mis@t.com","password":"Pass@123","confirmPassword":"Pass@456"}' @{} 400

# 4. Weak password
Call-Api "Auth: Weak password" POST "$base/api/auth/register" `
    '{"firstName":"T","lastName":"U","userName":"weak","email":"wk@t.com","password":"123","confirmPassword":"123"}' @{} 400

# 5. Wrong password login
Start-Sleep -Seconds 8
Call-Api "Auth: Wrong password" POST "$base/api/auth/login" `
    '{"email":"teacher@test.com","password":"WrongPass@123"}' @{} 400

# 6. Non-existent user login  
Start-Sleep -Seconds 8
Call-Api "Auth: Non-existent user" POST "$base/api/auth/login" `
    '{"email":"nobody@test.com","password":"Test@123"}' @{} 400

# 7. Refresh token - valid
$refreshBody = @{ accessToken=$global:TT; refreshToken=$global:TR } | ConvertTo-Json
$refreshResult = Call-Api "Auth: Refresh token" POST "$base/api/auth/refresh-token" $refreshBody @{} 200
if ($refreshResult.data.accessToken) {
    $global:TT = $refreshResult.data.accessToken
    $global:TR = $refreshResult.data.refreshToken
    $TH = @{ Authorization = "Bearer $($global:TT)" }
    Write-Host "  Tokens refreshed successfully" -ForegroundColor Gray
}

# 8. Refresh with invalid token
Call-Api "Auth: Invalid refresh" POST "$base/api/auth/refresh-token" '{"accessToken":"bad","refreshToken":"bad"}' @{} 400

# 9. No token on protected endpoint
Call-Api "Auth: No token" GET "$base/api/users/me" @{} @{} 401

# 10. Invalid bearer token
Call-Api "Auth: Bad bearer" GET "$base/api/users/me" $null @{Authorization="Bearer invalid"} 401

########################################################################
Write-Host "`n===== PHASE 2: USER ENDPOINTS =====" -ForegroundColor Cyan
########################################################################

# 11. Get teacher profile
$prof = Call-Api "User: Teacher profile" GET "$base/api/users/me" $null $TH 200
Write-Host "  Name: $($prof.data.firstName) $($prof.data.lastName)" -ForegroundColor Gray

# 12. Get student profile
$sprof = Call-Api "User: Student profile" GET "$base/api/users/me" $null $SH 200
Write-Host "  Name: $($sprof.data.firstName) $($sprof.data.lastName)" -ForegroundColor Gray

# 13. Update teacher profile
Call-Api "User: Update profile" PUT "$base/api/users/me" '{"firstName":"TeacherUPD","lastName":"TestUser"}' $TH 200

# 14. Verify update
$uprof = Call-Api "User: Verify update" GET "$base/api/users/me" $null $TH 200
if ($uprof.data.firstName -eq "TeacherUPD") { Write-Host "  Update verified" -ForegroundColor Gray }
else { Write-Host "  WARNING: firstName=$($uprof.data.firstName)" -ForegroundColor Yellow }

# 15. Revert
Call-Api "User: Revert profile" PUT "$base/api/users/me" '{"firstName":"Teacher","lastName":"TestUser"}' $TH 200

# 16. Become teacher (already)
Call-Api "User: Become teacher (already)" POST "$base/api/users/become-teacher" $null $TH 400

# 17. Get user by ID
Call-Api "User: Get by ID" GET "$base/api/users/$($global:studentId)" $null $TH 200

# 18. Get user stats
$stats = Call-Api "User: Stats" GET "$base/api/users/stats" $null $TH 200
Write-Host "  Stats: $($stats | ConvertTo-Json -Depth 2 -Compress)" -ForegroundColor Gray

# 19. Teacher dashboard
$dash = Call-Api "User: Teacher dashboard" GET "$base/api/users/teacher/dashboard" $null $TH 200
Write-Host "  Dashboard: $($dash | ConvertTo-Json -Depth 2 -Compress)" -ForegroundColor Gray

# 20. Teacher dashboard (student - forbidden)
Call-Api "User: Dashboard (student)" GET "$base/api/users/teacher/dashboard" $null $SH 403

# 21. Get invalid user ID
Call-Api "User: Invalid ID" GET "$base/api/users/00000000-0000-0000-0000-000000000000" $null $TH 404

########################################################################
Write-Host "`n===== PHASE 3: COURSE CRUD =====" -ForegroundColor Cyan
########################################################################

# 22. Create course
$c = Call-Api "Course: Create" POST "$base/api/courses" '{"title":"Test Course","description":"API testing course"}' $TH 200
$global:courseId = $c.data.id
Write-Host "  CourseId: $($global:courseId)" -ForegroundColor Gray

# 23. Create course (student-forbidden)
Call-Api "Course: Create (student)" POST "$base/api/courses" '{"title":"Fail","description":"x"}' $SH 403

# 24. Create course - validation fail
Call-Api "Course: Empty title" POST "$base/api/courses" '{"title":"","description":"x"}' $TH 400

# 25. Get course by ID
$cd = Call-Api "Course: Get by ID" GET "$base/api/courses/$($global:courseId)" @{} @{} 200
Write-Host "  Title: $($cd.data.title), Status: $($cd.data.status)" -ForegroundColor Gray

# 26. Update course
Call-Api "Course: Update" PUT "$base/api/courses/$($global:courseId)" `
    "{`"courseId`":`"$($global:courseId)`",`"title`":`"Updated Test Course`",`"description`":`"Updated desc`"}" $TH 200

# 27. Verify update
$ucd = Call-Api "Course: Verify update" GET "$base/api/courses/$($global:courseId)" @{} @{} 200
Write-Host "  Updated title: $($ucd.data.title)" -ForegroundColor Gray

# 28. Update course (student-forbidden)
Call-Api "Course: Update (student)" PUT "$base/api/courses/$($global:courseId)" `
    "{`"courseId`":`"$($global:courseId)`",`"title`":`"Hacked`",`"description`":`"x`"}" $SH 403

# 29. Get teacher's courses
$tc = Call-Api "Course: My courses" GET "$base/api/courses/my-courses?IncludeUnpublished=true" $null $TH 200
Write-Host "  Teacher courses: $($tc.data.items.Count)" -ForegroundColor Gray

# 30. Publish course
Call-Api "Course: Publish" POST "$base/api/courses/$($global:courseId)/publish" $null $TH 200

# 31. Verify publish
$pc = Call-Api "Course: Verify publish" GET "$base/api/courses/$($global:courseId)" @{} @{} 200
Write-Host "  Status: $($pc.data.status)" -ForegroundColor Gray

# 32. Search courses
$sc = Call-Api "Course: Search" GET "$base/api/courses/search?Keyword=Updated" @{} @{} 200
Write-Host "  Search results: $($sc.data.items.Count)" -ForegroundColor Gray

# 33. Get all published
$all = Call-Api "Course: Get all" GET "$base/api/courses" @{} @{} 200
Write-Host "  Published courses count: $(if($all.data.items){$all.data.items.Count}else{$all.data.Count})" -ForegroundColor Gray

# 34. Get course invalid ID
Call-Api "Course: Invalid ID" GET "$base/api/courses/00000000-0000-0000-0000-000000000000" @{} @{} 404

# 35. Get instructor courses
Call-Api "Course: Instructor courses" GET "$base/api/courses/instructor/$($global:teacherId)" $null $TH 200

########################################################################
Write-Host "`n===== PHASE 4: ENROLLMENTS =====" -ForegroundColor Cyan
########################################################################

# 36. Enroll student
$enr = Call-Api "Enroll: Student enroll" POST "$base/api/courses/$($global:courseId)/enroll" $null $SH 200
Write-Host "  Enrollment result: $($enr | ConvertTo-Json -Depth 2 -Compress)" -ForegroundColor Gray

# 37. Duplicate enrollment
Call-Api "Enroll: Duplicate" POST "$base/api/courses/$($global:courseId)/enroll" $null $SH 400

# 38. Get student enrolled courses
$enrolled = Call-Api "Enroll: My courses" GET "$base/api/courses/enrolled" $null $SH 200
Write-Host "  Enrolled courses: $(if($enrolled.data.items){$enrolled.data.items.Count}else{$enrolled.data.Count})" -ForegroundColor Gray

# 39. Get course enrollments (teacher)
$cenr = Call-Api "Enroll: Course enrollments" GET "$base/api/courses/$($global:courseId)/enrollments" $null $TH 200
Write-Host "  Course enrollments: $(if($cenr.data.items){$cenr.data.items.Count}else{$cenr.data.Count})" -ForegroundColor Gray

# 40. Teacher self-enroll (should fail - they own it)
Call-Api "Enroll: Teacher self-enroll" POST "$base/api/courses/$($global:courseId)/enroll" $null $TH 400

# 41. Enroll in non-existent course
Call-Api "Enroll: Non-existent course" POST "$base/api/courses/00000000-0000-0000-0000-000000000000/enroll" $null $SH 404

########################################################################
Write-Host "`n===== PHASE 5: LECTURES =====" -ForegroundColor Cyan
########################################################################

# 42. Create lecture
$lec = Call-Api "Lecture: Create" POST "$base/api/courses/$($global:courseId)/lectures" `
    "{`"courseId`":`"$($global:courseId)`",`"title`":`"Lecture 1: Intro`",`"description`":`"First lecture`",`"orderIndex`":1}" $TH 200
$global:lectureId = $lec.data.id
Write-Host "  LectureId: $($global:lectureId)" -ForegroundColor Gray

# 43. Create second lecture
$lec2 = Call-Api "Lecture: Create second" POST "$base/api/courses/$($global:courseId)/lectures" `
    "{`"courseId`":`"$($global:courseId)`",`"title`":`"Lecture 2: Advanced`",`"description`":`"Second`",`"orderIndex`":2}" $TH 200
$global:lectureId2 = $lec2.data.id

# 44. Create lecture (student)
Call-Api "Lecture: Create (student)" POST "$base/api/courses/$($global:courseId)/lectures" `
    "{`"courseId`":`"$($global:courseId)`",`"title`":`"Fail`",`"description`":`"x`",`"orderIndex`":3}" $SH 403

# 45. Get lecture by ID
$ld = Call-Api "Lecture: Get by ID" GET "$base/api/lectures/$($global:lectureId)" $null $TH 200
Write-Host "  Title: $($ld.data.title)" -ForegroundColor Gray

# 46. Get course lectures
$cls = Call-Api "Lecture: Get course lectures" GET "$base/api/courses/$($global:courseId)/lectures" $null $TH 200
Write-Host "  Lectures: $(if($cls.data.Count){$cls.data.Count}else{'?'})" -ForegroundColor Gray

# 47. Update lecture
Call-Api "Lecture: Update" PUT "$base/api/courses/lectures/$($global:lectureId)" `
    "{`"lectureId`":`"$($global:lectureId)`",`"title`":`"Lecture 1: Updated Intro`",`"description`":`"Updated`",`"orderIndex`":1}" $TH 200

# 48. Delete second lecture
Call-Api "Lecture: Delete" DELETE "$base/api/courses/lectures/$($global:lectureId2)" $null $TH 200

# 49. Verify deleted lecture
Call-Api "Lecture: Get deleted" GET "$base/api/lectures/$($global:lectureId2)" $null $TH 404

# 50. Recreate lecture 2 for materials
$lec2b = Call-Api "Lecture: Recreate" POST "$base/api/courses/$($global:courseId)/lectures" `
    "{`"courseId`":`"$($global:courseId)`",`"title`":`"Lecture 2: Materials`",`"description`":`"For materials`",`"orderIndex`":2}" $TH 200
$global:lectureId2 = $lec2b.data.id

########################################################################
Write-Host "`n===== PHASE 6: MATERIALS =====" -ForegroundColor Cyan
########################################################################

# 51. Upload PDF
Write-Host "  Uploading Documents.pdf..." -ForegroundColor Yellow
try {
    $pdfPath = "$testMaterials\Documents.pdf"
    $boundary = [System.Guid]::NewGuid().ToString()
    $fileBytes = [System.IO.File]::ReadAllBytes($pdfPath)
    $fileEnc = [System.Text.Encoding]::GetEncoding('iso-8859-1').GetString($fileBytes)
    $LF = "`r`n"
    $body = ("--$boundary", "Content-Disposition: form-data; name=`"Files`"; filename=`"Documents.pdf`"", "Content-Type: application/pdf", "", $fileEnc, "--$boundary--") -join $LF
    
    $r = Invoke-WebRequest -Uri "$base/api/courses/lectures/$($global:lectureId)/materials" `
        -Method POST -Body $body -ContentType "multipart/form-data; boundary=$boundary" -Headers $TH -UseBasicParsing
    $mat = ($r.Content | ConvertFrom-Json)
    $global:materialId = if ($mat.data -is [array]) { $mat.data[0].id } else { $mat.data.id }
    Log-Result "Material: Upload PDF" "PASS" $r.StatusCode
    Write-Host "  MaterialId: $($global:materialId)" -ForegroundColor Gray
} catch {
    Log-Result "Material: Upload PDF" "FAIL" $_.Exception.Response.StatusCode.value__ $_.ErrorDetails.Message
    Write-Host "  Error: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# 52. Upload Image  
Write-Host "  Uploading Image.png to lecture 2..." -ForegroundColor Yellow
try {
    $imgPath = "$testMaterials\Image.png"
    $boundary2 = [System.Guid]::NewGuid().ToString()
    $imgBytes = [System.IO.File]::ReadAllBytes($imgPath)
    $imgEnc = [System.Text.Encoding]::GetEncoding('iso-8859-1').GetString($imgBytes)
    $body2 = ("--$boundary2", "Content-Disposition: form-data; name=`"Files`"; filename=`"Image.png`"", "Content-Type: image/png", "", $imgEnc, "--$boundary2--") -join $LF
    
    $r2 = Invoke-WebRequest -Uri "$base/api/courses/lectures/$($global:lectureId2)/materials" `
        -Method POST -Body $body2 -ContentType "multipart/form-data; boundary=$boundary2" -Headers $TH -UseBasicParsing
    $mat2 = ($r2.Content | ConvertFrom-Json)
    $global:materialId2 = if ($mat2.data -is [array]) { $mat2.data[0].id } else { $mat2.data.id }
    Log-Result "Material: Upload Image" "PASS" $r2.StatusCode
} catch {
    Log-Result "Material: Upload Image" "FAIL" $_.Exception.Response.StatusCode.value__ $_.ErrorDetails.Message
    Write-Host "  Error: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# 53. Get lecture materials
if ($global:lectureId) {
    $lm = Call-Api "Material: Get lecture materials" GET "$base/api/courses/lectures/$($global:lectureId)/materials" $null $TH 200
    Write-Host "  Materials: $(if($lm.data -is [array]){$lm.data.Count}else{'?'})" -ForegroundColor Gray
}

# 54. Download material
if ($global:materialId) {
    try {
        $dl = Invoke-WebRequest -Uri "$base/api/materials/$($global:materialId)/download" -Headers $TH -UseBasicParsing
        Log-Result "Material: Download PDF" "PASS" $dl.StatusCode "Size: $($dl.RawContentLength) bytes"
        Write-Host "  Downloaded: $($dl.RawContentLength) bytes" -ForegroundColor Gray
    } catch {
        Log-Result "Material: Download PDF" "FAIL" $_.Exception.Response.StatusCode.value__
    }
}

# 55. Stream material
if ($global:materialId) {
    try {
        $sh2 = @{ Authorization = "Bearer $($global:TT)"; Range = "bytes=0-1023" }
        $st = Invoke-WebRequest -Uri "$base/api/materials/$($global:materialId)/stream" -Headers $sh2 -UseBasicParsing
        Log-Result "Material: Stream (Range)" "PASS" $st.StatusCode "Got $($st.RawContentLength) bytes"
    } catch {
        Log-Result "Material: Stream (Range)" "FAIL" $_.Exception.Response.StatusCode.value__
    }
}

# 56. Invalid material ID
Call-Api "Material: Invalid ID" GET "$base/api/materials/00000000-0000-0000-0000-000000000000/download" $null $TH 404

# 57. Delete material (image)
if ($global:materialId2) {
    Call-Api "Material: Delete image" DELETE "$base/api/courses/materials/$($global:materialId2)" $null $TH 200
}

########################################################################
Write-Host "`n===== PHASE 7: EXAMS =====" -ForegroundColor Cyan
########################################################################

$now = (Get-Date).ToUniversalTime()
$start = $now.AddMinutes(-10).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
$end = $now.AddHours(3).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

# 58. Create exam
$examBody = @{
    courseId = $global:courseId
    title = "Midterm Exam"
    startTime = $start
    endTime = $end
    durationMinutes = 60
} | ConvertTo-Json

$ex = Call-Api "Exam: Create" POST "$base/api/courses/$($global:courseId)/exams" $examBody $TH 200
$global:examId = $ex.data.id
Write-Host "  ExamId: $($global:examId)" -ForegroundColor Gray

# 59. Create exam (student-forbidden)
Call-Api "Exam: Create (student)" POST "$base/api/courses/$($global:courseId)/exams" $examBody $SH 403

# 60. Get exam
if ($global:examId) {
    $exd = Call-Api "Exam: Get by ID" GET "$base/api/exams/$($global:examId)" $null $TH 200
    Write-Host "  Title: $($exd.data.title)" -ForegroundColor Gray
}

# 61. Get course exams
$cex = Call-Api "Exam: Course exams" GET "$base/api/exams/course/$($global:courseId)" $null $TH 200
Write-Host "  Course exams: $(if($cex.data.items){$cex.data.items.Count}elseif($cex.data -is [array]){$cex.data.Count}else{'?'})" -ForegroundColor Gray

# 62. Update exam
if ($global:examId) {
    $uexBody = @{
        examId = $global:examId
        title = "Midterm Exam - Updated"
        startTime = $start
        endTime = $end
        durationMinutes = 90
    } | ConvertTo-Json
    Call-Api "Exam: Update" PUT "$base/api/exams/$($global:examId)" $uexBody $TH 200
}

# 63. Get exam total points
if ($global:examId) {
    $tp = Call-Api "Exam: Total points" GET "$base/api/exams/$($global:examId)/total-points" $null $TH 200
    Write-Host "  Total points: $($tp.data)" -ForegroundColor Gray
}

# 64. Get active exams
Call-Api "Exam: Active exams" GET "$base/api/exams/active/$($global:courseId)" $null $SH 200

# 65. Get upcoming exams
Call-Api "Exam: Upcoming exams" GET "$base/api/exams/upcoming/$($global:courseId)" $null $SH 200

# 66. Get past exams
Call-Api "Exam: Past exams" GET "$base/api/exams/past/$($global:courseId)" $null $SH 200

# 67. Get available exams
Call-Api "Exam: Available exams" GET "$base/api/exams/available" $null $SH 200

# 68. Invalid exam ID
Call-Api "Exam: Invalid ID" GET "$base/api/exams/00000000-0000-0000-0000-000000000000" $null $TH 404

########################################################################
Write-Host "`n===== PHASE 8: QUESTIONS =====" -ForegroundColor Cyan
########################################################################

if ($global:examId) {
    # 69. Add MCQ
    $mcqBody = @{
        examId = $global:examId
        type = "MultipleChoice"
        text = "What is the capital of France?"
        options = @("London","Paris","Berlin","Madrid")
        correctAnswer = "Paris"
        points = 10
    } | ConvertTo-Json -Depth 3
    $q1 = Call-Api "Question: Add MCQ" POST "$base/api/exams/$($global:examId)/questions" $mcqBody $TH 200
    $global:questionId = $q1.data.id
    Write-Host "  QuestionId: $($global:questionId)" -ForegroundColor Gray

    # 70. Add Essay
    $essayBody = @{
        examId = $global:examId
        type = "Essay"
        text = "Explain the theory of relativity."
        correctAnswer = "Einstein's theory describes the relationship between space, time, and gravity."
        points = 40
    } | ConvertTo-Json -Depth 3
    $q2 = Call-Api "Question: Add Essay" POST "$base/api/exams/$($global:examId)/questions" $essayBody $TH 200
    $global:essayQId = $q2.data.id

    # 71. Add TrueFalse
    $tfBody = @{
        examId = $global:examId
        type = "TrueFalse"
        text = "The Earth is flat."
        options = @("True","False")
        correctAnswer = "False"
        points = 10
    } | ConvertTo-Json -Depth 3
    $q3 = Call-Api "Question: Add TF" POST "$base/api/exams/$($global:examId)/questions" $tfBody $TH 200
    $global:tfQId = $q3.data.id

    # 72. Get exam questions
    $qs = Call-Api "Question: Get exam questions" GET "$base/api/exams/$($global:examId)/questions" $null $TH 200
    Write-Host "  Questions: $(if($qs.data -is [array]){$qs.data.Count}else{'?'})" -ForegroundColor Gray

    # 73. Update question
    if ($global:questionId) {
        $uqBody = @{
            questionId = $global:questionId
            type = "MultipleChoice"
            text = "What is the capital city of France?"
            options = @("London","Paris","Berlin","Rome")
            correctAnswer = "Paris"
            points = 15
        } | ConvertTo-Json -Depth 3
        Call-Api "Question: Update" PUT "$base/api/exams/questions/$($global:questionId)" $uqBody $TH 200
    }

    # 74. Add question (student-forbidden)
    Call-Api "Question: Add (student)" POST "$base/api/exams/$($global:examId)/questions" $mcqBody $SH 403

    # 75. Bulk add questions
    $bulkBody = @{
        examId = $global:examId
        questions = @(
            @{ type="MultipleChoice"; text="What is 2+2?"; options=@("3","4","5"); correctAnswer="4"; points=5 }
            @{ type="TrueFalse"; text="Water is wet."; options=@("True","False"); correctAnswer="True"; points=5 }
        )
    } | ConvertTo-Json -Depth 4
    Call-Api "Question: Bulk add" POST "$base/api/exams/$($global:examId)/questions/bulk" $bulkBody $TH 200

    # 76. Reorder questions
    $allQs = Call-Api "Question: Get all for reorder" GET "$base/api/exams/$($global:examId)/questions" $null $TH 200
    if ($allQs.data -is [array] -and $allQs.data.Count -ge 2) {
        $reorderMap = @{}
        $idx = $allQs.data.Count
        foreach ($q in $allQs.data) {
            $reorderMap[$q.id] = $idx
            $idx--
        }
        $reorderBody = @{ examId=$global:examId; questionOrders=$reorderMap } | ConvertTo-Json -Depth 3
        Call-Api "Question: Reorder" POST "$base/api/exams/$($global:examId)/questions/reorder" $reorderBody $TH 200
    }

    # 77. Delete TF question
    if ($global:tfQId) {
        Call-Api "Question: Delete" DELETE "$base/api/exams/questions/$($global:tfQId)" $null $TH 200
    }
}

########################################################################
Write-Host "`n===== PHASE 9: SUBMISSIONS =====" -ForegroundColor Cyan
########################################################################

if ($global:examId) {
    # 78. Submit exam (student)
    $answers = @{}
    if ($global:questionId) { $answers[$global:questionId] = "Paris" }
    if ($global:essayQId) { $answers[$global:essayQId] = "Einstein's theory of relativity describes how space and time are interconnected." }
    
    $subBody = @{
        examId = $global:examId
        answers = $answers
    } | ConvertTo-Json -Depth 3
    $sub = Call-Api "Submission: Submit exam" POST "$base/api/exams/$($global:examId)/submit" $subBody $SH 200
    $global:submissionId = $sub.data.id
    Write-Host "  SubmissionId: $($global:submissionId)" -ForegroundColor Gray

    # 79. Double submit (should fail)
    Call-Api "Submission: Double submit" POST "$base/api/exams/$($global:examId)/submit" $subBody $SH 400

    # 80. Get submission by ID
    if ($global:submissionId) {
        $sd = Call-Api "Submission: Get by ID" GET "$base/api/exams/submissions/$($global:submissionId)" $null $SH 200
        Write-Host "  Status: $($sd.data.status)" -ForegroundColor Gray
    }

    # 81. Get exam submissions (teacher)
    $esubs = Call-Api "Submission: Exam submissions" GET "$base/api/exams/$($global:examId)/submissions" $null $TH 200
    Write-Host "  Exam submissions: $(if($esubs.data.items){$esubs.data.items.Count}elseif($esubs.data -is [array]){$esubs.data.Count}else{'?'})" -ForegroundColor Gray

    # 82. Get student submissions
    $ssubs = Call-Api "Submission: Student submissions" GET "$base/api/exams/submissions/student" $null $SH 200

    # 83. Get ungraded submissions
    Call-Api "Submission: Ungraded" GET "$base/api/exams/submissions/ungraded?ExamId=$($global:examId)" $null $TH 200

    # 84. Submission stats
    Call-Api "Submission: Stats" GET "$base/api/submissions/stats/$($global:examId)" $null $TH 200
}

########################################################################
Write-Host "`n===== PHASE 10: GRADING =====" -ForegroundColor Cyan
########################################################################

if ($global:submissionId) {
    # 85. Manual grade
    $gradeBody = @{
        submissionId = $global:submissionId
        score = 85.0
        feedback = "Good work! Excellent essay response."
    } | ConvertTo-Json
    $grade = Call-Api "Grade: Manual grade" POST "$base/api/exams/submissions/$($global:submissionId)/grade" $gradeBody $TH 200
    $global:gradeId = $grade.data.id
    Write-Host "  GradeId: $($global:gradeId)" -ForegroundColor Gray

    # 86. Get grade for submission
    $sg = Call-Api "Grade: Get submission grade" GET "$base/api/exams/submissions/$($global:submissionId)/grade" $null $SH 200
    Write-Host "  Score: $($sg.data.score)" -ForegroundColor Gray

    # 87. Update grade
    if ($global:gradeId) {
        $ugBody = @{ gradeId=$global:gradeId; score=90.0; feedback="Updated: Excellent work!" } | ConvertTo-Json
        Call-Api "Grade: Update" PUT "$base/api/exams/grades/$($global:gradeId)" $ugBody $TH 200
    }

    # 88. Approve grade
    if ($global:gradeId) {
        Call-Api "Grade: Approve" POST "$base/api/exams/grades/$($global:gradeId)/approve" $null $TH 200
    }

    # 89. Get exam grades
    $egs = Call-Api "Grade: Exam grades" GET "$base/api/exams/$($global:examId)/grades" $null $TH 200

    # 90. Student grades
    $sgs = Call-Api "Grade: Student grades" GET "$base/api/exams/grades/student" $null $SH 200

    # 91. Pending approval
    Call-Api "Grade: Pending approval" GET "$base/api/exams/grades/pending-approval" $null $TH 200

    # 92. Grade stats for exam
    Call-Api "Grade: Exam stats" GET "$base/api/grades/stats/exam/$($global:examId)" $null $TH 200

    # 93. Grade distribution
    Call-Api "Grade: Distribution" GET "$base/api/grades/distribution/$($global:examId)" $null $TH 200

    # 94. Student grade stats
    Call-Api "Grade: Student stats" GET "$base/api/grades/stats/student/$($global:studentId)" $null $TH 200

    # 95. Grade by student (forbidden)
    Call-Api "Grade: Student grade (forbidden)" POST "$base/api/exams/submissions/$($global:submissionId)/grade" $gradeBody $SH 403
}

########################################################################
Write-Host "`n===== PHASE 11: REVIEWS =====" -ForegroundColor Cyan
########################################################################

# 96. Add review (student)
$revBody = @{ courseId=$global:courseId; rating=4; comment="Great course! Well-structured." } | ConvertTo-Json
$rev = Call-Api "Review: Add" POST "$base/api/courses/$($global:courseId)/reviews" $revBody $SH 200
$global:reviewId = $rev.data.id
Write-Host "  ReviewId: $($global:reviewId)" -ForegroundColor Gray

# 97. Duplicate review
Call-Api "Review: Duplicate" POST "$base/api/courses/$($global:courseId)/reviews" $revBody $SH 400

# 98. Get course reviews
$crevs = Call-Api "Review: Course reviews" GET "$base/api/courses/$($global:courseId)/reviews" @{} @{} 200
Write-Host "  Reviews: $(if($crevs.data.items){$crevs.data.items.Count}elseif($crevs.data -is [array]){$crevs.data.Count}else{'?'})" -ForegroundColor Gray

# 99. Get rating summary
$rating = Call-Api "Review: Rating summary" GET "$base/api/courses/$($global:courseId)/rating" @{} @{} 200
Write-Host "  Rating: $($rating.data.averageRating)" -ForegroundColor Gray

# 100. Update review
if ($global:reviewId) {
    $urevBody = @{ reviewId=$global:reviewId; rating=5; comment="Updated: Excellent!" } | ConvertTo-Json
    Call-Api "Review: Update" PUT "$base/api/reviews/$($global:reviewId)" $urevBody $SH 200
}

# 101. Verify updated rating
$urating = Call-Api "Review: Updated rating" GET "$base/api/courses/$($global:courseId)/rating" @{} @{} 200
Write-Host "  Updated rating: $($urating.data.averageRating)" -ForegroundColor Gray

# 102. Delete review
if ($global:reviewId) {
    Call-Api "Review: Delete" DELETE "$base/api/reviews/$($global:reviewId)" $null $SH 200
}

########################################################################
Write-Host "`n===== PHASE 12: ENGAGEMENT =====" -ForegroundColor Cyan
########################################################################

# 103. Get engagement report
$eng = Call-Api "Engagement: Get report" GET "$base/api/courses/$($global:courseId)/engagement" $null $TH 200
Write-Host "  Engagement: $($eng | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor Gray

# 104. Get engagement (student-forbidden)
Call-Api "Engagement: Student (forbidden)" GET "$base/api/courses/$($global:courseId)/engagement" $null $SH 403

# 105. Send alerts
$alertBody = @{ courseId=$global:courseId } | ConvertTo-Json
Call-Api "Engagement: Send alerts" POST "$base/api/courses/$($global:courseId)/engagement/alerts" $alertBody $TH 200

# 106. Send alerts to specific students
$alertBody2 = @{ courseId=$global:courseId; studentIds=@($global:studentId) } | ConvertTo-Json
Call-Api "Engagement: Send targeted alert" POST "$base/api/courses/$($global:courseId)/engagement/alerts" $alertBody2 $TH 200

########################################################################
Write-Host "`n===== PHASE 13: DIALOGUE ENDPOINTS =====" -ForegroundColor Cyan
########################################################################

# 107. Get voices
Call-Api "Dialogue: Get voices" GET "$base/api/dialogue/voices" $null $TH 200

# 108. Get default voice config
Call-Api "Dialogue: Default config" GET "$base/api/dialogue/voice-config/default" $null $TH 200

# 109. Get supported formats
Call-Api "Dialogue: Formats" GET "$base/api/dialogue/supported-formats" $null $TH 200

# 110. Get supported languages
Call-Api "Dialogue: Languages" GET "$base/api/dialogue/supported-languages" $null $TH 200

########################################################################
Write-Host "`n===== PHASE 14: ENROLLMENT COMPLETE & UNENROLL =====" -ForegroundColor Cyan
########################################################################

# 111. Complete enrollment
Call-Api "Enroll: Complete" POST "$base/api/courses/$($global:courseId)/complete" $null $SH 200

# 112. Unenroll
Call-Api "Enroll: Unenroll" DELETE "$base/api/courses/$($global:courseId)/unenroll" $null $SH 200

# 113. Verify unenrolled
$afterUnenroll = Call-Api "Enroll: Verify unenrolled" GET "$base/api/courses/enrolled" $null $SH 200

########################################################################
Write-Host "`n===== PHASE 15: EDGE CASES =====" -ForegroundColor Cyan
########################################################################

# 114. Very long title
$longTitle = "A" * 500
$longBody = @{ title=$longTitle; description="Test" } | ConvertTo-Json
Call-Api "Edge: Long title" POST "$base/api/courses" $longBody $TH 400

# 115. XSS in title
$xssBody = @{ title="<script>alert('xss')</script>"; description="SQL' OR 1=1 --" } | ConvertTo-Json
$xssResult = Call-Api "Edge: XSS/SQL injection" POST "$base/api/courses" $xssBody $TH 200
if ($xssResult.data.id) {
    Write-Host "  XSS stored as: $($xssResult.data.title)" -ForegroundColor Gray
    Call-Api "Edge: Cleanup XSS course" DELETE "$base/api/courses/$($xssResult.data.id)" $null $TH 200
}

# 116. Empty body
Call-Api "Edge: Empty body" POST "$base/api/courses" '{}' $TH 400

# 117. Negative exam values
$negBody = @{ courseId=$global:courseId; title="Neg"; startTime=$start; endTime=$end; durationMinutes=-10 } | ConvertTo-Json
Call-Api "Edge: Negative duration" POST "$base/api/courses/$($global:courseId)/exams" $negBody $TH 400

# 118. Invalid rating (0)
$ir0 = @{ courseId=$global:courseId; rating=0; comment="Bad" } | ConvertTo-Json
# Re-enroll first for review
Call-Api "Edge: Re-enroll for review" POST "$base/api/courses/$($global:courseId)/enroll" $null $SH 200
Call-Api "Edge: Rating 0" POST "$base/api/courses/$($global:courseId)/reviews" $ir0 $SH 400

# 119. Invalid rating (6)
$ir6 = @{ courseId=$global:courseId; rating=6; comment="Too high" } | ConvertTo-Json
Call-Api "Edge: Rating 6" POST "$base/api/courses/$($global:courseId)/reviews" $ir6 $SH 400

# 120. Logout
$logoutBody = @{ refreshToken=$global:TR } | ConvertTo-Json
Call-Api "Auth: Logout" POST "$base/api/auth/logout" $logoutBody $TH 200

# 121. Access after logout
Call-Api "Auth: Access after logout" GET "$base/api/users/me" $null $TH 401

########################################################################
Write-Host "`n`n" -ForegroundColor White
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host "                    TEST RESULTS SUMMARY" -ForegroundColor Cyan
Write-Host "================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total:  $($global:testNum)" -ForegroundColor White
Write-Host "Passed: $($global:passed)" -ForegroundColor Green
Write-Host "Failed: $($global:failed)" -ForegroundColor $(if($global:failed -gt 0){"Red"}else{"Green"})
Write-Host ""

if ($global:failed -gt 0) {
    Write-Host "FAILED TESTS:" -ForegroundColor Red
    Write-Host "─────────────" -ForegroundColor Red
    $global:results | Where-Object { $_.Result -ne "PASS" } | ForEach-Object {
        Write-Host "  #$($_.Num) $($_.Test) → $($_.Result) [HTTP $($_.Status)]" -ForegroundColor Red
        if ($_.Detail) {
            $detailStr = if ($_.Detail -is [string]) { $_.Detail } else { $_.Detail | ConvertTo-Json -Compress }
            if ($detailStr.Length -gt 200) { $detailStr = $detailStr.Substring(0, 200) + "..." }
            Write-Host "     $detailStr" -ForegroundColor DarkRed
        }
    }
}

Write-Host "`nAll Results:" -ForegroundColor White
$global:results | Format-Table Num, Test, Result, Status -AutoSize -Wrap
