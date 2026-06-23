namespace BuildingBlocks.Exceptions;

public class CustomValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public CustomValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public CustomValidationException(IReadOnlyDictionary<string, string[]> errors)
        : this()
    {
        Errors = errors;
    }
}