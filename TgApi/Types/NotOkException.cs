namespace TgApi.Types;

public class NotOkException: Exception
{
    public NotOkException() : base() {}
    public NotOkException(string? message) : base(message) { }
}
