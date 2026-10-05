using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Wp1Fall26Aws.Storage;

public sealed class S3DocumentStorage : IDocumentStorage
{
    private readonly IS3ObjectClient s3Client;
    private readonly S3StorageOptions options;
    private readonly DocumentValidator validator;

    public S3DocumentStorage(IS3ObjectClient s3Client, IOptions<S3StorageOptions> options, DocumentValidator validator)
    {
        this.s3Client = s3Client;
        this.options = options.Value;
        this.validator = validator;
    }

    public async Task<StoredDocument> UploadAsync(DocumentUploadRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new StorageConfigurationException("S3Storage:BucketName must be configured before documents can be uploaded.");
        }

        var errors = validator.Validate(request, options);
        if (errors.Count > 0)
        {
            throw new DocumentValidationException(errors);
        }

        var safeFileName = Path.GetFileName(request.FileName);
        var extension = Path.GetExtension(safeFileName).ToLowerInvariant();
        var contentType = DocumentValidator.NormalizeContentType(request.ContentType);
        var objectKey = BuildObjectKey(options.KeyPrefix, extension);
        var metadata = new Dictionary<string, string>
        {
            ["original-file-name"] = safeFileName,
            ["content-type"] = contentType,
            ["uploaded-utc"] = DateTimeOffset.UtcNow.ToString("O"),
            ["content-length"] = request.Length.ToString()
        };

        var putRequest = new PutObjectRequest
        {
            BucketName = options.BucketName,
            Key = objectKey,
            InputStream = request.Content,
            ContentType = contentType,
            AutoCloseStream = false
        };

        foreach (var item in metadata)
        {
            putRequest.Metadata.Add(item.Key, item.Value);
        }

        var response = await s3Client.PutObjectAsync(putRequest, cancellationToken);

        return new StoredDocument(options.BucketName, objectKey, request.Length, contentType, response.ETag, metadata);
    }

    private static string BuildObjectKey(string keyPrefix, string extension)
    {
        var normalizedPrefix = keyPrefix.Trim('/');
        var datePath = DateTimeOffset.UtcNow.ToString("yyyy/MM/dd");
        var fileName = $"{Guid.NewGuid():N}{extension}";

        return string.IsNullOrWhiteSpace(normalizedPrefix)
            ? $"{datePath}/{fileName}"
            : $"{normalizedPrefix}/{datePath}/{fileName}";
    }
}