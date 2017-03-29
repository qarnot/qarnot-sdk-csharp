using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.IO;

namespace qarnotsdk
{
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
                LookForErrorAndThrow (client, response);
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

        internal static async void LookForErrorAndThrow(HttpClient client, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode) {
                Error e;
                try {
                    e = await response.Content.ReadAsAsync<Error> ();
                  } catch (Exception ex) {
                    response.EnsureSuccessStatusCode ();
                    throw ex;
                }
                System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                Console.WriteLine (t.ToString());
                throw new Exception (e.Message);
            }
        }

       
    }
}

