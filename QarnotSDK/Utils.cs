using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.IO;

namespace qarnotsdk
{
    public class QarnotApiException : Exception {
        public QarnotApiException(string error, Exception inner) : base(error, inner) { }
    }

    public class QarnotApiResourceNotFoundException : QarnotApiException {
        public QarnotApiResourceNotFoundException(string error, Exception inner) : base(error, inner) { }
    }

    internal static class Utils
    {
        internal static async Task Download(HttpClient client, string disk, string filePath, string outDir, CancellationToken cancellationToken)
        {
            string fileUri = disk + "/" + filePath;
            var response = await client.GetAsync (
                fileUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode) {
                await LookForErrorAndThrow (client, response);
            }

            string outFile = outDir + "/" + filePath;

            if (!Directory.Exists (Path.GetDirectoryName (outFile)))
                Directory.CreateDirectory (Path.GetDirectoryName (outFile));
            //Console.WriteLine ("Downloading : " + fileUri + " to : " + outFile);
            using (var fileStream = new FileStream (outFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                using (var httpStream = await response.Content.ReadAsStreamAsync ()) {
                    httpStream.CopyTo (fileStream);
                    fileStream.Flush ();
                }
            }
        }

        internal static async Task LookForErrorAndThrow(HttpClient client, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) {
                //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                //Console.WriteLine (t.ToString());

                // First try to retrieve the error returned by the API
                Error e;
                try {
                    e = await response.Content.ReadAsAsync<Error>();
                } catch (Exception ex) {
                    e = new Error();
                    e.Message = ex.Message;
                }

                // Then retrieve the http error returned by the HttpClient
                Exception inner = null;
                try {
                    response.EnsureSuccessStatusCode();
                } catch(Exception ex) {
                    inner = ex;
                }

                // Throw a custom error
                switch (response.StatusCode) {
                    case System.Net.HttpStatusCode.NotFound:
                        throw new QarnotApiResourceNotFoundException(e.Message, inner);
                    default:
                        throw new QarnotApiException(e.Message, inner);
                }
            }
        }
    }
}

