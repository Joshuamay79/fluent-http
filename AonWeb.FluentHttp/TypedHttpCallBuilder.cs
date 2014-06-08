﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

using AonWeb.FluentHttp.Caching;
using AonWeb.FluentHttp.Client;
using AonWeb.FluentHttp.Handlers;
using AonWeb.FluentHttp.Serialization;

namespace AonWeb.FluentHttp
{
    public class TypedHttpCallBuilder : IRecursiveTypedHttpCallBuilder
    {
        private readonly TypedHttpCallBuilderSettings _settings;
        private readonly IChildHttpCallBuilder _innerBuilder;

        private readonly  IHttpCallFormatter _formatter;

        protected TypedHttpCallBuilder()
            : this(new HttpCallFormatter()) { }

        protected TypedHttpCallBuilder(IHttpCallFormatter formatter)
            : this(HttpCallBuilder.CreateAsChild(), formatter) { }

        protected TypedHttpCallBuilder(IChildHttpCallBuilder builder, IHttpCallFormatter formatter)
            : this(new TypedHttpCallBuilderSettings(), builder, formatter) { }

        private TypedHttpCallBuilder(TypedHttpCallBuilderSettings settings, IChildHttpCallBuilder builder, IHttpCallFormatter formatter)
            : this(settings, builder, formatter, new TypedCacheHandler()) { }

        private TypedHttpCallBuilder(TypedHttpCallBuilderSettings settings, IChildHttpCallBuilder builder,  IHttpCallFormatter formatter, params ITypedHttpCallHandler[] defaultHandlers)
        {
            _formatter = formatter;
            _settings = settings;
            _innerBuilder = builder;

            builder.ApplySettings(_settings);
            
            foreach (var handler in defaultHandlers)
                WithHandler(handler);
        }

        public static ITypedHttpCallBuilder Create()
        {
            return new TypedHttpCallBuilder();
        }

        public static ITypedHttpCallBuilder Create(string baseUri)
        {
            return Create().WithBaseUri(baseUri);
        }

        public static ITypedHttpCallBuilder Create(Uri baseUri)
        {
            return Create().WithBaseUri(baseUri);
        }

        public ITypedHttpCallBuilder WithUri(string uri)
        {
            _innerBuilder.WithUri(uri);

            return this;
        }

        public ITypedHttpCallBuilder WithUri(Uri uri)
        {
            _innerBuilder.WithUri(uri);

            return this;
        }

        public ITypedHttpCallBuilder WithBaseUri(string uri)
        {
            _innerBuilder.WithBaseUri(uri);

            return this;
        }

        public ITypedHttpCallBuilder WithBaseUri(Uri uri)
        {
            _innerBuilder.WithBaseUri(uri);

            return this;
        }

        public ITypedHttpCallBuilder WithRelativePath(string pathAndQuery)
        {
            _innerBuilder.WithRelativePath(pathAndQuery);

            return this;
        }

        public ITypedHttpCallBuilder WithQueryString(string name, string value)
        {
            _innerBuilder.WithQueryString(name, value);

            return this;
        }

        public ITypedHttpCallBuilder WithQueryString(NameValueCollection values)
        {
            _innerBuilder.WithQueryString(values);

            return this;
        }

        public ITypedHttpCallBuilder AsGet()
        {
            return WithMethod(HttpMethod.Get);
        }

        public ITypedHttpCallBuilder AsPut()
        {
            return WithMethod(HttpMethod.Put);
        }

        public ITypedHttpCallBuilder AsPost()
        {
            return WithMethod(HttpMethod.Post);
        }

        public ITypedHttpCallBuilder AsDelete()
        {
            return WithMethod(HttpMethod.Delete);
        }

        public ITypedHttpCallBuilder AsPatch()
        {
            return WithMethod(new HttpMethod("PATCH"));
        }

        public ITypedHttpCallBuilder AsHead()
        {
            return WithMethod(HttpMethod.Head);
        }

        public IAdvancedTypedHttpCallBuilder WithScheme(string scheme)
        {
            _innerBuilder.WithScheme(scheme);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithHost(string host)
        {
            _innerBuilder.WithHost(host);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithPort(int port)
        {
            _innerBuilder.WithPort(port);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithPath(string absolutePathAndQuery)
        {
            _innerBuilder.WithPath(absolutePathAndQuery);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithContentEncoding(Encoding encoding)
        {
            _settings.ContentEncoding = encoding;

            _innerBuilder.WithContentEncoding(encoding);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithMediaType(string mediaType)
        {
            _settings.MediaType = mediaType;

            _innerBuilder.WithMediaType(mediaType);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithMethod(string method)
        {
            _innerBuilder.WithMethod(method);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithMethod(HttpMethod method)
        {
            _innerBuilder.WithMethod(method);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithAcceptHeader(string mediaType)
        {
            _innerBuilder.WithAcceptHeader(mediaType);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithAcceptCharSet(Encoding encoding)
        {
            _innerBuilder.WithAcceptCharSet(encoding);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithAcceptCharSet(string charSet)
        {
            _innerBuilder.WithAcceptCharSet(charSet);

            return this;
        }

        public ITypedHttpCallBuilder WithContent<TContent>(TContent content)
        {

            return WithContent(content, _settings.ContentEncoding, _settings.MediaType);
        }

        public ITypedHttpCallBuilder WithContent<TContent>(TContent content, Encoding encoding)
        {

            return WithContent(content, encoding, _settings.MediaType);
        }

        public ITypedHttpCallBuilder WithContent<TContent>(TContent content, Encoding encoding, string mediaType)
        {

            return WithContent(() => content, encoding, mediaType);
        }

        public ITypedHttpCallBuilder WithContent<TContent>(Func<TContent> contentFactory)
        {
            return WithContent(contentFactory, _settings.ContentEncoding, _settings.MediaType);
        }

        public ITypedHttpCallBuilder WithContent<TContent>(Func<TContent> contentFactory, Encoding encoding)
        {
            return WithContent(contentFactory, encoding, _settings.MediaType);
        }

        public ITypedHttpCallBuilder WithContent<TContent>(Func<TContent> contentFactory, Encoding encoding, string mediaType)
        {
            if (contentFactory == null)
                throw new ArgumentNullException("contentFactory");

            _settings.ContentType = typeof(TContent);

            if (!typeof(IEmptyRequest).IsAssignableFrom(typeof(TContent)))
                _settings.ContentFactory = () => contentFactory();

            WithContentEncoding(encoding);
            WithMediaType(mediaType);

            return this;
        }

        public ITypedHttpCallBuilder WithDefaultResult<TResult>(TResult result)
        {
            return WithDefaultResult(() => result);
        }

        public ITypedHttpCallBuilder WithDefaultResult<TResult>(Func<TResult> resultFactory)
        {
            _settings.DefaultResultFactory = t => resultFactory();

            return this;
        }

        public ITypedHttpCallBuilder WithErrorType<TError>()
        {
            _settings.ErrorType = typeof(TError);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder ConfigureClient(Action<IHttpClientBuilder> configuration)
        {
            _innerBuilder.ConfigureClient(configuration);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithMediaTypeFormatter(MediaTypeFormatter formatter)
        {
            _settings.MediaTypeFormatters.Add(formatter);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder ConfigureMediaTypeFormatter<TFormatter>(Action<TFormatter> configure) where TFormatter : MediaTypeFormatter
        {
            if (configure == null)
                throw new ArgumentNullException("configure");

            var formatter = _settings.MediaTypeFormatters.OfType<TFormatter>().FirstOrDefault();

            if (formatter != null)
                configure(formatter);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithHandler<TResult, TContent, TError>(ITypedHttpCallHandler handler)
        {
            _settings.Handler.AddHandler<TResult, TContent, TError>(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithHandler(ITypedHttpCallHandler handler)
        {
            return WithHandler<object, object, object>(handler);
        }

        public IAdvancedTypedHttpCallBuilder ConfigureHandler<THandler>(Action<THandler> configure)
            where THandler : class, IHttpCallHandler
        {
            _settings.Handler.ConfigureHandler(configure);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder TryConfigureHandler<THandler>(Action<THandler> configure) where THandler : class, IHttpCallHandler
        {
            _settings.Handler.ConfigureHandler(configure, false);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithSuccessfulResponseValidator(Func<HttpResponseMessage, bool> validator)
        {
            if (validator == null)
                throw new ArgumentNullException("validator");

            _settings.SuccessfulResponseValidators.Add(validator);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithExceptionFactory(Func<HttpCallErrorContext, Exception> factory)
        {
            _settings.ExceptionFactory = factory;

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithCaching(bool enabled = true)
        {

            ConfigureHandler<CacheHandler>(handler => handler.WithCaching(enabled));

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithNoCache(bool nocache = true)
        {
            _innerBuilder.WithNoCache(nocache);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithDependentUri(Uri uri)
        {
            return TryConfigureHandler<CacheHandler>(h => h.WithDependentUri(uri));
        }

        public IAdvancedTypedHttpCallBuilder WithDependentUris(IEnumerable<Uri> uris)
        {
            return TryConfigureHandler<CacheHandler>(h => h.WithDependentUris(uris));
        }

        public IAdvancedTypedHttpCallBuilder OnSending<TResult, TContent>(Action<TypedHttpSendingContext<TResult, TContent>> handler)
        {
            _settings.Handler.AddSendingHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnSending<TResult, TContent>(HttpCallHandlerPriority priority, Action<TypedHttpSendingContext<TResult, TContent>> handler)
        {
            _settings.Handler.AddSendingHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnSending<TResult, TContent>(Func<TypedHttpSendingContext<TResult, TContent>, Task> handler)
        {
            _settings.Handler.AddSendingHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnSending<TResult, TContent>(HttpCallHandlerPriority priority, Func<TypedHttpSendingContext<TResult, TContent>, Task> handler)
        {
            _settings.Handler.AddSendingHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnSent<TResult>(Action<TypedHttpSentContext<TResult>> handler)
        {
            _settings.Handler.AddSentHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnSent<TResult>(HttpCallHandlerPriority priority, Action<TypedHttpSentContext<TResult>> handler)
        {
            _settings.Handler.AddSentHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnSent<TResult>(Func<TypedHttpSentContext<TResult>, Task> handler)
        {
            _settings.Handler.AddSentHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnSent<TResult>(HttpCallHandlerPriority priority, Func<TypedHttpSentContext<TResult>, Task> handler)
        {
            _settings.Handler.AddSentHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnResult<TResult>(Action<TypedHttpResultContext<TResult>> handler)
        {
            _settings.Handler.AddResultHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnResult<TResult>(HttpCallHandlerPriority priority, Action<TypedHttpResultContext<TResult>> handler)
        {
            _settings.Handler.AddResultHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnResult<TResult>(Func<TypedHttpResultContext<TResult>, Task> handler)
        {
            _settings.Handler.AddResultHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnResult<TResult>(HttpCallHandlerPriority priority, Func<TypedHttpResultContext<TResult>, Task> handler)
        {
            _settings.Handler.AddResultHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnError<TError>(Action<TypedHttpCallErrorContext<TError>> handler)
        {
            _settings.Handler.AddErrorHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnError<TError>(HttpCallHandlerPriority priority, Action<TypedHttpCallErrorContext<TError>> handler)
        {
            _settings.Handler.AddErrorHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnError<TError>(Func<TypedHttpCallErrorContext<TError>, Task> handler)
        {
            _settings.Handler.AddErrorHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnError<TError>(HttpCallHandlerPriority priority, Func<TypedHttpCallErrorContext<TError>, Task> handler)
        {
            _settings.Handler.AddErrorHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnException(Action<TypedHttpCallExceptionContext> handler)
        {
            _settings.Handler.AddExceptionHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnException(HttpCallHandlerPriority priority, Action<TypedHttpCallExceptionContext> handler)
        {
            _settings.Handler.AddExceptionHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnException(Func<TypedHttpCallExceptionContext, Task> handler)
        {
            _settings.Handler.AddExceptionHandler(handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder OnException(HttpCallHandlerPriority priority, Func<TypedHttpCallExceptionContext, Task> handler)
        {
            _settings.Handler.AddExceptionHandler(priority, handler);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithAutoDecompression(bool enabled = true)
        {
            _innerBuilder.WithAutoDecompression(enabled);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithSuppressCancellationErrors(bool suppress = true)
        {
            _innerBuilder.WithSuppressCancellationErrors(suppress);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder WithTimeout(TimeSpan? timeout)
        {
            _innerBuilder.WithTimeout(timeout);

            return this;
        }

        public IAdvancedTypedHttpCallBuilder Advanced
        {
            get
            {
                return this;
            }
        }

        public ITypedHttpCallBuilder CancelRequest()
        {
            _innerBuilder.CancelRequest();

            return this;
        }

        public async Task<TResult> ResultAsync<TResult>()
        {
            if (typeof(IEmptyResult).IsAssignableFrom(typeof(TResult)))
                ConfigureResponseDeserialization(false);

            var result = await RecursiveResultAsync<TResult>();

            Reset();

            return result;
        }

        public async Task SendAsync()
        {
            ConfigureResponseDeserialization(false);

            await ResultAsync(new TypedHttpCallContext(this, _settings)).ConfigureAwait(false);
        }

        public async Task<TResult> RecursiveResultAsync<TResult>()
        {
            var result = await ResultAsync(new TypedHttpCallContext(this, _settings)).ConfigureAwait(false);

            return (TResult)result;
        }

        private async Task<object> ResultAsync(TypedHttpCallContext context)
        {
            HttpResponseMessage response = null;
            ExceptionDispatchInfo capturedException = null;
            try
            {
                //build content before creating request
                var hasContent = false;
                object content = null;

                if (context.ContentFactory != null)
                {
                    content = context.ContentFactory();

                    var httpContent = await _formatter.CreateContent(content, context);

                    _innerBuilder.WithContent(() => httpContent);
                    hasContent = true;
                }

                using (var request = _innerBuilder.CreateRequest())
                {
                    var sendingResult = await context.Handler.OnSending(context, request, content, hasContent);

                    if (sendingResult.Modified)
                        return sendingResult.Value;

                    response = await _innerBuilder.ResultFromRequestAsync(request);
                }

                if (!context.IsSuccessfulResponse(response))
                {
                    object error;

                    if (typeof(IEmptyError).IsAssignableFrom(context.ErrorType)) 
                        error = Helper.GetDefaultValueForType(context.ErrorType);
                    else
                        error = await _formatter.DeserializeError(response, context);

                    var errorResult = await context.Handler.OnError(context, response, error);

                    if (!(bool)errorResult.Value) 
                        if (_settings.ExceptionFactory != null) 
                            throw _settings.ExceptionFactory(new HttpCallErrorContext(context, response, error));
                }
                else
                {
                    var sentResult = await context.Handler.OnSent(context, response);

                    object result = null;

                    if (sentResult.Modified)
                    {
                        result = sentResult.Value;
                    }
                    else
                    {
                        if (context.DeserializeResult)
                            result = await _formatter.DeserializeResult(response, context);
                    }

                    var resultResult = await context.Handler.OnResult(context, response, result);

                    return resultResult.Value;
                }
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
            }
            finally
            {
                Helper.DisposeResponse(response);
            }

            if (capturedException != null)
            {
                
                var exceptionResult = await context.Handler.OnException(context, capturedException.SourceException);

                if (!(bool)exceptionResult.Value)
                    capturedException.Throw();
            }

            return _settings.DefaultResultFactory(context.ResultType);
        }

        private void ConfigureResponseDeserialization(bool willDeserialize)
        {
            _settings.DeserializeResult = willDeserialize;
            _innerBuilder.TryConfigureHandler<FollowLocationHandler>(h => h.Enabled = willDeserialize);
        }

        private void Reset()
        {
            ConfigureResponseDeserialization(true);

            _settings.Reset();
        }
    }
}
