using System.Threading.Tasks;
using UnityEngine.Networking;

public static class UnityWebRequestExtensions
{
    public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest webRequest)
    {
        var completionSource = new TaskCompletionSource<UnityWebRequest>();

        webRequest.SendWebRequest().completed += operation =>
        {
            if (
                (webRequest.result == UnityWebRequest.Result.ConnectionError)
                || (webRequest.result == UnityWebRequest.Result.ProtocolError)
            )
            {
                completionSource.SetException(new UnityWebRequestException(webRequest.error));
            }
            else
            {
                completionSource.SetResult(webRequest);
            }
        };

        return completionSource.Task;
    }
}

public class UnityWebRequestException : System.Exception
{
    public UnityWebRequestException(string message)
        : base(message) { }
}
