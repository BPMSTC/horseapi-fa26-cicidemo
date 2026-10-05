using Amazon.S3.Model;

namespace Wp1Fall26Aws.Storage;

public interface IS3ObjectClient
{
    Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default);
}