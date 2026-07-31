using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using System.Reflection;

#if (DEBUG)
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("QarnotSDK.UnitTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
#else
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("QarnotSDK.UnitTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d90399a0cf9d9d"
                                                                                            + "50a5999f3da1a6d5a5a9777d0e5faf95a10381da641b3fbf15f9ea13096eda48f1ab8498d348d3"
                                                                                            + "d711185ed238ec17a097c0c7a39754643f9eebecb6c12ccdc767c0655845fc03c33f1ee90a7cce"
                                                                                            + "23b6d8f40bde0bffff0d8a4cf2b0d0ea365b2f58c1293854c2f23ed7e8196a06e5e33a1e25f5fd"
                                                                                            + "3ab01d88")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
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
                    if (response.Content.Headers.ContentType?.MediaType == "application/problem+json") {
                        var problemDetailsStr = await response.Content.ReadAsStringAsync();
                        var problemDetails = JsonConvert.DeserializeObject<ProblemDetailsWithErrors>(problemDetailsStr);
                        e = new Error(problemDetailsStr, problemDetails);
                    } else {
                        e = await response.Content.ReadAsAsync<Error>(cancellationToken: ct);
                    }
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

        internal static string GetSanitizedBucketPath(string path, bool showWarnings = true, bool ensureNotNull = true)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ensureNotNull ? string.Empty : path;
            }
            var originalPath = path;
            var warnings = "";
            var directorySeparators = new[]{'/', '\\'};
            foreach (var separator in directorySeparators)
            {
                while (path.Contains(String.Format("{0}{0}",separator)))
                {
                    warnings = $"Warning: Bucket path should not contain duplicated slashes ('{String.Format("{0}{0}",separator)}')\n";
                    path = path.Replace(String.Format("{0}{0}", separator), separator.ToString());
                }
                if (path.StartsWith(separator.ToString()))
                {
                    warnings += $"Warning: Bucket path should not start with a slash ('{separator}')\n";
                    path = path.TrimStart(separator);
                }
            }

            if (path != originalPath)
            {
                warnings+= $"Remote path changed from {originalPath} to {path}.\n";
                warnings += "If bucket path sanitization is not wanted, please open a new connection with the constructor flag 'sanitizeBucketPaths: false'";
            }
            if (showWarnings)
            {
                Console.WriteLine(warnings);
            }
            return path.Trim();
        }

        /// <summary>
        /// Compare two dictionaries for value equality (same keys, and values equal according to the default equality comparer).
        /// </summary>
        internal static bool DictionaryEquals<TKey, TValue>(Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;
            foreach (var kv in a)
            {
                if (!b.TryGetValue(kv.Key, out var v) || !EqualityComparer<TValue>.Default.Equals(kv.Value, v)) return false;
            }
            return true;
        }

        /// <summary>
        /// Compute an order-independent hash code for a dictionary (XOR is commutative and associative so enumeration order
        /// does not matter), consistent with <see cref="DictionaryEquals{TKey, TValue}"/>.
        /// </summary>
        internal static int DictionaryHashCode<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict == null) return 0;
            int hash = dict.Count;
            foreach (var kv in dict)
            {
                hash ^= HashCode.Combine(kv.Key, kv.Value);
            }
            return hash;
        }

        /// <summary>
        /// Compare two sequences for element-wise equality, treating a null sequence as unequal to a non-null one
        /// instead of throwing (unlike a bare <c>Enumerable.SequenceEqual</c>).
        /// </summary>
        internal static bool SequenceEquals<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return Enumerable.SequenceEqual(a, b);
        }

        /// <summary>
        /// Compute an order-dependent hash code for a sequence, consistent with an <c>Enumerable.SequenceEqual</c>
        /// comparison performed on the same sequence (e.g. after an identical <c>OrderBy</c>).
        /// </summary>
        internal static int SequenceHashCode<T>(IEnumerable<T> seq)
        {
            if (seq == null) return 0;
            int hash = 17;
            foreach (var item in seq)
            {
                hash = HashCode.Combine(hash, item);
            }
            return hash;
        }

        internal static IEnumerable<MediaTypeFormatter> GetCustomResponseFormatter()
        {
            return new JsonMediaTypeFormatter[]
            {
                new JsonMediaTypeFormatter {
                    SerializerSettings = new JsonSerializerSettings {
                        Converters = new List<JsonConverter> {
                            new HardwareConstraintsJsonConverter(),
                            new ScalingPolicyConverter(),
                            new TimePeriodSpecificationConverter(),
                        }
                    }
                }
            };
        }
    }
}
