﻿using System.Net.Http;

namespace AonWeb.FluentHttp.Handlers
{
    public class TypedHttpSendingContext<TResult, TContent> : TypedHttpCallHandlerContext
    {
        private readonly ModifyTracker<TResult> _result;

        public TypedHttpSendingContext(TypedHttpCallContext context, HttpRequestMessage request, TContent content, bool hasContent)
            : base(context)
        {
            Request = request;
            _result = new ModifyTracker<TResult>();
            Content = content;
            HasContent = hasContent;
        }

        public HttpRequestMessage Request { get; private set; }

        public TResult Result
        {
            get
            {
                return _result.Value;
            }
            internal set
            {
                _result.Value = value;
            }
        }

        public TContent Content{ get; private set; }
        public bool HasContent { get; private set; }

        public override ModifyTracker GetHandlerResult()
        {
            return _result.ToResult();
        } 
    }
}