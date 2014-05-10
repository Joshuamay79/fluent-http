using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;

using AonWeb.FluentHttp.Caching;
using AonWeb.FluentHttp.Client;
using AonWeb.FluentHttp.Exceptions;
using AonWeb.FluentHttp.Handlers;

namespace AonWeb.FluentHttp
{
    public static class HttpCallBuilderDefaults
    {
        static HttpCallBuilderDefaults()
        {
            Reset();
        }

        public static void Reset()
        {
            ValidStatusCodes = new HashSet<HttpStatusCode>
            { 
                HttpStatusCode.OK, 
                HttpStatusCode.Created,
                HttpStatusCode.Accepted,
                HttpStatusCode.NonAuthoritativeInformation,
                HttpStatusCode.NoContent,
                HttpStatusCode.ResetContent, 
                HttpStatusCode.PartialContent
            };

            // Call Defaults
            DefaultHttpMethod = HttpMethod.Get;
            DefaultCompletionOption = HttpCompletionOption.ResponseContentRead;
            DefaultSuccessfulResponseValidator = IsSuccessfulResponse;
            DefaultMediaType = "application/json";
            DefaultContentEncoding = Encoding.UTF8;

            DefaultMediaTypeFormatters = new MediaTypeFormatterCollection();

            //Client Defaults
            AutoDecompressionEnabled = true;
            DefaultClientConfiguration = null;
            DefaultRequestHeadersConfiguration = null;
            DefaultDecompressionMethods = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            DefaultClientCertificateOptions = null;
            DefaultClientTimeout = null;
            DefaultMaxRequestContentBufferSize = null;
            DefaultCredentials = null;
            SuppressCancellationErrors = true;

            //Redirect Defaults
            AutoRedirectEnabled = true;
            DefaultMaxAutoRedirects = 2;
            DefaultRedirectStatusCodes = new HashSet<HttpStatusCode>
            {
                HttpStatusCode.Redirect,
                HttpStatusCode.MovedPermanently,
                HttpStatusCode.Created
            };

            //Retry
            AutoRetryEnabled = true;
            DefaultMaxAutoRetries = 2;
            DefaultRetryAfter = TimeSpan.FromMilliseconds(100);
            DefaultMaxRetryAfter = TimeSpan.FromSeconds(5);
            DefaultRetryStatusCodes = new HashSet<HttpStatusCode> { HttpStatusCode.ServiceUnavailable };

            //Caching Defaults
            CachingEnabled = true;
            DefaultCacheableMethods = new HashSet<HttpMethod> { HttpMethod.Get };
            DefaultCacheableStatusCodes = new HashSet<HttpStatusCode> { HttpStatusCode.OK };
            DefaultVaryByHeaders = new HashSet<string> { "Accept" };
            DefaultCacheExpiration = TimeSpan.FromMinutes(15);
            DefaultCacheStoreFactory = () => new InMemoryCacheStore();
            DefaultVaryByStoreFactory = () => new InMemoryVaryByStore();
        }

        

        public static void ClearCache()
        {
            if (DefaultCacheStoreFactory == null)
                return;

            var store = DefaultCacheStoreFactory();

            store.Clear();

        }

        public static void RemoveItemFromCache(Uri uri)
        {
            if (DefaultCacheStoreFactory == null)
                return;

            var store = DefaultCacheStoreFactory();

            store.RemoveItem(uri);
        }

        public static MediaTypeFormatterCollection DefaultMediaTypeFormatters { get; set; }
        public static HttpMethod DefaultHttpMethod { get; set; }
        public static HttpCompletionOption DefaultCompletionOption { get; set; }
        public static Encoding DefaultContentEncoding { get; set; }
        public static string DefaultMediaType { get; set; }
        public static HashSet<HttpStatusCode> ValidStatusCodes { get; private set; }
        public static Func<HttpResponseMessage, bool> DefaultSuccessfulResponseValidator { get; set; }

        public static bool CachingEnabled { get; set; }
        public static TimeSpan DefaultCacheExpiration { get; set; }
        public static ISet<string> DefaultVaryByHeaders { get; set; }
        public static ISet<HttpStatusCode> DefaultCacheableStatusCodes { get; set; }
        public static ISet<HttpMethod> DefaultCacheableMethods { get; set; }
        public static Func<IHttpCacheStore> DefaultCacheStoreFactory { get; set; }
        public static Func<IVaryByStore> DefaultVaryByStoreFactory { get; set; }
        public static Action<IHttpClient> DefaultClientConfiguration { get; set; }
        public static Action<HttpRequestHeaders> DefaultRequestHeadersConfiguration { get; set; }
        public static bool AutoDecompressionEnabled { get; set; }
        public static TimeSpan? DefaultClientTimeout { get; set; }
        public static long? DefaultMaxRequestContentBufferSize { get; set; }
        public static DecompressionMethods? DefaultDecompressionMethods { get; set; }
        public static ClientCertificateOption? DefaultClientCertificateOptions { get; set; }
        public static ICredentials DefaultCredentials { get; set; }

        public static bool AutoRedirectEnabled { get; set; }
        public static int DefaultMaxAutoRedirects { get; set; }
        public static ISet<HttpStatusCode> DefaultRedirectStatusCodes { get; set; }

        public static bool AutoRetryEnabled { get; set; }
        public static int DefaultMaxAutoRetries { get; set; }
        public static TimeSpan DefaultRetryAfter { get; set; }
        public static TimeSpan DefaultMaxRetryAfter { get; set; }
        public static ISet<HttpStatusCode> DefaultRetryStatusCodes { get; set; }
        public static bool SuppressCancellationErrors { get; set; }

        private static bool IsSuccessfulResponse(HttpResponseMessage response)
        {
            return ValidStatusCodes.Contains(response.StatusCode);
        }

        public static Exception DefaultExceptionFactory<TResult, TContent, TError>(HttpErrorContext<TResult, TContent, TError> context)
        {
            return new HttpErrorException<TError>(context.Error, context.StatusCode);
        }

        public static TResult DefaultResultFactory<TResult>()
        {
            return default(TResult);
        }
    }
}