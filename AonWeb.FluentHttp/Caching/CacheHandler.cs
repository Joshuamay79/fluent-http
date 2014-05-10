﻿using System.Collections.Generic;

using AonWeb.FluentHttp.Handlers;
using System.Net.Http;
using System.Threading.Tasks;

namespace AonWeb.FluentHttp.Caching
{
    
    public class CacheHandler<TResult, TContent, TError> : CacheHandlerBase<TResult>, IHttpCallHandler<TResult, TContent, TError>
    {
        public CacheHandler()
            : this(new CacheSettings<TResult>()) { }

        public CacheHandler(CacheSettings<TResult> settings )
            : base(settings) { }

        public CacheHandler<TResult, TContent, TError> WithCaching(bool enabled = true)
        {
            Settings.Enabled = enabled;

            return this;
        }

        public CacheHandler<TResult, TContent, TError> WithDependentUris(IEnumerable<string> uris)
        {
            foreach (var uri in uris)
            {
                WithDependentUri(uri);
            }

            return this;
        }

        public CacheHandler<TResult, TContent, TError> WithDependentUri(string uri)
        {
            uri = uri.NormalizeUri();
            if (!string.IsNullOrWhiteSpace(uri) && !Settings.DependentUris.Contains(uri))
                Settings.DependentUris.Add(uri);

            return this;
        }

        public HttpCallHandlerPriority GetPriority(HttpCallHandlerType type)
        {
            if (type == HttpCallHandlerType.Sending)
                return HttpCallHandlerPriority.First;

            if (type == HttpCallHandlerType.Sent)
                return HttpCallHandlerPriority.Last;

            return HttpCallHandlerPriority.Default;
        }

        public async Task OnSending(HttpSendingContext<TResult, TContent, TError> context)
        {
            var result = await TryGetFromCache(context, context.Request);

            if (result.Found) 
                context.Result = result.Result;
        }

        public async Task OnSent(HttpSentContext<TResult, TContent, TError> context)
        {
            var result = TryGetRevalidatedResult(context, context.Response);

            if (result.Found)
                context.Result = result.Result;
        }

        public async Task OnResult(HttpResultContext<TResult, TContent, TError> context)
        {
            await TryCacheResult(context.Response, context.Result);
        }

        #region Unimplemented Methods

        
        // TODO: invalidate caches for uri on error or exception?
        public async Task OnError(HttpErrorContext<TResult, TContent, TError> context) { /* do nothing */  }
        public async Task OnException(HttpExceptionContext<TResult, TContent, TError> context) {  /* do nothing */ }
        
        #endregion
    }

    public class CacheHandler : CacheHandlerBase<HttpResponseMessage>, IHttpCallHandler
    {

        public CacheHandler()
            : this(new CacheSettings<HttpResponseMessage>()) { }

        public CacheHandler(CacheSettings<HttpResponseMessage> settings)
            : base(settings) { }

        public CacheHandler WithCaching(bool enabled = true)
        {
            Settings.Enabled = enabled;

            return this;
        }

        public CacheHandler WithDependentUris(IEnumerable<string> uris)
        {
            foreach (var uri in uris)
            {
                WithDependentUri(uri);
            }

            return this;
        }

        public CacheHandler WithDependentUri(string uri)
        {
            uri = uri.NormalizeUri();
            if (!string.IsNullOrWhiteSpace(uri) && !Settings.DependentUris.Contains(uri))
                Settings.DependentUris.Add(uri);

            return this;
        }

        public HttpCallHandlerPriority GetPriority(HttpCallHandlerType type)
        {
            if (type == HttpCallHandlerType.Sending)
                return HttpCallHandlerPriority.First;

            if (type == HttpCallHandlerType.Sent)
                return HttpCallHandlerPriority.Last;

            return HttpCallHandlerPriority.Default;
        }

        public async Task OnSending(HttpSendingContext context)
        {
            Settings.ResultInspector = cacheResult => cacheResult.Result.RequestMessage = context.Request;

            var result = await TryGetFromCache(context, context.Request);

            if (result.Found)
                context.Response = result.Result;
        }

        public async Task OnSent(HttpSentContext context)
        {
            var request = context.Response.RequestMessage;
            Settings.ResultInspector = cacheResult => cacheResult.Result.RequestMessage = request;

            var result = TryGetRevalidatedResult(context, context.Response);

            if (result.Found)
                context.Response = result.Result;

            await TryCacheResult(context.Response, context.Response);
        }

        #region Unimplemented Methods

        // TODO: invalidate caches for uri on exception?
        public async Task OnException(HttpExceptionContext context) {  /* do nothing */ }

        #endregion
    }
}
