namespace DalilAcademy.Core.Exceptions
{
    public class UnauthorizedException(string message)
        : Exception(message)
    {
    }
}