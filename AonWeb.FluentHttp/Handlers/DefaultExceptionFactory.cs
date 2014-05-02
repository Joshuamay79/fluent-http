using System;

using AonWeb.FluentHttp.Exceptions;

namespace AonWeb.FluentHttp.Handlers
{
    public class DefaultExceptionFactory
    {
        public static Exception CreateException(HttpErrorContext context)
        {
            return new HttpCallException(context.StatusCode);
        }
    }

    public class DefaultExceptionFactory<TError>
    {
        public static Exception CreateException<TResult, TContent>(HttpErrorContext<TResult, TContent, TError> context )
        {
            return new HttpErrorException<TError>(context.Error, context.StatusCode);
        }
    }
}