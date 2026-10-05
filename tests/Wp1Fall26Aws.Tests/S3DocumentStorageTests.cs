using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

public sealed class S3DocumentStorageTests
{
    [Fact]
    public async Task UploadAsync_WritesS3ObjectWithKeyAndMetadata()
    {
        var s3Client = new RecordingS3ObjectClient();
        var options = Options.Create(new S3StorageOptions
        {
            BucketName = "redacted-lab-bucket",
            KeyPrefix = "student-prefix/documents"
        });
        var storage = new S3DocumentStorage(s3Client, options, new DocumentValidator());
        await using var content = new MemoryStream("synthetic lab file"u8.ToArray());

        var result = await storage.UploadAsync(new DocumentUploadRequest("synthetic-document.txt", "text/plain", content.Length, content));

        Assert.Equal("redacted-lab-bucket", result.BucketName);
        Assert.StartsWith("student-prefix/documents/", result.ObjectKey);
        Assert.EndsWith(".txt", result.ObjectKey);
        Assert.Equal("text/plain", result.ContentType);
        Assert.Equal(content.Length, result.Length);
        Assert.Equal("synthetic-document.txt", result.Metadata["original-file-name"]);
        Assert.Equal(result.ObjectKey, s3Client.Request?.Key);
        Assert.Equal("redacted-lab-bucket", s3Client.Request?.BucketName);
        Assert.Equal("text/plain", s3Client.Request?.ContentType);
        Assert.Equal("synthetic-document.txt", s3Client.Request?.Metadata["original-file-name"]);
    }

    [Fact]
    public async Task UploadAsync_RequiresConfiguredBucketName()
    {
        var storage = new S3DocumentStorage(new RecordingS3ObjectClient(), Options.Create(new S3StorageOptions()), new DocumentValidator());

        await using var content = new MemoryStream("synthetic lab file"u8.ToArray());

        await Assert.ThrowsAsync<StorageConfigurationException>(() => storage.UploadAsync(new DocumentUploadRequest("synthetic-document.txt", "text/plain", content.Length, content)));
    }

    private sealed class RecordingS3ObjectClient : IS3ObjectClient
    {
        public PutObjectRequest? Request { get; private set; }

        public Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
        {
            Request = request;
            return Task.FromResult(new PutObjectResponse { ETag = "\"test-etag\"" });
        }
    }
}