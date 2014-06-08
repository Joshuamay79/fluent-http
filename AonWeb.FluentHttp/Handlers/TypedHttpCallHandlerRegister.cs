using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AonWeb.FluentHttp.Handlers
{
    public class TypedHttpCallHandlerRegister
    {
        private readonly ISet<ITypedHttpCallHandler> _callHandlers;
        private readonly IDictionary<HttpCallHandlerType, IList<KeyValuePair<HttpCallHandlerPriority, Delegate>>> _handlers;

        public TypedHttpCallHandlerRegister()
        {
            _callHandlers = new HashSet<ITypedHttpCallHandler>();
            _handlers = new Dictionary<HttpCallHandlerType, IList<KeyValuePair<HttpCallHandlerPriority, Delegate>>>();

            foreach (var callType in Enum.GetValues(typeof(HttpCallHandlerType)).Cast<HttpCallHandlerType>())
            {
                _handlers[callType] = new List<KeyValuePair<HttpCallHandlerPriority, Delegate>>();
            }
        }

        private Type CreateHandlerContextType(Type contextType, Type[] genericTypes)
        {
            var closedContextType = contextType;

            if (contextType.IsGenericTypeDefinition)
            {
                closedContextType = contextType.MakeGenericType(genericTypes);
            }

            return closedContextType;
        }

        private TypedHttpCallHandlerContext CreateHandlerContext(Type contextType, params object[] ctorArgs)
        {
            return (TypedHttpCallHandlerContext)Activator.CreateInstance(contextType, ctorArgs);
        }

        private Type CreateHandlerType(Type contextType)
        {
            return typeof(Func<,>).MakeGenericType(contextType, typeof(Task));
        }

        private Task InvokeHandler(Type handlerType, Delegate handler, TypedHttpCallHandlerContext context)
        {
            return (Task)handlerType.InvokeMember("Invoke", BindingFlags.InvokeMethod, null, handler, new object[] { context });
        }

        private async Task<HttpCallHandlerResult> InvokeHandlers(HttpCallHandlerType callType, Type contextType, bool suppressTypeExceptions, params object[] handlerConstructorArgs)
        {
            var handlerContext = CreateHandlerContext(contextType, handlerConstructorArgs);
            var handlerType = CreateHandlerType(contextType);
            var tasks = new List<Task>();

            foreach (var pair in _handlers[callType].OrderBy(kp => kp.Key))
            {
                var priority = pair.Key;
                var handler = pair.Value;

                if (!ValidateContextType(handler, handlerType, suppressTypeExceptions))
                    continue;

                var task = InvokeHandler(handlerType, handler, handlerContext);

                if (priority != HttpCallHandlerPriority.Parallel) 
                    await task;
                else
                    tasks.Add(task);
            }

            if (tasks.Count > 0) 
                await Task.WhenAll(tasks);

            return handlerContext.GetHandlerResult();
        }

        private bool ValidateContextType(object handler, Type genericType, bool suppressException)
        {
            var valid = genericType.IsInstanceOfType(handler);

            if (!valid && !suppressException)
                throw new InvalidCastException("This type ain't that type");

            return valid;
        }

        private void AddHandler(HttpCallHandlerType callType, HttpCallHandlerPriority priority, Delegate handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            _handlers[callType].Add(new KeyValuePair<HttpCallHandlerPriority, Delegate>(priority, handler));
        }

        public async Task<HttpCallHandlerResult> OnSending(TypedHttpCallContext context, HttpRequestMessage request, object content, bool hasContent)
        {
            var contextType = CreateHandlerContextType(typeof(TypedHttpSendingContext<,>), new[] { context.ResultType, context.ContentType });

            return await InvokeHandlers(HttpCallHandlerType.Sending, contextType, context.SuppressHandlerTypeExceptions, context, request, content, hasContent);
        }

        public async Task<HttpCallHandlerResult> OnSent(TypedHttpCallContext context, HttpResponseMessage response)
        {
            var contextType = CreateHandlerContextType(typeof(TypedHttpSentContext<>), new[] { context.ResultType });

            return await InvokeHandlers(HttpCallHandlerType.Sent, contextType, context.SuppressHandlerTypeExceptions, context, response);
        }

        public async Task<HttpCallHandlerResult> OnResult(TypedHttpCallContext context, HttpResponseMessage response, object result)
        {
            var contextType = CreateHandlerContextType(typeof(TypedHttpResultContext<>), new[] { context.ResultType });

            return await InvokeHandlers(HttpCallHandlerType.Result, contextType, context.SuppressHandlerTypeExceptions, context, response, result);
        }

        public async Task<HttpCallHandlerResult> OnError(TypedHttpCallContext context, HttpResponseMessage response, object error)
        {
            var contextType = CreateHandlerContextType(typeof(TypedHttpCallErrorContext<>), new[] { context.ErrorType });

            return await InvokeHandlers(HttpCallHandlerType.Error, contextType, context.SuppressHandlerTypeExceptions, context, response, error);
        }

        public async Task<HttpCallHandlerResult> OnException(TypedHttpCallContext context, Exception exception)
        {
            var contextType = CreateHandlerContextType(typeof(TypedHttpCallExceptionContext), null);

            return await InvokeHandlers(HttpCallHandlerType.Exception, contextType, context.SuppressHandlerTypeExceptions, context, exception);
        }


        public TypedHttpCallHandlerRegister AddHandler<TResult, TContent, TError>(ITypedHttpCallHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            if (_callHandlers.Contains(handler))
                throw new InvalidOperationException(SR.HanderAlreadyExistsError);

            _callHandlers.Add(handler);

            AddSendingHandler<TResult, TContent>(handler.GetPriority(HttpCallHandlerType.Sending), async ctx =>
            {
                if (handler.Enabled)
                    await handler.OnSending(ctx);
            });

            AddSentHandler<TResult>(handler.GetPriority(HttpCallHandlerType.Sent), async ctx =>
                {
                    if (handler.Enabled)
                        await handler.OnSent(ctx);
                });

            AddResultHandler<TResult>(handler.GetPriority(HttpCallHandlerType.Result), async ctx =>
                {
                    if (handler.Enabled)
                        await handler.OnResult(ctx);
                });

            AddErrorHandler<TError>(handler.GetPriority(HttpCallHandlerType.Error), async ctx =>
                {
                    if (handler.Enabled)
                        await handler.OnError(ctx);
                });

            AddExceptionHandler(handler.GetPriority(HttpCallHandlerType.Exception), async ctx =>
                {
                    if (handler.Enabled)
                        await handler.OnException(ctx);
                });

            return this;
        }

        public TypedHttpCallHandlerRegister ConfigureHandler<THandler>(Action<THandler> configure, bool throwOnNotFound = true)
            where THandler : class, IHttpCallHandler
        {
            if (configure == null)
                throw new ArgumentNullException("configure");

            var handler = _callHandlers.OfType<THandler>().FirstOrDefault();

            if (handler == null)
            {
                if (throwOnNotFound)
                    throw new KeyNotFoundException(string.Format(SR.HanderDoesNotExistErrorFormat, typeof(THandler).Name));
            }
            else
            {
                configure(handler);
            }

            return this;
        }

        #region Sending

        public TypedHttpCallHandlerRegister AddSendingHandler<TResult, TContent>(Action<TypedHttpSendingContext<TResult, TContent>> handler)
        {
            return AddSendingHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddSendingHandler<TResult, TContent>(HttpCallHandlerPriority priority, Action<TypedHttpSendingContext<TResult, TContent>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            return AddSendingHandler<TResult, TContent>(HttpCallHandlerPriority.Default, ctx => Task.Run(() => handler(ctx)));
        }

        public TypedHttpCallHandlerRegister AddSendingHandler<TResult, TContent>(Func<TypedHttpSendingContext<TResult, TContent>, Task> handler)
        {
            return AddSendingHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddSendingHandler<TResult, TContent>(HttpCallHandlerPriority priority, Func<TypedHttpSendingContext<TResult, TContent>, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            AddHandler(HttpCallHandlerType.Sending, priority, handler);

            return this;
        }

        #endregion

        #region Sent

        public TypedHttpCallHandlerRegister AddSentHandler<TResult>(Action<TypedHttpSentContext<TResult>> handler)
        {
            return AddSentHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddSentHandler<TResult>(HttpCallHandlerPriority priority, Action<TypedHttpSentContext<TResult>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            return AddSentHandler<TResult>(HttpCallHandlerPriority.Default, ctx => Task.Run(() => handler(ctx)));
        }

        public TypedHttpCallHandlerRegister AddSentHandler<TResult>(Func<TypedHttpSentContext<TResult>, Task> handler)
        {
            return AddSentHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddSentHandler<TResult>(HttpCallHandlerPriority priority, Func<TypedHttpSentContext<TResult>, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            AddHandler(HttpCallHandlerType.Sent, priority, handler);

            return this;
        }

        #endregion

        #region Result

        public TypedHttpCallHandlerRegister AddResultHandler<TResult>(Action<TypedHttpResultContext<TResult>> handler)
        {
            return AddResultHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddResultHandler<TResult>(HttpCallHandlerPriority priority, Action<TypedHttpResultContext<TResult>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            return AddResultHandler<TResult>(HttpCallHandlerPriority.Default, ctx => Task.Run(() => handler(ctx)));
        }

        public TypedHttpCallHandlerRegister AddResultHandler<TResult>(Func<TypedHttpResultContext<TResult>, Task> handler)
        {
            return AddResultHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddResultHandler<TResult>(HttpCallHandlerPriority priority, Func<TypedHttpResultContext<TResult>, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            AddHandler(HttpCallHandlerType.Result, priority, handler);

            return this;
        }

        #endregion

        #region Error

        public TypedHttpCallHandlerRegister AddErrorHandler<TError>(Action<TypedHttpCallErrorContext<TError>> handler)
        {
            return AddErrorHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddErrorHandler<TError>(HttpCallHandlerPriority priority, Action<TypedHttpCallErrorContext<TError>> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            return AddErrorHandler<TError>(HttpCallHandlerPriority.Default, ctx => Task.Run(() => handler(ctx)));
        }

        public TypedHttpCallHandlerRegister AddErrorHandler<TError>(Func<TypedHttpCallErrorContext<TError>, Task> handler)
        {
            return AddErrorHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddErrorHandler<TError>(HttpCallHandlerPriority priority, Func<TypedHttpCallErrorContext<TError>, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            AddHandler(HttpCallHandlerType.Error, priority, handler);

            return this;
        }

        #endregion

        #region Exception

        public TypedHttpCallHandlerRegister AddExceptionHandler(Action<TypedHttpCallExceptionContext> handler)
        {
            return AddExceptionHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddExceptionHandler(HttpCallHandlerPriority priority, Action<TypedHttpCallExceptionContext> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            return AddExceptionHandler(HttpCallHandlerPriority.Default, ctx => Task.Run(() => handler(ctx)));
        }

        public TypedHttpCallHandlerRegister AddExceptionHandler(Func<TypedHttpCallExceptionContext, Task> handler)
        {
            return AddExceptionHandler(HttpCallHandlerPriority.Default, handler);
        }

        public TypedHttpCallHandlerRegister AddExceptionHandler(HttpCallHandlerPriority priority, Func<TypedHttpCallExceptionContext, Task> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            AddHandler(HttpCallHandlerType.Exception, priority, handler);

            return this;
        }

        #endregion
    }
}