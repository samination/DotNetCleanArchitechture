namespace Domain.Helpers.Exceptions;

public class ConcurrencyException : CustomException
{
    public ConcurrencyException()
    {
    }

    public ConcurrencyException(string message)
        : base(message)
    {
    }
}

