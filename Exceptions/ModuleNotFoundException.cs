namespace MasstransitTest.WebApiTest.Exceptions;

public class ModuleNotFoundException : Exception
{

    public ModuleNotFoundException(string? message) : base(message)
    {
    }

    public ModuleNotFoundException() : base()
    {
    }

    public ModuleNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
