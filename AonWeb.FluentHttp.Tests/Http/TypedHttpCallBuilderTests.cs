﻿using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using AonWeb.FluentHttp.Exceptions;
using AonWeb.FluentHttp.Mocks;
using AonWeb.FluentHttp.Serialization;
using AonWeb.FluentHttp.Tests.Helpers;
using NUnit.Framework;

namespace AonWeb.FluentHttp.Tests.Http
{
    [TestFixture]
    public class TypedMockHttpCallBuilderTests
    {
        #region Declarations, Set up, & Tear Down

        private const string TestUriString = LocalWebServer.DefaultListenerUri;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            HttpCallBuilderDefaults.CachingEnabled = false;
        }

        #endregion

        #region Test Classes

        public static string TestResultString = @"{""StringProperty"":""TestString"",""IntProperty"":2,""BoolProperty"":true,""DateOffsetProperty"":""2000-01-01T00:00:00-05:00"",""DateProperty"":""2000-01-01T00:00:00""}";
        public static TestResult TestResultValue = new TestResult();

        public class TestResult : IEquatable<TestResult>
        {
            public TestResult()
            {
                StringProperty = "TestString";
                IntProperty = 2;
                BoolProperty = true;
                DateOffsetProperty = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromHours(-5));
                DateProperty = new DateTime(2000, 1, 1, 0, 0, 0);
            }

            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public bool BoolProperty { get; set; }
            public DateTimeOffset DateOffsetProperty { get; set; }
            public DateTime DateProperty { get; set; }

            #region Equality

            public bool Equals(TestResult other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }
                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return DateProperty.Equals(other.DateProperty)
                    && DateOffsetProperty.Equals(other.DateOffsetProperty)
                    && BoolProperty.Equals(other.BoolProperty)
                    && IntProperty == other.IntProperty
                    && string.Equals(StringProperty, other.StringProperty);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj.GetType() != this.GetType())
                {
                    return false;
                }
                return Equals((TestResult)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = DateProperty.GetHashCode();
                    hashCode = (hashCode * 397) ^ DateOffsetProperty.GetHashCode();
                    hashCode = (hashCode * 397) ^ BoolProperty.GetHashCode();
                    hashCode = (hashCode * 397) ^ IntProperty;
                    hashCode = (hashCode * 397) ^ StringProperty.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(TestResult left, TestResult right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(TestResult left, TestResult right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        #endregion

        #region Results

        [Test]
        public void WhenComplexTypedGet_WithValidResponse_ExpectValidDeserializedResult()
        {

                var builder = MockHttpCallBuilder<TestResult, EmptyRequest, EmptyError>.CreateMock(TestUriString);
                //arrange
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.OK,
                    Body = TestResultString
                });



                //act
                var actual = builder.ResultAsync().Result;

                Assert.AreEqual(TestResultValue, actual);
        }

        [Test]
        public void WhenSimpleTypedGet_WithValidResponse_ExpectValidDeserializedResult()
        {
            var builder = MockHttpCallBuilder<bool, EmptyRequest, EmptyError>.CreateMock(TestUriString);

                //arrange
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.OK,
                    Body = "true"
                });


                //act
                var actual = builder.ResultAsync().Result;

                Assert.IsTrue(actual);
        }

        [Test]
        public void WhenStringTypedGet_WithValidResponse_ExpectValidDeserializedResult()
        {
            var builder = MockHttpCallBuilder<string, EmptyRequest, EmptyError>.CreateMock(TestUriString);

                //arrange
                var expected = "some string data";
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.OK,
                    Body = "\"" + expected + "\""
                });


                //act
                var actual = builder.ResultAsync().Result;

                Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhenStringTypedGet_WithPlainTextResponseResponse_ExpectValidDeserializedResult()
        {
            var builder = MockHttpCallBuilder<string, EmptyRequest, EmptyError>.CreateMock(TestUriString);

            //arrange
            var expected = "some string data";
            builder.WithResponse(new ResponseInfo
            {
                ContentEncoding = Encoding.UTF8,
                ContentType = "text/plain",
                StatusCode = HttpStatusCode.OK,
                Body = expected
            });


            //act
            var actual = builder.ResultAsync().Result;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhenEmptyStringTypedGet_WithValidResponse_ExpectValidDeserializedResult()
        {
            var builder = MockHttpCallBuilder<string, EmptyRequest, EmptyError>.CreateMock(TestUriString);

                //arrange
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.OK
                });


                //act
                var actual = builder.ResultAsync().Result;

                Assert.IsNullOrEmpty(actual);
        }

        [Test]
        public void WhenEmptyTypedGet_WithValidResponse_ExpectValidDeserializedResult()
        {
            var builder = MockHttpCallBuilder<TestResult, EmptyRequest, EmptyError>.CreateMock(TestUriString);

                //arrange
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.OK
                });


                //act
                var actual = builder.ResultAsync().Result;

                Assert.IsNull(actual);
        }

        #endregion

        #region Content

        [Test]
        public void WhenPostingComplexType_ExpectRequestContentSerializedCorrectly()
        {
            var builder = MockHttpCallBuilder<EmptyResult, TestResult, EmptyError>.CreateMock(TestUriString);

                var expected = TestResultString;
                builder.WithResponse(new ResponseInfo());

                //arrange
                string actual = null;
                builder.OnSending(ctx => actual = ctx.Request.Content.ReadAsStringAsync().Result);

                //act
                builder.WithContent(() => TestResultValue).AsPost().SendAsync().Wait();

                Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhenPuttingSimpleTyped_ExpectRequestContentSerializedCorrectly()
        {
            var builder = MockHttpCallBuilder<EmptyResult, bool, EmptyError>.CreateMock(TestUriString);

                //arrange
                var expected = "true";
                builder.WithResponse(new ResponseInfo());

                string actual = null;
                builder.OnSending(ctx => actual = ctx.Request.Content.ReadAsStringAsync().Result);

                //act
                builder.WithContent(() => true).AsPut().SendAsync().Wait();

                Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task WhenPostingStringType_ExpectRequestContentSerializedCorrectly()
        {
            var builder = MockHttpCallBuilder<EmptyResult, string, EmptyError>.CreateMock(TestUriString);

                //arrange
                var expected = "some string data";
                builder.WithResponse(new ResponseInfo());

                string actual = null;
                builder.OnSending(ctx => actual = ctx.Request.Content.ReadAsStringAsync().Result);

                //act
                var result = await builder.WithContent(() => expected).AsPost().ResultAsync();

                Assert.AreEqual("\"" + expected + "\"", actual);
        }

        [Test]
        public void WhenPostingEmptyStringType_ExpectRequestContentSerializedCorrectly()
        {
            var builder = MockHttpCallBuilder<EmptyResult, string, EmptyError>.CreateMock(TestUriString);

                //arrange
                var expected = "null";
                builder.WithResponse(new ResponseInfo());

                string actual = null;
                builder.OnSending(ctx => actual = ctx.Request.Content.ReadAsStringAsync().Result);

                //act
                builder.WithContent(() => null).AsPost().SendAsync().Wait();

                Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WhenPostingEmptyType_ExpectRequestContentSerializedCorrectly()
        {
            var builder = MockHttpCallBuilder<EmptyResult, TestResult, EmptyError>.CreateMock(TestUriString);

                var expected = "null";
                builder.WithResponse(new ResponseInfo());

                //arrange
                string actual = null;
                builder.OnSending(ctx => actual = ctx.Request.Content.ReadAsStringAsync().Result);

                //act
                builder.WithContent(() => null).AsPost().SendAsync().Wait();

                Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Errors

        [Test]
        public async Task WhenCallFailsAndErrorIsComplexType_ExpectRequestContentSerializedCorrectly()
        {
            var builder = MockHttpCallBuilder<EmptyResult, EmptyRequest, TestResult>.CreateMock(TestUriString);

                //arrange
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.BadRequest,
                    Body = TestResultString
                });

                //act
                try
                {
                    await builder.ResultAsync();
                    Assert.Fail();
                }
                catch (HttpErrorException<TestResult> ex)
                {
                    Assert.AreEqual(TestResultValue, ex.Error);
                }
        }

        [Test]
        public async Task WhenCallFailsAndErrorIsSimpleTyped_ExpectExceptionWithCorrectlyDeserializedError()
        {
            var builder = MockHttpCallBuilder<EmptyResult, EmptyRequest, bool>.CreateMock(TestUriString);

                //arrange
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.BadRequest,
                    Body = "true"
                });

                //act
                try
                {
                    await builder.ResultAsync();
                    Assert.Fail();
                }
                catch (HttpErrorException<bool> ex)
                {
                    Assert.IsTrue(ex.Error);
                }
        }

        [Test]
        public async Task WhenCallFailsAndErrorIsStringType_ExpectExceptionWithCorrectlyDeserializedError()
        {
            var builder = MockHttpCallBuilder<EmptyResult, EmptyRequest, string>.CreateMock(TestUriString);

                //arrange
                var expected = "some string data";
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.BadRequest,
                    Body = "\"" + expected + "\""
                });

                //act
                try
                {
                    await builder.ResultAsync();
                    Assert.Fail();
                }
                catch (HttpErrorException<string> ex)
                {
                    Assert.AreEqual(expected, ex.Error);
                }
        }

        [Test]
        public async Task WhenCallFailsAndErrorIsEmptyStringType_ExpectExceptionWithCorrectlyDeserializedError()
        {
            var builder = MockHttpCallBuilder<EmptyResult, EmptyRequest, string>.CreateMock(TestUriString);

                //arrange

                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.BadRequest,
                    Body = null
                });

                //act
                try
                {
                    await builder.ResultAsync();
                    Assert.Fail();
                }
                catch (HttpErrorException<string> ex)
                {
                    Assert.IsNullOrEmpty(ex.Error);
                }
        }

        [Test]
        public async Task WhenCallFailsAndErrorIsEmptyType_ExpectExceptionWithCorrectlyDeserializedError()
        {
            var builder = MockHttpCallBuilder<EmptyResult, EmptyRequest, TestResult>.CreateMock(TestUriString);

                //arrange
                builder.WithResponse(new ResponseInfo
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    StatusCode = HttpStatusCode.BadRequest,
                    Body = null
                });

                //act
                try
                {
                    await builder.ResultAsync();
                    Assert.Fail();
                }
                catch (HttpErrorException<TestResult> ex)
                {
                    Assert.IsNull(ex.Error);
                }
        }

        #endregion
    }
}
