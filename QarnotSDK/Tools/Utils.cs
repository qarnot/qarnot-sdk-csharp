using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

#if (DEBUG)
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("QarnotSDK.UnitTests")]
#else
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("QarnotSDK.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d90399a0cf9d9d"
                                                                                            + "50a5999f3da1a6d5a5a9777d0e5faf95a10381da641b3fbf15f9ea13096eda48f1ab8498d348d3"
                                                                                            + "d711185ed238ec17a097c0c7a39754643f9eebecb6c12ccdc767c0655845fc03c33f1ee90a7cce"
                                                                                            + "23b6d8f40bde0bffff0d8a4cf2b0d0ea365b2f58c1293854c2f23ed7e8196a06e5e33a1e25f5fd"
                                                                                            + "3ab01d88")]
#endif

namespace QarnotSDK {
    internal static class Utils
    {
        internal static async Task LookForErrorAndThrowAsync(HttpClient client, HttpResponseMessage response,
            CancellationToken ct = default(CancellationToken))
        {
            if (!response.IsSuccessStatusCode) {
                //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();

                // First try to retrieve the error returned by the API
                Error e;
                try {
                    e = await response.Content.ReadAsAsync<Error>(cancellationToken: ct);
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
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return (collection == null || collection.Count() < 1);
        }

        internal static DelegatingHandler LinkHandlers(List<DelegatingHandler> handlers, HttpClientHandler httpClientHandler)
        {
            DelegatingHandler actual = null;

            foreach (var handler in handlers)
            {
                if (actual != null)
                {
                    actual.InnerHandler = handler;
                }
                actual = handler;
            }

            actual.InnerHandler = httpClientHandler;

            return handlers.First();
        }

    }
}
