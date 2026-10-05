namespace Wp1Fall26Aws.Storage;

public sealed class StorageConfigurationException : Exception
{
    public StorageConfigurationException(string message)
        : base(message)
    {
    }
}