using System;
using System.Threading.Tasks;
using System.Net.Http;

namespace QarnotSDK {
    internal static class Utils
    {
        internal static async Task LookForErrorAndThrowAsync(HttpClient client, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) {
                //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();

                // First try to retrieve the error returned by the API
                Error e;
                try {
                    e = await response.Content.ReadAsAsync<Error>();
                } catch (Exception ex) {
                    e = new Error();
                    e.Message = ex.Message;
                }

                // Then retrieve the HTTP error returned by the HttpClient
                Exception inner = null;
                try {
                    response.EnsureSuccessStatusCode();
                } catch(Exception ex) {
                    inner = ex;
                }

                if (e == null) {
                    if (inner != null) e = new Error(inner.Message);
                    else e = new Error("Unknown");
                }

                // Throw a custom error
                switch (response.StatusCode) {
                    case System.Net.HttpStatusCode.NotFound:
                        throw new QarnotApiResourceNotFoundException(e.Message, inner);
                    default:
                        throw new QarnotApiException(e.Message, inner, response);
                }
            }
        }
    }
}

