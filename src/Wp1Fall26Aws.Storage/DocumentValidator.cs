namespace Wp1Fall26Aws.Storage;

public sealed class DocumentValidator
{
    public IReadOnlyList<string> Validate(DocumentUploadRequest request, S3StorageOptions options)
    {
        var errors = new List<string>();
        var safeFileName = Path.GetFileName(request.FileName);
        var extension = Path.GetExtension(safeFileName).ToLowerInvariant();
        var contentType = NormalizeContentType(request.ContentType);

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            errors.Add("A file name is required.");
        }

        if (request.Length <= 0)
        {
            errors.Add("The file must not be empty.");
        }

        if (request.Length > options.MaxFileSizeBytes)
        {
            errors.Add($"The file must be {options.MaxFileSizeBytes} bytes or smaller.");
        }

        if (!options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"The file extension '{extension}' is not allowed.");
        }

        if (!options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"The content type '{contentType}' is not allowed.");
        }

        return errors;
    }

    public static string NormalizeContentType(string? contentType)
    {
        return string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim().ToLowerInvariant();
    }
}