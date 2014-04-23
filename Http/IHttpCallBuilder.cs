﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AonWeb.Fluent.Http
{
    public interface IHttpCallBuilder
    {
        IHttpCallBuilder WithUri(string uri);
        IHttpCallBuilder WithUri(Uri uri);
        IHttpCallBuilder WithQueryString(string name, string value);
        IHttpCallBuilder WithMethod(string method);
        IHttpCallBuilder WithMethod(HttpMethod method);
        IHttpCallBuilder WithContent(string content);
        IHttpCallBuilder WithContent(string content, Encoding encoding);
        IHttpCallBuilder WithContent(string content, Encoding encoding, string mediaType);
        IHttpCallBuilder WithContent(Func<string> contentFunc);
        IHttpCallBuilder WithContent(Func<string> contentFunc, Encoding encoding);
        IHttpCallBuilder WithContent(Func<string> contentFunc, Encoding encoding, string mediaType);
        IHttpCallBuilder WithContent(Func<HttpContent> contentFunc);
        HttpResponseMessage Result();
        Task<HttpResponseMessage> ResultAsync();

        IHttpCallBuilder CancelRequest();

        IAdvancedHttpCallBuilder Advanced { get; }

        //// conversion methods
        IHttpCallBuilder<T, string, string> WithResultOfType<T>();
        IHttpCallBuilder<HttpResponseMessage, T, string> WithContentOfType<T>();
        IHttpCallBuilder<HttpResponseMessage, string, T> WithErrorsOfType<T>();

    }

    public interface IHttpCallBuilder<TResult, in TContent, TError>
    {
        IHttpCallBuilder<TResult, TContent, TError> WithUri(string uri);
        IHttpCallBuilder<TResult, TContent, TError> WithUri(Uri uri);
        IHttpCallBuilder<TResult, TContent, TError> WithQueryString(string name, string value);
        IHttpCallBuilder<TResult, TContent, TError> WithMethod(string method);
        IHttpCallBuilder<TResult, TContent, TError> WithMethod(HttpMethod method);
        IHttpCallBuilder<TResult, TContent, TError> WithContent(TContent content);
        IHttpCallBuilder<TResult, TContent, TError> WithContent(TContent content, Encoding encoding);
        IHttpCallBuilder<TResult, TContent, TError> WithContent(TContent content, Encoding encoding, string mediaType);
        IHttpCallBuilder<TResult, TContent, TError> WithContent(Func<TContent> contentFunc);
        IHttpCallBuilder<TResult, TContent, TError> WithContent(Func<TContent> contentFunc, Encoding encoding);
        IHttpCallBuilder<TResult, TContent, TError> WithContent(Func<TContent> contentFunc, Encoding encoding, string mediaType);
        IHttpCallBuilder<TResult, TContent, TError> WithDefaultResult(TResult result);
        IHttpCallBuilder<TResult, TContent, TError> WithDefaultResult(Func<TResult> resultFunc);

        TResult Result();
        Task<TResult> ResultAsync();

        IHttpCallBuilder<TResult, TContent, TError> CancelRequest();

        // conversion methods
        IAdvancedHttpCallBuilder<TResult, TContent, TError> Advanced { get; }
        IHttpCallBuilder<T, TContent, TError> WithResultOfType<T>();
        IHttpCallBuilder<TResult, T, TError> WithContentOfType<T>();
        IHttpCallBuilder<TResult, TContent, T> WithErrorsOfType<T>();
    }
}