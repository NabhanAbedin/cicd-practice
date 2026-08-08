namespace CicdPractice.Api.Exceptions;

public class DuplicateJerseyNumberException : Exception
{
    public DuplicateJerseyNumberException(string message) : base(message)
    {
    }
}
