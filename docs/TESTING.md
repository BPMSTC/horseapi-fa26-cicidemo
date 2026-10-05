# Testing

There are **8 automated tests**, written with xUnit. Running `dotnet test Wp1Fall26Aws.slnx` runs all of them, locally or in the pipeline.

## The test pyramid, applied here

Mike Cohn's test pyramid says to write many small, fast tests and fewer big ones. In this repo:

| Layer | Tests | What they run | Speed |
|---|---|---|---|
| Unit | 5 | One class at a time (`DocumentValidator`, `S3DocumentStorage`) | Milliseconds |
| Integration | 3 | The whole API running in memory, with real HTTP requests | A bit slower |
| End-to-end / smoke | 0 (not in this demo) | The deployed app over the network | Slowest. Week 8 |

## The 8 tests

### `DocumentValidatorTests` (unit, 3 tests)

| Test | What it proves |
|---|---|
| `Validate_AllowsTinySyntheticTextFile` | A small `.txt` file with a valid type produces no errors |
| `Validate_RejectsOversizedDocument` | A file over the size limit produces the size error. The stream passed in is a **dummy**: the validator never reads it |
| `Validate_RejectsUnsupportedFileType` | A `.sh` file gets both an extension error and a content-type error |

### `S3DocumentStorageTests` (unit, 2 tests)

| Test | What it proves |
|---|---|
| `UploadAsync_WritesS3ObjectWithKeyAndMetadata` | The storage class builds the right S3 request (bucket, key, content type, metadata). It uses a private **spy** called `RecordingS3ObjectClient` |
| `UploadAsync_RequiresConfiguredBucketName` | With no bucket configured, upload refuses with a configuration error |

### `DocumentApiTests` (integration, 3 tests)

| Test | What it proves |
|---|---|
| `GetHealth_AppRunning_ReturnsOk` | `GET /health` returns 200 OK through the real app |
| `PostDocuments_ValidTextFile_Returns201AndSendsToS3` | A multipart upload of a valid file returns 201 Created, and the request reached the S3 boundary with the right bucket and content type |
| `PostDocuments_ShellScript_Returns400AndNeverCallsS3` | An upload of a `.sh` file returns 400 Bad Request, and nothing reached S3 |

## Why none of the tests need AWS

The storage code depends on an interface, `IS3ObjectClient`, not on the AWS SDK directly. The app hands it the real `S3ObjectClient` in production. In tests we hand it a stand-in:

- In **unit tests**, `S3DocumentStorageTests` creates its own spy and passes it to the class.
- In **integration tests**, `WebApplicationFactory` boots the real app in memory, and `ConfigureTestServices` replaces `IS3ObjectClient` with `RecordingS3ObjectClientSpy`. "Last registration wins": when the app asks for an `IS3ObjectClient`, it gets the most recently registered one, which is the spy.

A **spy** is a stub that also records how it was called. Ours returns a canned response and remembers the last request, so a test can check what would have been uploaded.

The integration tests also set a fake bucket name, because `S3DocumentStorage` refuses to upload when `BucketName` is empty.

## The integration-test files

- `tests/RecordingS3ObjectClientSpy.cs`: the reusable spy.
- `tests/DocumentApiTests.cs`: the three tests. It uses `IClassFixture<WebApplicationFactory<Program>>`, so one shared factory is created for the class, and each upload test builds its own spy.
- `tests/Wp1Fall26Aws.Tests.csproj`: references the `Microsoft.AspNetCore.Mvc.Testing` package, which provides `WebApplicationFactory`. Its version must match the app's .NET version (10.x).

In .NET 10 the generated `Program` class is public automatically, so the `public partial class Program;` line at the bottom of `Program.cs` is no longer required. It is harmless, but a code analyzer (ASP0027) may point it out.

## Good-test habits you can point at

- **Arrange, Act, Assert**: set up, run one thing, check the result. Each test has those three comments.
- **Naming**: method, scenario, expected result, for example `PostDocuments_ShellScript_Returns400AndNeverCallsS3`.
- **One behavior per test**, no logic inside tests, no real infrastructure.
