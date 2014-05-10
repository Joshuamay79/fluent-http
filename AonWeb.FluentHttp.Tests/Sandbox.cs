﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace AonWeb.FluentHttp.Tests
{
    [TestFixture]
    public class Sandbox
    {
        public class TestClass
        {
            public bool TestProperty { get; set; }
        }

        [Test]
        [Ignore]
        public async Task CanCatchExceptionFromHttpClientSendAsync()
        {
            var caughtException = false;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://somedomain.com")
                {
                    Content = new ObjectContent<TestClass>(
                        new TestClass { TestProperty = true },
                        new JsonMediaTypeFormatter())
                };

                await new HttpClient().SendAsync(request);
            }
            catch (Exception)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);
        }

        [Test]
        public async Task CanCatchExceptionFromHttpClientSendAsyncWithCustomSerialization()
        {
            var caughtException = false;

            try
            {
    var content = await CreateContent(
        new TestClass(), 
        "application/json", 
        new JsonMediaTypeFormatter());

    var request = new HttpRequestMessage(HttpMethod.Get, "http://foo.com")
    {
        Content = content
    };

                await new HttpClient().SendAsync(request);
            }
            catch (Exception)
            {
                caughtException = true;
            }

            Assert.IsTrue(caughtException);
        }

    private static async Task<HttpContent> CreateContent<T>(
        T value,
        string mediaType, 
        MediaTypeFormatter formatter)
    {
        var type = typeof(T);
        var header = new MediaTypeHeaderValue(mediaType);

        HttpContent content;
        using (var stream = new MemoryStream())
        {
            await formatter.WriteToStreamAsync(type, value, stream, null, null);

            content = new ByteArrayContent(stream.ToArray());
        }

        formatter.SetDefaultContentHeaders(type, content.Headers, header);

        return content;
    }
    }
}
