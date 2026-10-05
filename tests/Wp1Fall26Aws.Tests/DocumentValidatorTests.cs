using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

public sealed class DocumentValidatorTests
{
    private readonly DocumentValidator validator = new();

    [Fact]
    public void Validate_AllowsTinySyntheticTextFile()
    {
        var request = new DocumentUploadRequest("synthetic-document.txt", "text/plain", 42, Stream.Null);
        var options = new S3StorageOptions();

        var errors = validator.Validate(request, options);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RejectsOversizedDocument()
    {
        var request = new DocumentUploadRequest("synthetic-document.txt", "text/plain", 11, Stream.Null);
        var options = new S3StorageOptions { MaxFileSizeBytes = 10 };

        var errors = validator.Validate(request, options);

        Assert.Contains(errors, error => error.Contains("10 bytes or smaller", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_RejectsUnsupportedFileType()
    {
        var request = new DocumentUploadRequest("script.sh", "text/x-shellscript", 10, Stream.Null);
        var options = new S3StorageOptions();

        var errors = validator.Validate(request, options);

        Assert.Contains(errors, error => error.Contains("extension '.sh'", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, error => error.Contains("content type 'text/x-shellscript'", StringComparison.OrdinalIgnoreCase));
    }
}