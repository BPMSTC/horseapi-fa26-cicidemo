using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Wp1Fall26Aws.Storage;

public static class DependencyInjection
{
    public static IServiceCollection AddS3DocumentStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3StorageOptions>(configuration.GetSection(S3StorageOptions.SectionName));
        services.AddSingleton<DocumentValidator>();
        services.AddSingleton<IS3ObjectClient, S3ObjectClient>();
        services.AddSingleton<IDocumentStorage, S3DocumentStorage>();
        services.AddSingleton<IAmazonS3>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<S3StorageOptions>>().Value;
            var configuredRegion = FirstValue(options.Region, Environment.GetEnvironmentVariable("AWS_REGION"), Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION"));

            if (configuredRegion is null)
            {
                return new AmazonS3Client();
            }

            return new AmazonS3Client(RegionEndpoint.GetBySystemName(configuredRegion));
        });

        return services;
    }

    private static string? FirstValue(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}