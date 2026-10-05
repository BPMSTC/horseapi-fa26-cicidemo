using Amazon.S3.Model;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

// A spy for the S3 boundary: no network, canned response, and it records the last request.
// (Named differently from the private RecordingS3ObjectClient inside S3DocumentStorageTests to avoid confusion.)
public sealed class RecordingS3ObjectClientSpy : IS3ObjectClient
{
    // The request our app tried to send to S3, or null if nothing was sent.
    public PutObjectRequest? LastRequest { get; private set; }

    public Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;                                                       // remember what was "uploaded"
        return Task.FromResult(new PutObjectResponse { ETag = "\"integration-test-etag\"" }); // pretend S3 succeeded
    }
}
