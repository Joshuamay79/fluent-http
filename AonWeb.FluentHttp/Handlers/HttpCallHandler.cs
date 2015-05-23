using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace AonWeb.FluentHttp.Handlers
{
    
    public abstract class HttpCallHandler : IHttpCallHandler
    {
        private bool _enabled = true;

        public virtual bool Enabled
        {
            get{ return _enabled; } 
            set { _enabled = value; }
        }

        public virtual HttpCallHandlerPriority GetPriority(HttpCallHandlerType type)
        {
            return HttpCallHandlerPriority.Default;
        }

        public virtual Task OnSending(HttpSendingContext context)     { return Task.Delay(0); }
        public virtual Task OnSent(HttpSentContext context)           { return Task.Delay(0); }
        public virtual Task OnException(HttpExceptionContext context) { return Task.Delay(0); }
    }

    
    public abstract class TypedHttpCallHandler : ITypedHttpCallHandler
    {
        private bool _enabled = true;

        public virtual bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public virtual HttpCallHandlerPriority GetPriority(HttpCallHandlerType type)
        {
            return HttpCallHandlerPriority.Default;
        }

        public virtual Task OnSending<TResult, TContent>(TypedHttpSendingContext<TResult, TContent> context) { return Task.Delay(0); }
        public virtual Task OnSent<TResult>(TypedHttpSentContext<TResult> context) { return Task.Delay(0); }
        public virtual Task OnResult<TResult>(TypedHttpResultContext<TResult> context) { return Task.Delay(0); }
        public virtual Task OnError<TError>(TypedHttpErrorContext<TError> context) { return Task.Delay(0); }
        public virtual Task OnException(TypedHttpExceptionContext context) { return Task.Delay(0); }
    }

}