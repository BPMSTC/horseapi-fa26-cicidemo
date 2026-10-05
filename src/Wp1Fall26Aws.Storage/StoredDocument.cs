namespace Wp1Fall26Aws.Storage;

public sealed record StoredDocument(
    string BucketName,
    string ObjectKey,
    long Length,
    string ContentType,
    string? ETag,
    IReadOnlyDictionary<string, string> Metadata);