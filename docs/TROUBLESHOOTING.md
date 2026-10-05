# Troubleshooting

Find your symptom, then follow the fix.

## On your computer

| Symptom | Likely cause | Fix |
|---|---|---|
| `dotnet: command not found` | .NET SDK not installed | Install the .NET 10 SDK, then open a **new** terminal. Check with `dotnet --version`. |
| `dotnet test` says 5 tests, not 8 | The new integration test files are missing | Check that `tests/Wp1Fall26Aws.Tests/DocumentApiTests.cs` and `RecordingS3ObjectClientSpy.cs` exist. |
| Build error: `WebApplicationFactory` not found | Missing package | Confirm the test `.csproj` has `Microsoft.AspNetCore.Mvc.Testing`, then run `dotnet restore`. |
| Build error: `Program` is inaccessible | Test project can't see the API's `Program` class | Make sure the test project has a ProjectReference to `Wp1Fall26Aws.Api`. In .NET 10 no `public partial class Program` is needed. |
| Build error: ASP.NET Core types such as `IServiceCollection` or `IWebHostBuilder` not found | Test project SDK too narrow | Change the first line of the test `.csproj` from `Microsoft.NET.Sdk` to `Microsoft.NET.Sdk.Web`. |
| Line-ending warnings (`LF will be replaced by CRLF`) | Windows and Git normalizing line endings | Harmless. `.gitattributes` handles it. |
| `git push` rejected | Remote has changes you don't | `git pull --rebase`, then push again. |

## On GitHub

| Symptom | Likely cause | Fix |
|---|---|---|
| No workflow runs appear | The file is not in `.github/workflows/`, or Actions is disabled | Check the path and name. In repo **Settings → Actions → General**, allow actions. |
| Workflow file shows a red error | YAML indentation | Open the run page: GitHub shows the line. YAML uses spaces, never tabs. |
| `build-test` fails at "Run unit and integration tests" | A test failed | Open the step log, find the line with `Failed`, and read the message under it. Reproduce locally with `dotnet test`. |
| `build-test` fails at "Set up .NET 10" | Version not found | Check `dotnet-version: 10.0.x` is spelled correctly. |
| `deploy` is **Skipped** on a pull request | Working as designed | Skipped is the point. It runs only on `main`. |
| `deploy` is skipped on `main` too | `build-test` failed, or the run was not on `main` | Fix the failure, or check the branch name in the run header. |
| `deploy` fails at the OIDC step: "Unable to get ACTIONS_ID_TOKEN_REQUEST_URL" | Job lacks `id-token: write` | Confirm the `permissions:` block inside the `deploy` job. |
| `deploy` fails at "Download artifact" | Artifact name mismatch | The name in `upload-artifact` and `download-artifact` must match (`api-publish`). |
| No Artifacts section on the run summary | Run failed before upload, or the artifact expired | Re-run the workflow. |

## Re-running

- **Re-run failed jobs:** on the run page choose **Re-run jobs**.
- **Run without a code change:** Actions tab, choose the workflow, **Run workflow**.

## Still stuck?

Copy the exact error text from the failing step and bring it to class. The first red line is usually the real problem; later lines are often side effects.
