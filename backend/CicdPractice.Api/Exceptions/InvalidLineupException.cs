namespace CicdPractice.Api.Exceptions;

public class InvalidLineupException : Exception
{
    public InvalidLineupException(string message) : base(message)
    {
    }
}
