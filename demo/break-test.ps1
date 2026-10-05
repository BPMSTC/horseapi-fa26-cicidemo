# Breaks ONE existing test on purpose: changes the expected error text from "10 bytes" to "99 bytes".
# The app itself is untouched, so nothing about the running app changes. Only the test now fails.
$f = "tests/Wp1Fall26Aws.Tests/DocumentValidatorTests.cs"
$t = [IO.File]::ReadAllText($f)
if (-not $t.Contains('10 bytes or smaller')) { Write-Host "Test is already broken (or the text changed)."; exit 1 }
[IO.File]::WriteAllText($f, $t.Replace('10 bytes or smaller', '99 bytes or smaller'), (New-Object Text.UTF8Encoding $false))
Write-Host "Broke Validate_RejectsOversizedDocument. Run 'dotnet test' to see it fail."
