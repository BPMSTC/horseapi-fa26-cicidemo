# Undoes break-test.ps1.
$f = "tests/Wp1Fall26Aws.Tests/DocumentValidatorTests.cs"
$t = [IO.File]::ReadAllText($f)
if (-not $t.Contains('99 bytes or smaller')) { Write-Host "Test is not broken."; exit 1 }
[IO.File]::WriteAllText($f, $t.Replace('99 bytes or smaller', '10 bytes or smaller'), (New-Object Text.UTF8Encoding $false))
Write-Host "Fixed Validate_RejectsOversizedDocument."
