namespace Wp1Fall26Aws.Storage;

public sealed class DocumentValidationException : Exception
{
    public DocumentValidationException(IReadOnlyList<string> errors)
        : base("The document failed validation.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}