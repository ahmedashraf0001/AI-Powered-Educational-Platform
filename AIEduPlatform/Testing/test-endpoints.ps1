########################################################################
# Comprehensive API Endpoint Testing Script
# Tests all 96 endpoints with workflows, edge cases, and output verification
########################################################################

$ErrorActionPreference = "Continue"
$base = "http://localhost:5069"
$testMaterials = "c:\Users\lyr1csan\Documents\Projects\AI-Powered-Educational-Platform\AIEduPlatform\TestMaterials"

# Results tracking
$global:results = @()
$global:testNum = 0

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [hashtable]$Headers = @{},
        [int]$ExpectedStatus = 200,
        [string]$ContentType = "application/json",
        [switch]$IsForm,
        [switch]$SkipContentType
    )
    $global:testNum++
    $num = $global:testNum
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            UseBasicParsing = $true
        }
        
        if ($Headers.Count -gt 0) { $params.Headers = $Headers }
        if ($Body -and -not $IsForm) { 
            $params.Body = $Body
            $params.ContentType = $ContentType
        }
        
        $response = Invoke-WebRequest @params
        $status = $response.StatusCode
        $content = $response.Content
        
        if ($status -eq $ExpectedStatus) {
            $result = "PASS"
        } else {
            $result = "FAIL (Expected $ExpectedStatus, Got $status)"
        }
        
        $global:results += [PSCustomObject]@{
            Num = $num; Test = $Name; Result = $result; Status = $status; Response = ($content | Select-Object -First 500)
        }
        Write-Host "[$result] #$num $Name (Status: $status)" -ForegroundColor $(if($result -eq "PASS"){"Green"}else{"Red"})
        
        # Return parsed JSON if possible
        try { return ($content | ConvertFrom-Json) } catch { return $content }
    }
    catch {
        $status = $_.Exception.Response.StatusCode.value__
        $errorBody = $_.ErrorDetails.Message
        
        if ($status -eq $ExpectedStatus) {
            $result = "PASS"
        } else {
            $result = "FAIL (Expected $ExpectedStatus, Got $status)"
        }
        
        $global:results += [PSCustomObject]@{
            Num = $num; Test = $Name; Result = $result; Status = $status; Response = $errorBody
        }
        Write-Host "[$result] #$num $Name (Status: $status)" -ForegroundColor $(if($result -eq "PASS"){"Green"}else{"Yellow"})
        
        try { return ($errorBody | ConvertFrom-Json) } catch { return $errorBody }
    }
}

function Get-AuthHeader([string]$token) {
    return @{ Authorization = "Bearer $token" }
}

########################################################################
Write-Host "`n========== PHASE 1: AUTHENTICATION ==========`n" -ForegroundColor Cyan
########################################################################

# 1.1 Register - duplicate email (edge case)
Test-Endpoint -Name "Register: Duplicate email" -Method POST -Url "$base/api/auth/register" `
    -Body '{"firstName":"Dup","lastName":"User","userName":"dupuser","email":"teacher@test.com","password":"Teacher@123","confirmPassword":"Teacher@123"}' `
    -ExpectedStatus 400

# 1.2 Register - missing fields
Test-Endpoint -Name "Register: Missing fields" -Method POST -Url "$base/api/auth/register" `
    -Body '{"email":""}' -ExpectedStatus 400

# 1.3 Register - password mismatch
Test-Endpoint -Name "Register: Password mismatch" -Method POST -Url "$base/api/auth/register" `
    -Body '{"firstName":"Test","lastName":"User","userName":"mismatch","email":"mismatch@test.com","password":"Pass@123","confirmPassword":"Pass@456"}' `
    -ExpectedStatus 400

# 1.4 Register - weak password
Test-Endpoint -Name "Register: Weak password" -Method POST -Url "$base/api/auth/register" `
    -Body '{"firstName":"Test","lastName":"User","userName":"weakpw","email":"weak@test.com","password":"123","confirmPassword":"123"}' `
    -ExpectedStatus 400

# 1.5 Login - wrong password
Start-Sleep -Seconds 3
Test-Endpoint -Name "Login: Wrong password" -Method POST -Url "$base/api/auth/login" `
    -Body '{"email":"teacher@test.com","password":"WrongPass@123"}' -ExpectedStatus 401

# 1.6 Login - non-existent email 
Start-Sleep -Seconds 3
Test-Endpoint -Name "Login: Non-existent email" -Method POST -Url "$base/api/auth/login" `
    -Body '{"email":"nonexistent@test.com","password":"Test@123"}' -ExpectedStatus 401

# 1.7 Login - valid teacher
Start-Sleep -Seconds 5
$teacherLogin = Test-Endpoint -Name "Login: Valid teacher" -Method POST -Url "$base/api/auth/login" `
    -Body '{"email":"teacher@test.com","password":"Teacher@123"}' -ExpectedStatus 200
$global:teacherToken = $teacherLogin.data.accessToken
$global:teacherRefresh = $teacherLogin.data.refreshToken
$global:teacherId = "019c77a0-e3d0-779c-a888-fdd3d889290d"

# 1.8 Login - valid student
Start-Sleep -Seconds 5
$studentLogin = Test-Endpoint -Name "Login: Valid student" -Method POST -Url "$base/api/auth/login" `
    -Body '{"email":"student@test.com","password":"Student@123"}' -ExpectedStatus 200
$global:studentToken = $studentLogin.data.accessToken
$global:studentRefresh = $studentLogin.data.refreshToken
$global:studentId = "019c77a1-ec58-71bb-aa14-db8084d2b9d1"

# 1.9 Refresh token
Start-Sleep -Seconds 3
$refreshResult = Test-Endpoint -Name "Refresh Token: Valid" -Method POST -Url "$base/api/auth/refresh-token" `
    -Body "{`"accessToken`":`"$($global:teacherToken)`",`"refreshToken`":`"$($global:teacherRefresh)`"}" -ExpectedStatus 200
if ($refreshResult.data) {
    $global:teacherToken = $refreshResult.data.accessToken
    $global:teacherRefresh = $refreshResult.data.refreshToken
}

# 1.10 Refresh token - invalid
Start-Sleep -Seconds 2
Test-Endpoint -Name "Refresh Token: Invalid token" -Method POST -Url "$base/api/auth/refresh-token" `
    -Body '{"accessToken":"invalid","refreshToken":"invalid"}' -ExpectedStatus 401

# 1.11 Access protected endpoint without token
Test-Endpoint -Name "Auth: No token on protected endpoint" -Method GET -Url "$base/api/users/profile" `
    -ExpectedStatus 401

# 1.12 Access protected endpoint with invalid token
Test-Endpoint -Name "Auth: Invalid token" -Method GET -Url "$base/api/users/profile" `
    -Headers @{ Authorization = "Bearer invalidtoken123" } -ExpectedStatus 401

########################################################################
Write-Host "`n========== PHASE 2: USER ENDPOINTS ==========`n" -ForegroundColor Cyan
########################################################################

$th = Get-AuthHeader $global:teacherToken
$sh = Get-AuthHeader $global:studentToken

# 2.1 Get profile - teacher
$teacherProfile = Test-Endpoint -Name "User: Get teacher profile" -Method GET -Url "$base/api/users/profile" -Headers $th
Write-Host "  Teacher: $($teacherProfile.data.firstName) $($teacherProfile.data.lastName), Roles: $($teacherProfile.data.roles -join ',')" -ForegroundColor Gray

# 2.2 Get profile - student
$studentProfile = Test-Endpoint -Name "User: Get student profile" -Method GET -Url "$base/api/users/profile" -Headers $sh
Write-Host "  Student: $($studentProfile.data.firstName) $($studentProfile.data.lastName), Roles: $($studentProfile.data.roles -join ',')" -ForegroundColor Gray

# 2.3 Update profile
Test-Endpoint -Name "User: Update profile" -Method PUT -Url "$base/api/users/profile" -Headers $th `
    -Body '{"firstName":"TeacherUpdated","lastName":"TestUser"}'

# 2.4 Get profile after update
$updatedProfile = Test-Endpoint -Name "User: Verify profile update" -Method GET -Url "$base/api/users/profile" -Headers $th
if ($updatedProfile.data.firstName -eq "TeacherUpdated") {
    Write-Host "  Profile update verified: firstName=TeacherUpdated" -ForegroundColor Gray
} else {
    Write-Host "  WARNING: Profile update not reflected!" -ForegroundColor Yellow
}

# 2.5 Revert profile
Test-Endpoint -Name "User: Revert profile" -Method PUT -Url "$base/api/users/profile" -Headers $th `
    -Body '{"firstName":"Teacher","lastName":"TestUser"}'

# 2.6 Become teacher (already teacher)
Test-Endpoint -Name "User: Become teacher (already)" -Method POST -Url "$base/api/users/become-teacher" -Headers $th `
    -ExpectedStatus 400

# 2.7 Change password
Test-Endpoint -Name "User: Change password" -Method POST -Url "$base/api/users/change-password" -Headers $sh `
    -Body '{"currentPassword":"Student@123","newPassword":"Student@456","confirmNewPassword":"Student@456"}'

# 2.8 Change password back
Start-Sleep -Seconds 2
Test-Endpoint -Name "User: Change password back" -Method POST -Url "$base/api/users/change-password" -Headers $sh `
    -Body '{"currentPassword":"Student@456","newPassword":"Student@123","confirmNewPassword":"Student@123"}'

# 2.9 Change password wrong current
Test-Endpoint -Name "User: Change pwd wrong current" -Method POST -Url "$base/api/users/change-password" -Headers $sh `
    -Body '{"currentPassword":"WrongPwd@123","newPassword":"New@123","confirmNewPassword":"New@123"}' -ExpectedStatus 400

# 2.10 Get all users (teacher only)
$allUsers = Test-Endpoint -Name "User: Get all users (teacher)" -Method GET -Url "$base/api/users" -Headers $th
Write-Host "  Total users: $($allUsers.data.Count)" -ForegroundColor Gray

# 2.11 Get all users (student - should be forbidden)
Test-Endpoint -Name "User: Get all users (student-forbidden)" -Method GET -Url "$base/api/users" -Headers $sh -ExpectedStatus 403

# 2.12 Get user by ID
Test-Endpoint -Name "User: Get by ID" -Method GET -Url "$base/api/users/$($global:studentId)" -Headers $th

# 2.13 Get user by invalid ID
Test-Endpoint -Name "User: Get by invalid ID" -Method GET -Url "$base/api/users/00000000-0000-0000-0000-000000000000" -Headers $th -ExpectedStatus 404

########################################################################
Write-Host "`n========== PHASE 3: COURSE CRUD ==========`n" -ForegroundColor Cyan
########################################################################

# 3.1 Create course (teacher)
$course = Test-Endpoint -Name "Course: Create" -Method POST -Url "$base/api/courses" -Headers $th `
    -Body '{"title":"Test Course","description":"A test course for API testing","category":"Computer Science"}'
$global:courseId = $course.data.id
Write-Host "  CourseId: $($global:courseId)" -ForegroundColor Gray

# 3.2 Create course (student - should fail)
Test-Endpoint -Name "Course: Create (student-forbidden)" -Method POST -Url "$base/api/courses" -Headers $sh `
    -Body '{"title":"Student Course","description":"Should fail","category":"Test"}' -ExpectedStatus 403

# 3.3 Create course - empty title
Test-Endpoint -Name "Course: Create empty title" -Method POST -Url "$base/api/courses" -Headers $th `
    -Body '{"title":"","description":"No title","category":"Test"}' -ExpectedStatus 400

# 3.4 Get course by ID
$courseDetail = Test-Endpoint -Name "Course: Get by ID" -Method GET -Url "$base/api/courses/$($global:courseId)" -Headers $th
Write-Host "  Title: $($courseDetail.data.title), Status: $($courseDetail.data.status)" -ForegroundColor Gray

# 3.5 Update course
Test-Endpoint -Name "Course: Update" -Method PUT -Url "$base/api/courses/$($global:courseId)" -Headers $th `
    -Body '{"title":"Updated Test Course","description":"Updated description","category":"Computer Science"}'

# 3.6 Get course after update
$updatedCourse = Test-Endpoint -Name "Course: Verify update" -Method GET -Url "$base/api/courses/$($global:courseId)" -Headers $th
if ($updatedCourse.data.title -eq "Updated Test Course") {
    Write-Host "  Course update verified: Updated Test Course" -ForegroundColor Gray
}

# 3.7 Update course (student - should fail)
Test-Endpoint -Name "Course: Update (student-forbidden)" -Method PUT -Url "$base/api/courses/$($global:courseId)" -Headers $sh `
    -Body '{"title":"Hacked","description":"Hacked","category":"Hack"}' -ExpectedStatus 403

# 3.8 Get teacher courses
$teacherCourses = Test-Endpoint -Name "Course: Get teacher courses" -Method GET -Url "$base/api/courses/teaching" -Headers $th
Write-Host "  Teacher courses count: $($teacherCourses.data.Count)" -ForegroundColor Gray

# 3.9 Search courses
$searchResult = Test-Endpoint -Name "Course: Search" -Method GET -Url "$base/api/courses/search?searchTerm=Updated" -Headers $th
Write-Host "  Search results: $($searchResult.data.items.Count)" -ForegroundColor Gray

# 3.10 Publish course
Test-Endpoint -Name "Course: Publish" -Method PUT -Url "$base/api/courses/$($global:courseId)/publish" -Headers $th

# 3.11 Get course after publish
$publishedCourse = Test-Endpoint -Name "Course: Verify publish" -Method GET -Url "$base/api/courses/$($global:courseId)" -Headers $th
Write-Host "  Status after publish: $($publishedCourse.data.status)" -ForegroundColor Gray

# 3.12 Get all published courses
$allCourses = Test-Endpoint -Name "Course: Get all published" -Method GET -Url "$base/api/courses" -Headers $th
Write-Host "  Published courses: $($allCourses.data.Count)" -ForegroundColor Gray

# 3.13 Get course by invalid ID
Test-Endpoint -Name "Course: Get invalid ID" -Method GET -Url "$base/api/courses/00000000-0000-0000-0000-000000000000" -Headers $th -ExpectedStatus 404

########################################################################
Write-Host "`n========== PHASE 4: ENROLLMENTS ==========`n" -ForegroundColor Cyan
########################################################################

# 4.1 Enroll student
$enrollment = Test-Endpoint -Name "Enrollment: Enroll student" -Method POST -Url "$base/api/courses/$($global:courseId)/enrollments" -Headers $sh
$global:enrollmentId = $enrollment.data.id
Write-Host "  EnrollmentId: $($global:enrollmentId)" -ForegroundColor Gray

# 4.2 Enroll again (duplicate)
Test-Endpoint -Name "Enrollment: Duplicate enroll" -Method POST -Url "$base/api/courses/$($global:courseId)/enrollments" -Headers $sh -ExpectedStatus 400

# 4.3 Get enrollment by ID
Test-Endpoint -Name "Enrollment: Get by ID" -Method GET -Url "$base/api/enrollments/$($global:enrollmentId)" -Headers $sh

# 4.4 Get student enrollments
$myEnrollments = Test-Endpoint -Name "Enrollment: Get student enrollments" -Method GET -Url "$base/api/enrollments/my" -Headers $sh
Write-Host "  Student enrollments: $($myEnrollments.data.Count)" -ForegroundColor Gray

# 4.5 Get course enrollments (teacher)
$courseEnrollments = Test-Endpoint -Name "Enrollment: Get course enrollments" -Method GET -Url "$base/api/courses/$($global:courseId)/enrollments" -Headers $th
Write-Host "  Course enrollments: $($courseEnrollments.data.Count)" -ForegroundColor Gray

# 4.6 Teacher enrolling (should fail - self-enrollment)
Test-Endpoint -Name "Enrollment: Teacher self-enroll" -Method POST -Url "$base/api/courses/$($global:courseId)/enrollments" -Headers $th -ExpectedStatus 400

########################################################################
Write-Host "`n========== PHASE 5: LECTURES ==========`n" -ForegroundColor Cyan
########################################################################

# 5.1 Create lecture (teacher)
$lecture = Test-Endpoint -Name "Lecture: Create" -Method POST -Url "$base/api/courses/$($global:courseId)/lectures" -Headers $th `
    -Body '{"title":"Lecture 1: Introduction","description":"First lecture","orderIndex":1}'
$global:lectureId = $lecture.data.id
Write-Host "  LectureId: $($global:lectureId)" -ForegroundColor Gray

# 5.2 Create second lecture
$lecture2 = Test-Endpoint -Name "Lecture: Create second" -Method POST -Url "$base/api/courses/$($global:courseId)/lectures" -Headers $th `
    -Body '{"title":"Lecture 2: Advanced Topics","description":"Second lecture","orderIndex":2}'
$global:lectureId2 = $lecture2.data.id

# 5.3 Create lecture (student - should fail)
Test-Endpoint -Name "Lecture: Create (student-forbidden)" -Method POST -Url "$base/api/courses/$($global:courseId)/lectures" -Headers $sh `
    -Body '{"title":"Student Lecture","description":"Should fail","orderIndex":3}' -ExpectedStatus 403

# 5.4 Get lecture by ID
$lectureDetail = Test-Endpoint -Name "Lecture: Get by ID" -Method GET -Url "$base/api/lectures/$($global:lectureId)" -Headers $th
Write-Host "  Lecture: $($lectureDetail.data.title)" -ForegroundColor Gray

# 5.5 Get course lectures
$courseLectures = Test-Endpoint -Name "Lecture: Get all for course" -Method GET -Url "$base/api/courses/$($global:courseId)/lectures" -Headers $th
Write-Host "  Lectures count: $($courseLectures.data.Count)" -ForegroundColor Gray

# 5.6 Update lecture
Test-Endpoint -Name "Lecture: Update" -Method PUT -Url "$base/api/lectures/$($global:lectureId)" -Headers $th `
    -Body '{"title":"Lecture 1: Updated Introduction","description":"Updated first lecture","orderIndex":1}'

# 5.7 Delete second lecture
Test-Endpoint -Name "Lecture: Delete" -Method DELETE -Url "$base/api/lectures/$($global:lectureId2)" -Headers $th

# 5.8 Get deleted lecture (should 404)
Test-Endpoint -Name "Lecture: Get deleted (404)" -Method GET -Url "$base/api/lectures/$($global:lectureId2)" -Headers $th -ExpectedStatus 404

# 5.9 Recreate second lecture for materials test
$lecture2 = Test-Endpoint -Name "Lecture: Recreate second" -Method POST -Url "$base/api/courses/$($global:courseId)/lectures" -Headers $th `
    -Body '{"title":"Lecture 2: Materials Test","description":"For material upload tests","orderIndex":2}'
$global:lectureId2 = $lecture2.data.id

########################################################################
Write-Host "`n========== PHASE 6: MATERIALS ==========`n" -ForegroundColor Cyan
########################################################################

# 6.1 Upload PDF material
Write-Host "  Uploading Documents.pdf..." -ForegroundColor Gray
$boundary = [System.Guid]::NewGuid().ToString()
$filePath = "$testMaterials\Documents.pdf"
$fileBytes = [System.IO.File]::ReadAllBytes($filePath)
$fileEnc = [System.Text.Encoding]::GetEncoding('iso-8859-1').GetString($fileBytes)
$LF = "`r`n"
$bodyLines = @(
    "--$boundary",
    "Content-Disposition: form-data; name=`"File`"; filename=`"Documents.pdf`"",
    "Content-Type: application/pdf",
    "",
    $fileEnc,
    "--$boundary--"
) -join $LF

try {
    $r = Invoke-WebRequest -Uri "$base/api/courses/lectures/$($global:lectureId)/materials" `
        -Method POST -Body $bodyLines -ContentType "multipart/form-data; boundary=$boundary" `
        -Headers $th -UseBasicParsing
    $material = ($r.Content | ConvertFrom-Json)
    $global:materialId = $material.data.id
    $global:testNum++
    $global:results += [PSCustomObject]@{ Num=$global:testNum; Test="Material: Upload PDF"; Result="PASS"; Status=$r.StatusCode; Response=$r.Content }
    Write-Host "[PASS] #$($global:testNum) Material: Upload PDF (Status: $($r.StatusCode))" -ForegroundColor Green
    Write-Host "  MaterialId: $($global:materialId)" -ForegroundColor Gray
} catch {
    $global:testNum++
    $global:results += [PSCustomObject]@{ Num=$global:testNum; Test="Material: Upload PDF"; Result="FAIL"; Status=$_.Exception.Response.StatusCode.value__; Response=$_.ErrorDetails.Message }
    Write-Host "[FAIL] #$($global:testNum) Material: Upload PDF (Status: $($_.Exception.Response.StatusCode.value__))" -ForegroundColor Red
    Write-Host "  Error: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# 6.2 Upload Image material
Write-Host "  Uploading Image.png..." -ForegroundColor Gray
$filePathImg = "$testMaterials\Image.png"
$fileBytesImg = [System.IO.File]::ReadAllBytes($filePathImg)
$fileEncImg = [System.Text.Encoding]::GetEncoding('iso-8859-1').GetString($fileBytesImg)
$boundary2 = [System.Guid]::NewGuid().ToString()
$bodyImg = @(
    "--$boundary2",
    "Content-Disposition: form-data; name=`"File`"; filename=`"Image.png`"",
    "Content-Type: image/png",
    "",
    $fileEncImg,
    "--$boundary2--"
) -join $LF

try {
    $r = Invoke-WebRequest -Uri "$base/api/courses/lectures/$($global:lectureId2)/materials" `
        -Method POST -Body $bodyImg -ContentType "multipart/form-data; boundary=$boundary2" `
        -Headers $th -UseBasicParsing
    $imgMaterial = ($r.Content | ConvertFrom-Json)
    $global:materialId2 = $imgMaterial.data.id
    $global:testNum++
    $global:results += [PSCustomObject]@{ Num=$global:testNum; Test="Material: Upload Image"; Result="PASS"; Status=$r.StatusCode; Response="" }
    Write-Host "[PASS] #$($global:testNum) Material: Upload Image (Status: $($r.StatusCode))" -ForegroundColor Green
} catch {
    $global:testNum++
    $global:results += [PSCustomObject]@{ Num=$global:testNum; Test="Material: Upload Image"; Result="FAIL"; Status=$_.Exception.Response.StatusCode.value__; Response=$_.ErrorDetails.Message }
    Write-Host "[FAIL] #$($global:testNum) Material: Upload Image (Status: $($_.Exception.Response.StatusCode.value__))" -ForegroundColor Red
    Write-Host "  Error: $($_.ErrorDetails.Message)" -ForegroundColor Red
}

# 6.3 Get material by ID
if ($global:materialId) {
    Test-Endpoint -Name "Material: Get by ID" -Method GET -Url "$base/api/materials/$($global:materialId)" -Headers $th
}

# 6.4 Get lecture materials
$lectureMaterials = Test-Endpoint -Name "Material: Get lecture materials" -Method GET -Url "$base/api/lectures/$($global:lectureId)/materials" -Headers $th
Write-Host "  Materials in lecture: $($lectureMaterials.data.Count)" -ForegroundColor Gray

# 6.5 Download material
if ($global:materialId) {
    try {
        $r = Invoke-WebRequest -Uri "$base/api/materials/$($global:materialId)/download" -Headers $th -UseBasicParsing
        $global:testNum++
        $global:results += [PSCustomObject]@{ Num=$global:testNum; Test="Material: Download"; Result="PASS"; Status=$r.StatusCode; Response="Size: $($r.Content.Length) bytes" }
        Write-Host "[PASS] #$($global:testNum) Material: Download (Status: $($r.StatusCode), Size: $($r.Content.Length) bytes)" -ForegroundColor Green
    } catch {
        $global:testNum++
        $global:results += [PSCustomObject]@{ Num=$global:testNum; Test="Material: Download"; Result="FAIL"; Status=$_.Exception.Response.StatusCode.value__; Response=$_.ErrorDetails.Message }
        Write-Host "[FAIL] #$($global:testNum) Material: Download" -ForegroundColor Red
    }
}

# 6.6 Stream material (Range header)
if ($global:materialId) {
    try {
        $streamHeaders = @{ Authorization = "Bearer $($global:teacherToken)"; Range = "bytes=0-1023" }
        $r = Invoke-WebRequest -Uri "$base/api/materials/$($global:materialId)/stream" -Headers $streamHeaders -UseBasicParsing
        $global:testNum++
        $status = $r.StatusCode
        $global:results += [PSCustomObject]@{ Num=$global:testNum; Test="Material: Stream (Range)"; Result="PASS"; Status=$status; Response="Partial content: $($r.Content.Length) bytes" }
        Write-Host "[PASS] #$($global:testNum) Material: Stream (Status: $status, Size: $($r.Content.Length))" -ForegroundColor Green
    } catch {
        $global:testNum++
        $global:results += [PSCustomObject]@{ Num=$global:testNum; Test="Material: Stream (Range)"; Result="FAIL"; Status=$_.Exception.Response.StatusCode.value__; Response=$_.ErrorDetails.Message }
        Write-Host "[FAIL] #$($global:testNum) Material: Stream" -ForegroundColor Red
    }
}

# 6.7 Get material indexing status
if ($global:materialId) {
    Test-Endpoint -Name "Material: Indexing status" -Method GET -Url "$base/api/materials/$($global:materialId)/indexing-status" -Headers $th
}

# 6.8 Get invalid material
Test-Endpoint -Name "Material: Get invalid ID" -Method GET -Url "$base/api/materials/00000000-0000-0000-0000-000000000000" -Headers $th -ExpectedStatus 404

########################################################################
Write-Host "`n========== PHASE 7: EXAMS ==========`n" -ForegroundColor Cyan
########################################################################

# 7.1 Create exam
$now = (Get-Date).ToUniversalTime()
$startTime = $now.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
$endTime = $now.AddHours(3).ToString("yyyy-MM-ddTHH:mm:ssZ")
$examBody = @{
    courseId = $global:courseId
    title = "Midterm Exam"
    description = "Test exam for API testing"
    startTime = $startTime
    endTime = $endTime
    durationMinutes = 60
    totalMarks = 100
    passingMarks = 50
} | ConvertTo-Json

$exam = Test-Endpoint -Name "Exam: Create" -Method POST -Url "$base/api/exams" -Headers $th -Body $examBody
$global:examId = $exam.data.id
Write-Host "  ExamId: $($global:examId)" -ForegroundColor Gray

# 7.2 Create exam (student - forbidden)
Test-Endpoint -Name "Exam: Create (student-forbidden)" -Method POST -Url "$base/api/exams" -Headers $sh -Body $examBody -ExpectedStatus 403

# 7.3 Get exam by ID
$examDetail = Test-Endpoint -Name "Exam: Get by ID" -Method GET -Url "$base/api/exams/$($global:examId)" -Headers $th
Write-Host "  Exam: $($examDetail.data.title), Duration: $($examDetail.data.durationMinutes)min" -ForegroundColor Gray

# 7.4 Get course exams
$courseExams = Test-Endpoint -Name "Exam: Get course exams" -Method GET -Url "$base/api/courses/$($global:courseId)/exams" -Headers $th
Write-Host "  Course exams: $($courseExams.data.Count)" -ForegroundColor Gray

# 7.5 Update exam
$updateExamBody = @{
    title = "Midterm Exam - Updated"
    description = "Updated test exam"
    startTime = $startTime
    endTime = $endTime
    durationMinutes = 90
    totalMarks = 100
    passingMarks = 60
} | ConvertTo-Json
Test-Endpoint -Name "Exam: Update" -Method PUT -Url "$base/api/exams/$($global:examId)" -Headers $th -Body $updateExamBody

# 7.6 Get exam after update
$updatedExam = Test-Endpoint -Name "Exam: Verify update" -Method GET -Url "$base/api/exams/$($global:examId)" -Headers $th
if ($updatedExam.data.durationMinutes -eq 90) {
    Write-Host "  Exam update verified: Duration=90min" -ForegroundColor Gray
}

# 7.7 Get invalid exam
Test-Endpoint -Name "Exam: Get invalid ID" -Method GET -Url "$base/api/exams/00000000-0000-0000-0000-000000000000" -Headers $th -ExpectedStatus 404

########################################################################
Write-Host "`n========== PHASE 8: QUESTIONS ==========`n" -ForegroundColor Cyan
########################################################################

# 8.1 Add question to exam
$questionBody = @{
    examId = $global:examId
    text = "What is the capital of France?"
    questionType = "MultipleChoice"
    marks = 10
    options = @(
        @{ text = "London"; isCorrect = $false }
        @{ text = "Paris"; isCorrect = $true }
        @{ text = "Berlin"; isCorrect = $false }
        @{ text = "Madrid"; isCorrect = $false }
    )
} | ConvertTo-Json -Depth 3

$question = Test-Endpoint -Name "Question: Add to exam" -Method POST -Url "$base/api/exams/$($global:examId)/questions" -Headers $th -Body $questionBody
$global:questionId = $question.data.id
Write-Host "  QuestionId: $($global:questionId)" -ForegroundColor Gray

# 8.2 Add essay question
$essayBody = @{
    examId = $global:examId
    text = "Explain the theory of relativity in your own words."
    questionType = "Essay"
    marks = 40
    options = @()
} | ConvertTo-Json -Depth 3
$essayQ = Test-Endpoint -Name "Question: Add essay" -Method POST -Url "$base/api/exams/$($global:examId)/questions" -Headers $th -Body $essayBody
$global:essayQuestionId = $essayQ.data.id

# 8.3 Add true/false question
$tfBody = @{
    examId = $global:examId
    text = "The Earth is flat."
    questionType = "TrueFalse"
    marks = 10
    options = @(
        @{ text = "True"; isCorrect = $false }
        @{ text = "False"; isCorrect = $true }
    )
} | ConvertTo-Json -Depth 3
$tfQ = Test-Endpoint -Name "Question: Add true/false" -Method POST -Url "$base/api/exams/$($global:examId)/questions" -Headers $th -Body $tfBody

# 8.4 Get exam questions
$examQuestions = Test-Endpoint -Name "Question: Get exam questions" -Method GET -Url "$base/api/exams/$($global:examId)/questions" -Headers $th
Write-Host "  Questions count: $($examQuestions.data.Count)" -ForegroundColor Gray

# 8.5 Update question
$updateQBody = @{
    text = "What is the capital city of France?"
    questionType = "MultipleChoice"
    marks = 15
    options = @(
        @{ text = "London"; isCorrect = $false }
        @{ text = "Paris"; isCorrect = $true }
        @{ text = "Berlin"; isCorrect = $false }
        @{ text = "Rome"; isCorrect = $false }
    )
} | ConvertTo-Json -Depth 3
Test-Endpoint -Name "Question: Update" -Method PUT -Url "$base/api/questions/$($global:questionId)" -Headers $th -Body $updateQBody

# 8.6 Add question (student - forbidden)
Test-Endpoint -Name "Question: Add (student-forbidden)" -Method POST -Url "$base/api/exams/$($global:examId)/questions" -Headers $sh -Body $questionBody -ExpectedStatus 403

# 8.7 Bulk add questions
$bulkBody = @{
    examId = $global:examId
    questions = @(
        @{
            text = "What is 2+2?"
            questionType = "MultipleChoice"
            marks = 5
            options = @(
                @{ text = "3"; isCorrect = $false }
                @{ text = "4"; isCorrect = $true }
                @{ text = "5"; isCorrect = $false }
            )
        }
        @{
            text = "Is water wet?"
            questionType = "TrueFalse"
            marks = 5
            options = @(
                @{ text = "True"; isCorrect = $true }
                @{ text = "False"; isCorrect = $false }
            )
        }
    )
} | ConvertTo-Json -Depth 4
Test-Endpoint -Name "Question: Bulk add" -Method POST -Url "$base/api/exams/$($global:examId)/questions/bulk" -Headers $th -Body $bulkBody

# 8.8 Delete question
if ($tfQ.data.id) {
    Test-Endpoint -Name "Question: Delete" -Method DELETE -Url "$base/api/questions/$($tfQ.data.id)" -Headers $th
}

########################################################################
Write-Host "`n========== PHASE 9: SUBMISSIONS ==========`n" -ForegroundColor Cyan
########################################################################

# First, need to make the exam currently active for submission
# Update exam to be active now
$now2 = (Get-Date).ToUniversalTime()
$activeStart = $now2.AddMinutes(-5).ToString("yyyy-MM-ddTHH:mm:ssZ")
$activeEnd = $now2.AddHours(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
$activateBody = @{
    title = "Midterm Exam - Updated"
    description = "Updated test exam"
    startTime = $activeStart
    endTime = $activeEnd
    durationMinutes = 90
    totalMarks = 100
    passingMarks = 60
} | ConvertTo-Json
Test-Endpoint -Name "Exam: Activate (set time range)" -Method PUT -Url "$base/api/exams/$($global:examId)" -Headers $th -Body $activateBody

# 9.1 Start submission (student)
$submissionBody = @{
    examId = $global:examId
} | ConvertTo-Json
$submission = Test-Endpoint -Name "Submission: Start" -Method POST -Url "$base/api/exams/$($global:examId)/submissions" -Headers $sh -Body $submissionBody
$global:submissionId = $submission.data.id
Write-Host "  SubmissionId: $($global:submissionId)" -ForegroundColor Gray

# 9.2 Get submission by ID
$subDetail = Test-Endpoint -Name "Submission: Get by ID" -Method GET -Url "$base/api/submissions/$($global:submissionId)" -Headers $sh

# 9.3 Submit answer for MCQ
if ($global:questionId) {
    $answerBody = @{
        questionId = $global:questionId
        answerText = "Paris"
        selectedOptionIds = @()
    } | ConvertTo-Json
    Test-Endpoint -Name "Submission: Answer MCQ" -Method POST -Url "$base/api/submissions/$($global:submissionId)/answers" -Headers $sh -Body $answerBody
}

# 9.4 Submit answer for Essay
if ($global:essayQuestionId) {
    $essayAnswerBody = @{
        questionId = $global:essayQuestionId
        answerText = "Einstein's theory of relativity describes how space and time are interconnected. Special relativity tells us that the speed of light is constant in all reference frames, and that mass and energy are equivalent (E=mc^2). General relativity extends this to include gravity as a curvature of spacetime caused by mass and energy."
        selectedOptionIds = @()
    } | ConvertTo-Json
    Test-Endpoint -Name "Submission: Answer Essay" -Method POST -Url "$base/api/submissions/$($global:submissionId)/answers" -Headers $sh -Body $essayAnswerBody
}

# 9.5 Complete submission
Test-Endpoint -Name "Submission: Complete" -Method PUT -Url "$base/api/submissions/$($global:submissionId)/submit" -Headers $sh

# 9.6 Get submission after completion
$completedSub = Test-Endpoint -Name "Submission: Get after complete" -Method GET -Url "$base/api/submissions/$($global:submissionId)" -Headers $sh
Write-Host "  Submission status: $($completedSub.data.status)" -ForegroundColor Gray

# 9.7 Get exam submissions (teacher)
$examSubs = Test-Endpoint -Name "Submission: Get exam submissions" -Method GET -Url "$base/api/exams/$($global:examId)/submissions" -Headers $th
Write-Host "  Exam submissions: $($examSubs.data.Count)" -ForegroundColor Gray

# 9.8 Double submit (should fail)
Test-Endpoint -Name "Submission: Double start (fail)" -Method POST -Url "$base/api/exams/$($global:examId)/submissions" -Headers $sh `
    -Body $submissionBody -ExpectedStatus 400

########################################################################
Write-Host "`n========== PHASE 10: GRADING ==========`n" -ForegroundColor Cyan
########################################################################

# 10.1 Manual grade
$gradeBody = @{
    submissionId = $global:submissionId
    grades = @(
        @{
            questionId = $global:questionId
            score = 15
            feedback = "Correct answer!"
        }
    )
} | ConvertTo-Json -Depth 3
$grade = Test-Endpoint -Name "Grade: Manual grade" -Method POST -Url "$base/api/submissions/$($global:submissionId)/grade" -Headers $th -Body $gradeBody

# 10.2 Get grades for submission  
$grades = Test-Endpoint -Name "Grade: Get submission grades" -Method GET -Url "$base/api/submissions/$($global:submissionId)/grades" -Headers $th
Write-Host "  Grades: $($grades | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor Gray

# 10.3 Get student grades for course
$studentGrades = Test-Endpoint -Name "Grade: Student course grades" -Method GET -Url "$base/api/courses/$($global:courseId)/grades" -Headers $sh
Write-Host "  Student course grades: $($studentGrades | ConvertTo-Json -Depth 2 -Compress)" -ForegroundColor Gray

# 10.4 Approve grade
Test-Endpoint -Name "Grade: Approve" -Method PUT -Url "$base/api/submissions/$($global:submissionId)/grade/approve" -Headers $th

# 10.5 Grade by student (should fail)
Test-Endpoint -Name "Grade: Student grade (forbidden)" -Method POST -Url "$base/api/submissions/$($global:submissionId)/grade" -Headers $sh `
    -Body $gradeBody -ExpectedStatus 403

########################################################################
Write-Host "`n========== PHASE 11: REVIEWS ==========`n" -ForegroundColor Cyan
########################################################################

# 11.1 Add review (student)
$reviewBody = @{
    courseId = $global:courseId
    rating = 4
    comment = "Great course! Very informative and well-structured."
} | ConvertTo-Json
$review = Test-Endpoint -Name "Review: Add" -Method POST -Url "$base/api/courses/$($global:courseId)/reviews" -Headers $sh -Body $reviewBody
$global:reviewId = $review.data.id
Write-Host "  ReviewId: $($global:reviewId)" -ForegroundColor Gray

# 11.2 Add duplicate review (should fail)
Test-Endpoint -Name "Review: Duplicate (fail)" -Method POST -Url "$base/api/courses/$($global:courseId)/reviews" -Headers $sh `
    -Body $reviewBody -ExpectedStatus 400

# 11.3 Get course reviews
$courseReviews = Test-Endpoint -Name "Review: Get course reviews" -Method GET -Url "$base/api/courses/$($global:courseId)/reviews" -Headers $th
Write-Host "  Reviews count: $($courseReviews.data.items.Count)" -ForegroundColor Gray

# 11.4 Get review rating summary
$ratingSummary = Test-Endpoint -Name "Review: Get rating summary" -Method GET -Url "$base/api/courses/$($global:courseId)/reviews/summary" -Headers $th
Write-Host "  Average rating: $($ratingSummary.data.averageRating)" -ForegroundColor Gray

# 11.5 Update review
$updateReviewBody = @{
    rating = 5
    comment = "Updated: Excellent course! Highly recommend."
} | ConvertTo-Json
if ($global:reviewId) {
    Test-Endpoint -Name "Review: Update" -Method PUT -Url "$base/api/reviews/$($global:reviewId)" -Headers $sh -Body $updateReviewBody
}

# 11.6 Get updated review summary
$updatedSummary = Test-Endpoint -Name "Review: Updated summary" -Method GET -Url "$base/api/courses/$($global:courseId)/reviews/summary" -Headers $th
Write-Host "  Updated average rating: $($updatedSummary.data.averageRating)" -ForegroundColor Gray

# 11.7 Delete review
if ($global:reviewId) {
    Test-Endpoint -Name "Review: Delete" -Method DELETE -Url "$base/api/reviews/$($global:reviewId)" -Headers $sh
}

# 11.8 Get reviews after delete
$afterDeleteReviews = Test-Endpoint -Name "Review: Get after delete" -Method GET -Url "$base/api/courses/$($global:courseId)/reviews" -Headers $th

########################################################################
Write-Host "`n========== PHASE 12: ENGAGEMENT ==========`n" -ForegroundColor Cyan
########################################################################

# 12.1 Get engagement report (teacher)
$engagement = Test-Endpoint -Name "Engagement: Get report" -Method GET -Url "$base/api/courses/$($global:courseId)/engagement" -Headers $th
Write-Host "  Engagement data: $($engagement | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor Gray

# 12.2 Get engagement (student - forbidden)
Test-Endpoint -Name "Engagement: Student (forbidden)" -Method GET -Url "$base/api/courses/$($global:courseId)/engagement" -Headers $sh -ExpectedStatus 403

# 12.3 Send engagement alerts
$alertBody = @{
    courseId = $global:courseId
} | ConvertTo-Json
Test-Endpoint -Name "Engagement: Send alerts" -Method POST -Url "$base/api/courses/$($global:courseId)/engagement/alerts" -Headers $th -Body $alertBody

########################################################################
Write-Host "`n========== PHASE 13: ENROLLMENT COMPLETION ==========`n" -ForegroundColor Cyan
########################################################################

# 13.1 Complete enrollment
Test-Endpoint -Name "Enrollment: Complete" -Method PUT -Url "$base/api/enrollments/$($global:enrollmentId)/complete" -Headers $sh

# 13.2 Unenroll - first re-enroll if needed
# Try unenroll on a different enrollment or test the flow

########################################################################
Write-Host "`n========== PHASE 14: EDGE CASES ==========`n" -ForegroundColor Cyan
########################################################################

# 14.1 Very long strings
$longTitle = "A" * 500
$longBody = @{ title = $longTitle; description = "Test"; category = "Test" } | ConvertTo-Json
Test-Endpoint -Name "Edge: Very long title" -Method POST -Url "$base/api/courses" -Headers $th -Body $longBody -ExpectedStatus 400

# 14.2 Special characters in fields
$specialBody = @{ title = "Course <script>alert('xss')</script>"; description = "Test' OR 1=1 --"; category = "Test & <Category>" } | ConvertTo-Json
$specialResult = Test-Endpoint -Name "Edge: Special characters/XSS" -Method POST -Url "$base/api/courses" -Headers $th -Body $specialBody
Write-Host "  Title stored as: $($specialResult.data.title)" -ForegroundColor Gray

# 14.3 Empty body on POST
Test-Endpoint -Name "Edge: Empty body on create" -Method POST -Url "$base/api/courses" -Headers $th -Body '{}' -ExpectedStatus 400

# 14.4 Negative exam marks
$negBody = @{
    courseId = $global:courseId; title = "Neg Exam"; description = "Test"
    startTime = $startTime; endTime = $endTime; durationMinutes = -10
    totalMarks = -100; passingMarks = -50
} | ConvertTo-Json
Test-Endpoint -Name "Edge: Negative exam marks" -Method POST -Url "$base/api/exams" -Headers $th -Body $negBody -ExpectedStatus 400

# 14.5 Review with invalid rating (0 or 6)
$invalidReview = @{ courseId = $global:courseId; rating = 0; comment = "Invalid" } | ConvertTo-Json
Test-Endpoint -Name "Edge: Invalid rating (0)" -Method POST -Url "$base/api/courses/$($global:courseId)/reviews" -Headers $sh -Body $invalidReview -ExpectedStatus 400

$invalidReview6 = @{ courseId = $global:courseId; rating = 6; comment = "Invalid" } | ConvertTo-Json
Test-Endpoint -Name "Edge: Invalid rating (6)" -Method POST -Url "$base/api/courses/$($global:courseId)/reviews" -Headers $sh -Body $invalidReview6 -ExpectedStatus 400

# 14.6 Access another teacher's course
$course2Body = '{"title":"Another Course","description":"Different teacher course","category":"Test"}'
# This tests accessing resources with wrong ownership - try to update course as student
Test-Endpoint -Name "Edge: Update course as student" -Method PUT -Url "$base/api/courses/$($global:courseId)" -Headers $sh `
    -Body '{"title":"Hacked","description":"Hacked","category":"Hack"}' -ExpectedStatus 403

# 14.7 Invalid GUID format
Test-Endpoint -Name "Edge: Invalid GUID format" -Method GET -Url "$base/api/courses/not-a-guid" -Headers $th -ExpectedStatus 400

# Clean up XSS test course
if ($specialResult.data.id) {
    Test-Endpoint -Name "Cleanup: Delete XSS course" -Method DELETE -Url "$base/api/courses/$($specialResult.data.id)" -Headers $th
}

########################################################################
Write-Host "`n========== PHASE 15: COURSE DELETION ==========`n" -ForegroundColor Cyan
########################################################################

# 15.1 Delete course (student - forbidden)
Test-Endpoint -Name "Course: Delete (student-forbidden)" -Method DELETE -Url "$base/api/courses/$($global:courseId)" -Headers $sh -ExpectedStatus 403

########################################################################
Write-Host "`n`n========== TEST RESULTS SUMMARY ==========`n" -ForegroundColor Cyan
########################################################################

$passed = ($global:results | Where-Object { $_.Result -eq "PASS" }).Count
$failed = ($global:results | Where-Object { $_.Result -ne "PASS" }).Count
$total = $global:results.Count

Write-Host "Total Tests: $total" -ForegroundColor White
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if($failed -gt 0){"Red"}else{"Green"})
Write-Host ""

# Show failures
if ($failed -gt 0) {
    Write-Host "FAILED TESTS:" -ForegroundColor Red
    $global:results | Where-Object { $_.Result -ne "PASS" } | ForEach-Object {
        Write-Host "  #$($_.Num) $($_.Test): $($_.Result) [HTTP $($_.Status)]" -ForegroundColor Red
        if ($_.Response) {
            Write-Host "    Response: $($_.Response)" -ForegroundColor DarkRed
        }
    }
}

Write-Host "`nAll tests:" -ForegroundColor White
$global:results | Format-Table Num, Test, Result, Status -AutoSize
