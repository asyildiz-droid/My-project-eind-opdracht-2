public class WebRequestData<T> : IWebRequestReponse
{
    public readonly T Data;

    public WebRequestData(T data)
    {
        Data = data;
    }
}
