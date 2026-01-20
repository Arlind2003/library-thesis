namespace Caktoje.Exceptions;

public class CriticalDevelopmentException(string message, string domain, string className) : Exception(message)
{
    public string Domain { get; } = domain;
    public string ClassName { get; } = className;
}