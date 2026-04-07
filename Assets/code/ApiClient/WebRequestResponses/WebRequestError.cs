
public class WebRequestError: IWebRequestReponse
{
    public string ErrorMessage;

    public WebRequestError(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}
