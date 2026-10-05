namespace Wp1Fall26Aws.Storage;

public sealed class S3StorageOptions
{
    public const string SectionName = "S3Storage";

    public string BucketName { get; set; } = string.Empty;

    public string KeyPrefix { get; set; } = "documents/uploads";

    public string Region { get; set; } = string.Empty;

    public long MaxFileSizeBytes { get; set; } = 1_048_576;

    public string[] AllowedExtensions { get; set; } = [".txt", ".pdf", ".png", ".jpg", ".jpeg"];

    public string[] AllowedContentTypes { get; set; } = ["text/plain", "application/pdf", "image/png", "image/jpeg"];
}