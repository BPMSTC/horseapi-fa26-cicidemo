namespace Wp1Fall26Aws.Storage;

public interface IDocumentStorage
{
    Task<StoredDocument> UploadAsync(DocumentUploadRequest request, CancellationToken cancellationToken = default);
}