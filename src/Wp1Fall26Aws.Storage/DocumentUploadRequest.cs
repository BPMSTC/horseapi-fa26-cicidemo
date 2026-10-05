namespace Wp1Fall26Aws.Storage;

public sealed record DocumentUploadRequest(
    string FileName,
    string? ContentType,
    long Length,
    Stream Content);