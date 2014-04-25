﻿using System;
using System.Net;
using System.Net.Http;

namespace AonWeb.Fluent.Http.Handlers
{
    public class HttpRetryContext
    {
        public HttpStatusCode StatusCode { get; internal set; }
        public HttpRequestMessage RequestMessage { get; internal set; }
        public Uri Uri { get; internal set; }
        public bool ShouldRetry { get; set; }
        public int RetryAfter { get; internal set; }
    }
}