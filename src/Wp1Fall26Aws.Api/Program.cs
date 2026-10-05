using Scalar.AspNetCore;
using Wp1Fall26Aws.Storage;

// Create the application builder and register the API's OpenAPI and S3 storage services.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddS3DocumentStorage(builder.Configuration);

var app = builder.Build();

// Publish the OpenAPI document and its browsable UI in every environment so the
// deployed API is self-documenting. This API is public and unauthenticated.
app.MapOpenApi();
app.MapScalarApiReference();

// Advertise the available endpoints so the base URL is not a bare 404.
app.MapGet("/", () => Results.Ok(new
{
    service = "wp1-fall26-aws",
    endpoints = new[] { "GET /health", "POST /documents" },
    docs = new[] { "GET /scalar/v1", "GET /openapi/v1.json" }
}));

// Provide a lightweight endpoint for service and storage health checks.
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "wp1-fall26-aws",
    storage = "s3"
}));

// Accept a document upload as multipart/form-data and store it in S3.
app.MapPost("/documents", async (HttpRequest request, IDocumentStorage storage, CancellationToken cancellationToken) =>
{
    // Reject requests that cannot contain an uploaded file.
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "Submit the document as multipart/form-data with a file field." });
    }

    var form = await request.ReadFormAsync(cancellationToken);
    var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();

    // Require the named file field, while accepting the first file for simple clients.
    if (file is null)
    {
        return Results.BadRequest(new { error = "A file is required." });
    }

    await using var content = file.OpenReadStream();

    try
    {
        // Let the storage layer validate metadata and perform the upload.
        var result = await storage.UploadAsync(new DocumentUploadRequest(
            file.FileName,
            file.ContentType,
            file.Length,
            content), cancellationToken);

        return Results.Created($"/documents/{Uri.EscapeDataString(result.ObjectKey)}", result);
    }
    // Translate expected validation failures into client errors.
    catch (DocumentValidationException exception)
    {
        return Results.BadRequest(new { error = exception.Message, details = exception.Errors });
    }
    // Hide storage configuration details behind a generic server-error response.
    catch (StorageConfigurationException exception)
    {
        return Results.Problem(exception.Message, statusCode: StatusCodes.Status500InternalServerError);
    }
})
// Describe the request and response contracts for OpenAPI consumers.
.Accepts<IFormFile>("multipart/form-data")
.Produces<StoredDocument>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status500InternalServerError)
.WithName("UploadDocument");

app.Run();

public partial class Program;
