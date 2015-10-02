using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using AonWeb.FluentHttp.Caching;
using AonWeb.FluentHttp.Helpers;

namespace AonWeb.FluentHttp.Handlers.Caching
{
    public class CacheSettings : ICacheSettings
    {
        public CacheSettings()
        {
            Enabled = Defaults.Caching.Enabled;
            CacheableHttpMethods = new HashSet<HttpMethod>(Defaults.Caching.CacheableHttpMethods);
            CacheableHttpStatusCodes = new HashSet<HttpStatusCode>(Defaults.Caching.CacheableHttpStatusCodes);
            DefaultVaryByHeaders = new HashSet<string>(Defaults.Caching.VaryByHeaders);
            DependentUris = new HashSet<Uri>();
            DefaultDurationForCacheableResults = Defaults.Caching.DefaultDurationForCacheableResults;
            Handler = new CacheHandlerRegister();
            CacheValidator = Defaults.Caching.CacheValidator;
            RevalidateValidator = Defaults.Caching.RevalidateValidator;
            ResponseValidator = Defaults.Caching.ResponseValidator;
            AllowStaleResultValidator = Defaults.Caching.AllowStaleResultValidator;
            SuppressTypeMismatchExceptions = Defaults.TypedBuilder.SuppressTypeMismatchExceptions;
            CacheKeyBuilder = Defaults.Caching.CacheKeyBuilderFactory?.Invoke() ?? new CacheKeyBuilder();
        }

        public ISet<HttpMethod> CacheableHttpMethods { get; }
        public ISet<HttpStatusCode> CacheableHttpStatusCodes { get; }
        public ISet<string> DefaultVaryByHeaders { get; }
        public ISet<Uri> DependentUris { get; }
        public CacheHandlerRegister Handler { get; }

        public bool SuppressTypeMismatchExceptions { get; }

        public bool Enabled { get; set; }
        public Action<CacheResult> ResultInspector { get; set; }
        public Func<ICacheContext, ResponseInfo, ResponseValidationResult> ResponseValidator { get; set; }
        public Func<ICacheContext, bool> CacheValidator { get; set; }
        public Func<ICacheContext, ResponseInfo, bool> RevalidateValidator { get; set; }
        public Func<ICacheContext, ResponseInfo, bool> AllowStaleResultValidator { get; set; }
        public ICacheKeyBuilder CacheKeyBuilder { get; set; }
        public TimeSpan? DefaultDurationForCacheableResults { get; set; }
        public bool MustRevalidateByDefault { get; set; }
        public TimeSpan? CacheDuration { get; set; }

        public ISet<string> GetVaryByHeaders(Uri uri)
        {
            return CollectionHelpers.MergeSet(DefaultVaryByHeaders, Cache.CurrentVaryByStore.Get(uri));
        }
    }
}