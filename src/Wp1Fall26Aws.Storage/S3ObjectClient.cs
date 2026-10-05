using Amazon.S3;
using Amazon.S3.Model;

namespace Wp1Fall26Aws.Storage;

public sealed class S3ObjectClient : IS3ObjectClient
{
    private readonly IAmazonS3 s3Client;

    public S3ObjectClient(IAmazonS3 s3Client)
    {
        this.s3Client = s3Client;
    }

    public Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
    {
        return s3Client.PutObjectAsync(request, cancellationToken);
    }
}