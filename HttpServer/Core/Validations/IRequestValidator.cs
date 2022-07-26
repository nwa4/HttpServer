namespace HttpServer.Core.Validations
{
    public interface IRequestValidator
    {
        WebResponse Validate(WebRequest request);
    }
}
