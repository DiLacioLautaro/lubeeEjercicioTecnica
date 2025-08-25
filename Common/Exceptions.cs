namespace PruebaTecnica2025.Common;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message) { }
}
