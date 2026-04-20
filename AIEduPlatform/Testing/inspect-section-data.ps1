$ErrorActionPreference = 'Stop'

$dll = "$env:USERPROFILE\.nuget\packages\npgsql\10.0.0\lib\net8.0\Npgsql.dll"
Add-Type -Path $dll

$connections = @(
  "Host=127.0.0.1;Port=5432;Database=AIEduPlatformDb;Username=postgres;Password=Drmando70",
  "Host=127.0.0.1;Port=5433;Database=AIEduPlatformDb;Username=postgres;Password=Ziad#2251"
)

$sectionPhrase = "Asymmetric Cryptographic Algorithms and Digital Signatures"
$opened = $null

foreach ($cs in $connections) {
  try {
    $conn = [Npgsql.NpgsqlConnection]::new($cs)
    $conn.Open()
    Write-Host "Connected: $cs" -ForegroundColor Green
    $opened = $conn
    break
  }
  catch {
    Write-Host "Failed: $cs" -ForegroundColor Yellow
  }
}

if (-not $opened) {
  throw "No database connection succeeded."
}

$cmd = $opened.CreateCommand()
$cmd.CommandText = @"
SELECT s.""Id"", s.""MaterialId"", s.""Title"", s.""StartPage"", s.""EndPage"", s.""StartSeconds"", s.""EndSeconds"", s.""OrderIndex"", m.""Title"" AS ""MaterialTitle""
FROM ""SemanticSections"" s
JOIN ""Materials"" m ON m.""Id"" = s.""MaterialId""
WHERE s.""Title"" ILIKE @phrase
ORDER BY s.""OrderIndex"";
"@
$null = $cmd.Parameters.AddWithValue("phrase", "%$sectionPhrase%")

$r = $cmd.ExecuteReader()
$sections = @()
while ($r.Read()) {
  $sections += [PSCustomObject]@{
    SectionId = $r["Id"].ToString()
    MaterialId = $r["MaterialId"].ToString()
    Title = $r["Title"].ToString()
    MaterialTitle = $r["MaterialTitle"].ToString()
    StartPage = if ($r.IsDBNull($r.GetOrdinal("StartPage"))) { $null } else { [int]$r["StartPage"] }
    EndPage = if ($r.IsDBNull($r.GetOrdinal("EndPage"))) { $null } else { [int]$r["EndPage"] }
    StartSeconds = if ($r.IsDBNull($r.GetOrdinal("StartSeconds"))) { $null } else { [int]$r["StartSeconds"] }
    EndSeconds = if ($r.IsDBNull($r.GetOrdinal("EndSeconds"))) { $null } else { [int]$r["EndSeconds"] }
    OrderIndex = [int]$r["OrderIndex"]
  }
}
$r.Close()

if (-not $sections -or $sections.Count -eq 0) {
  Write-Host "No matching semantic section found for phrase: $sectionPhrase" -ForegroundColor Red
  $opened.Close()
  exit 0
}

Write-Host "Matched sections:" -ForegroundColor Cyan
$sections | Format-Table -AutoSize

foreach ($s in $sections) {
  Write-Host "`n-- Chunk stats for SectionId=$($s.SectionId) MaterialId=$($s.MaterialId) --" -ForegroundColor Cyan

  $statsCmd = $opened.CreateCommand()
  $statsCmd.CommandText = @"
SELECT
  COUNT(*) AS total_chunks,
  SUM(CASE WHEN ""Section"" ILIKE @titleLike THEN 1 ELSE 0 END) AS title_section_matches,
  MIN(""PageOrTimestamp"") AS min_loc,
  MAX(""PageOrTimestamp"") AS max_loc
FROM ""Chunks""
WHERE ""MaterialId"" = @mid;
"@
  $null = $statsCmd.Parameters.AddWithValue("titleLike", "%$($s.Title)%")
  $null = $statsCmd.Parameters.AddWithValue("mid", [Guid]$s.MaterialId)

  $sr = $statsCmd.ExecuteReader()
  if ($sr.Read()) {
    Write-Host ("TotalChunks={0}, TitleSectionMatches={1}, MinLoc={2}, MaxLoc={3}" -f $sr["total_chunks"], $sr["title_section_matches"], $sr["min_loc"], $sr["max_loc"])
  }
  $sr.Close()

  $sampleCmd = $opened.CreateCommand()
  $sampleCmd.CommandText = @"
SELECT ""Section"", ""PageOrTimestamp"", LEFT(""Content"", 140) AS preview
FROM ""Chunks""
WHERE ""MaterialId"" = @mid
ORDER BY ""PageOrTimestamp""
LIMIT 12;
"@
  $null = $sampleCmd.Parameters.AddWithValue("mid", [Guid]$s.MaterialId)

  $reader = $sampleCmd.ExecuteReader()
  $rows = @()
  while ($reader.Read()) {
    $rows += [PSCustomObject]@{
      Section = if ($reader.IsDBNull($reader.GetOrdinal("Section"))) { "" } else { $reader["Section"].ToString() }
      PageOrTimestamp = if ($reader.IsDBNull($reader.GetOrdinal("PageOrTimestamp"))) { "" } else { $reader["PageOrTimestamp"].ToString() }
      Preview = $reader["preview"].ToString().Replace("`n", " ").Replace("`r", " ")
    }
  }
  $reader.Close()

  $rows | Format-Table -AutoSize
}

$opened.Close()
