using System.Net;                          // HttpStatusCode
using System.Net.Http.Headers;             // MediaTypeHeaderValue
using Microsoft.AspNetCore.Mvc.Testing;    // WebApplicationFactory<T>
using Microsoft.AspNetCore.TestHost;       // ConfigureTestServices
using Microsoft.Extensions.DependencyInjection;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

// IClassFixture: xUnit creates ONE WebApplicationFactory and shares it across every test in this class.
public sealed class DocumentApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    // xUnit passes the shared factory in through the constructor.
    public DocumentApiTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    // Builds a client for an app whose S3 client is replaced by the given spy.
    private HttpClient CreateClientWithSpy(RecordingS3ObjectClientSpy spy)
    {
        return factory.WithWebHostBuilder(builder =>
        {
            // ConfigureTestServices runs AFTER Program.cs registers its services,
            // so these registrations win when the app asks for them.
            builder.ConfigureTestServices(services =>
            {
                // Replace the real S3 client (which would need AWS credentials) with our spy.
                services.AddSingleton<IS3ObjectClient>(spy);

                // S3DocumentStorage refuses to upload without a bucket name, so give it a fake one.
                services.PostConfigure<S3StorageOptions>(options => options.BucketName = "integration-test-bucket");
            });
        }).CreateClient();
    }

    // Builds a multipart/form-data body with one file in the "file" field, like a browser upload.
    private static MultipartFormDataContent CreateUpload(string fileName, string contentType, byte[] bytes)
    {
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType); // becomes IFormFile.ContentType

        var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", fileName); // field name "file" matches what the endpoint reads
        return form;
    }

    [Fact]
    public async Task GetHealth_AppRunning_ReturnsOk()
    {
        // Arrange: a plain client - /health doesn't touch S3, so no replacement needed.
        var client = factory.CreateClient();

        // Act: a real HTTP GET, handled entirely in memory.
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostDocuments_ValidTextFile_Returns201AndSendsToS3()
    {
        // Arrange
        var spy = new RecordingS3ObjectClientSpy();
        var client = CreateClientWithSpy(spy);
        using var upload = CreateUpload("synthetic-document.txt", "text/plain", "synthetic lab file"u8.ToArray());

        // Act
        var response = await client.PostAsync("/documents", upload);

        // Assert: the HTTP contract AND the side effect.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(spy.LastRequest);                                   // something reached the S3 boundary
        Assert.Equal("integration-test-bucket", spy.LastRequest!.BucketName);
        Assert.Equal("text/plain", spy.LastRequest.ContentType);
    }

    [Fact]
    public async Task PostDocuments_ShellScript_Returns400AndNeverCallsS3()
    {
        // Arrange
        var spy = new RecordingS3ObjectClientSpy();
        var client = CreateClientWithSpy(spy);
        using var upload = CreateUpload("script.sh", "text/x-shellscript", "echo hi"u8.ToArray());

        // Act
        var response = await client.PostAsync("/documents", upload);

        // Assert: rejected at the boundary, and S3 was never called.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(spy.LastRequest);
    }
}
