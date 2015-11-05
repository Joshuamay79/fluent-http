﻿using System;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AonWeb.FluentHttp.Exceptions;
using AonWeb.FluentHttp.Handlers;
using AonWeb.FluentHttp.Serialization;

namespace AonWeb.FluentHttp.Helpers
{
    internal static class ObjectHelpers
    {
        public static Task ToTask<T>(this Action<T> action, T arg)
        {
            action?.Invoke(arg);

            return Task.FromResult(true);
        }

        public static void Dispose(IDisposable disposable)
        {
            try
            {
                disposable?.Dispose();
            }
            catch (ObjectDisposedException)
            {
            } 
        }

        public static CancellationTokenSource GetCancellationTokenSource(CancellationToken token)
        {
            return token == CancellationToken.None ? new CancellationTokenSource() : CancellationTokenSource.CreateLinkedTokenSource(token);
        }

        public static Task<HttpContent> CreateHttpContent(ITypedBuilderContext context, object content)
        {
            // TODO: handle empty request?
            return context.Formatter.CreateContent(content, context);
        }

        public static async Task<object> CreateResult(ITypedBuilderContext context, HttpRequestMessage request, HttpResponseMessage response)
        {
            object result;
            if (typeof(IEmptyResult).IsAssignableFrom(context.ResultType))
            {
                result = TypeHelpers.GetDefaultValueForType(context.ResultType);
            }
            else
            {
                result = await context.Formatter.DeserializeResult(response, context);
            }

            var resultWithMeta = result as IResultWithWritableMetadata;
            if (resultWithMeta != null)
            {
                resultWithMeta.Metadata = CachingHelpers.CreateResponseMetadata(result, request, response, context.CacheMetadata);
            }

            return result;
        }

        public static async Task<object> CreateError(
            ITypedBuilderContext context, HttpRequestMessage request,
            HttpResponseMessage response, ExceptionDispatchInfo capturedException)
        {
            var allowNullError = typeof (IEmptyError).IsAssignableFrom(context.ErrorType);

            object error = null;
            if (!allowNullError && response != null)
            {
                try
                {
                    error = await context.Formatter.DeserializeError(response, context);
                }
                catch (Exception ex)
                {
                    var newException = capturedException != null ? new AggregateException(ex, capturedException.SourceException) : ex;

                    capturedException =  ExceptionDispatchInfo.Capture(newException);
                }
            }

            if (error == null)
            {
                error = capturedException != null 
                    ?  context.DefaultErrorFactory?.Invoke(context.ErrorType, capturedException.SourceException) 
                    : TypeHelpers.GetDefaultValueForType(context.ErrorType);
            }

            if(error == null && !allowNullError)
                capturedException?.Throw();

            var errorWithMeta = error as IResultWithWritableMetadata;

            if (errorWithMeta != null)
            {
                errorWithMeta.Metadata = CachingHelpers.CreateResponseMetadata(error, request, response, context.CacheMetadata);
            }

            return error;
        }

        public static Exception CreateTypedException(ExceptionCreationContext context)
        {
            var openExType = typeof(HttpErrorException<>);

            var exType = openExType.MakeGenericType(context.ErrorType);

            var message = context.Error?.ToString() ?? context.InnerException?.Message;

            return (Exception)Activator.CreateInstance(exType, context.Error, context.StatusCode, message, context.InnerException);
        }

        public static Exception CreateHttpException(HttpResponseMessage response)
        {
            var statusCode = ((int?)response?.StatusCode).ToString() ?? "<Unknown>";
            var reasonPhrase = response?.ReasonPhrase ?? "<Unknown>";
            return new HttpRequestException($"Recieved response {statusCode} - {reasonPhrase}, which did not pass response validation. " + response.DetailsForException());
        }
    }
}