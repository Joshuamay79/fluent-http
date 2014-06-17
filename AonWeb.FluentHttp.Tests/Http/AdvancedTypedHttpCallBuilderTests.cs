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
    public class AdvancedTypedMockHttpCallBuilderTests
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

        public class SubTestResult : TestResult
        {
            public bool SubBoolProperty { get; set; }
        }

        #endregion

        [Test]
        [ExpectedException(typeof(TypeMismatchException))]
        public async Task WhenResultAndSendingHandlerTypesMismatch_ExpectException()
        {

            var builder = TypedHttpCallBuilder.Create(TestUriString);

            //act
            await builder.Advanced.OnSendingWithResult<Uri>(ctx => { }).ResultAsync<TestResult>();

            Assert.Fail();
        }

        [Test]
        public async Task WhenResultAndSendingTypesMismatchAndSuppressTypeException_ExpectResult()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var actual = await builder.Advanced.WithSuppressTypeMismatchExceptions().OnSendingWithResult<Uri>(ctx => { }).ResultAsync<TestResult>();

            Assert.NotNull(actual);
        }

        [Test]
        [ExpectedException(typeof(TypeMismatchException))]
        public async Task WhenContentAndSendingHandlerTypesMismatch_ExpectException()
        {

            var builder = TypedHttpCallBuilder.Create(TestUriString);

            //act
            await builder.WithContent(TestResultValue).AsPost().Advanced.OnSendingWithContent<Uri>(ctx => { }).ResultAsync<TestResult>();

            Assert.Fail();
        }

        [Test]
        public async Task WhenContentAndSendingHandlerTypesMismatchAndSuppressTypeException_ExpectResult()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var actual = await builder.WithContent(TestResultValue)
                .AsPost()
                .Advanced
                .WithSuppressTypeMismatchExceptions().OnSendingWithContent<Uri>(ctx => { }).ResultAsync<TestResult>();

            Assert.NotNull(actual);
        }

        [Test]
        [ExpectedException(typeof(TypeMismatchException))]
        public async Task WhenErrorAndErrorHandlerTypesMismatch_ExpectException()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithError(TestResultValue);

            //act
            await builder.WithErrorType<TestResult>().Advanced.OnError<Uri>(ctx => { }).ResultAsync<TestResult>();

            Assert.Fail();
        }

        [Test]
        public async Task WhenErrorAndErrorHandlerTypesMismatchAndSuppressTypeException_ExpectResult()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithError(TestResultValue);

            //act
            var actual = await builder
                .WithErrorType<TestResult>()
                .Advanced
                .WithExceptionFactory(context => null)
                .WithSuppressTypeMismatchExceptions()
                .OnError<Uri>(ctx => { })
                .ResultAsync<TestResult>();

            Assert.IsNull(actual);
        }

        [Test]
        [ExpectedException(typeof(TypeMismatchException))]
        public async Task WhenDefaultResultAndResultTypesMismatch_ExpectException()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithError(TestResultValue);

            //act
            await builder.WithDefaultResult(TestResultValue).Advanced.WithExceptionFactory(context => null).ResultAsync<Uri>();

            Assert.Fail();
        }

        [Test]
        public async Task WhenDefaultResultAndResultTypesMismatchAndSuppressTypeException_ExpectNullResult()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithError(TestResultValue);

            //act
            var actual = await builder.WithDefaultResult(TestResultValue)
                 .Advanced.WithSuppressTypeMismatchExceptions()
                 .WithExceptionFactory(context => null)
                 .ResultAsync<Uri>();

            Assert.IsNull(actual);
        }

        [Test]
        public async Task WhenHandlerIsSubTypeOfResult_ExpectSuccess()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResponse(new ResponseInfo { Body = TestResultString });

            //act
            var called = false;
            var result = await builder
                .Advanced
                .OnResult<TestResult>(ctx => { called = true; })
                .ResultAsync<SubTestResult>();

            Assert.NotNull(result);
            Assert.IsTrue(called);
        }

        [Test]
        [ExpectedException(typeof(TypeMismatchException))]
        public async Task WhenHandlerIsSuperTypeOfResult_ExpectException()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var actual = await builder
                .Advanced
                .OnResult<SubTestResult>(ctx => { })
                .ResultAsync<TestResult>();

            Assert.IsNull(actual);
        }

        [Test]
        public async Task WhenSendingContentHandlerIsObjectType_ExpectSuccess()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var called = false;
            var result = await builder.WithContent(TestResultValue)
                .AsPost()
                .Advanced
                .OnSendingWithContent<object>(ctx => { called = true; })
                .ResultAsync<TestResult>();

            Assert.NotNull(result);
            Assert.IsTrue(called);
        }

        [Test]
        public async Task WhenSendingResultHandlerIsObjectType_ExpectSuccess()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var called = false;
            var result = await builder.WithContent(TestResultValue)
                .AsPost()
                .Advanced
                .OnSendingWithResult<object>(ctx => { called = true; })
                .ResultAsync<TestResult>();

            Assert.NotNull(result);
            Assert.IsTrue(called);
        }

        [Test]
        public async Task WhenSentHandlerIsObjectType_ExpectSuccess()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var called = false;
            var result = await builder.WithContent(TestResultValue)
                .AsPost()
                .Advanced
                .OnSent<object>(ctx => { called = true; })
                .ResultAsync<TestResult>();

            Assert.NotNull(result);
            Assert.IsTrue(called);
        }

        [Test]
        public async Task WhenResultHandlerIsObjectType_ExpectSuccess()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var called = false;
            var result = await builder
                .Advanced
                .OnResult<object>(ctx => { called = true; })
                .ResultAsync<TestResult>();

            Assert.NotNull(result);
            Assert.IsTrue(called);
        }

        [Test]
        public async Task WhenErrorHandlerIsObjectType_ExpectSuccess()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithError(TestResultValue);

            //act
            var called = false;
            var result = await builder
                .Advanced
                .OnError<object>(ctx => { called = true; })
                .WithExceptionFactory(context => null)
                .ResultAsync<SubTestResult>();

            Assert.Null(result);
            Assert.IsTrue(called);
        }

        [Test]
        public async Task WhenErrorTypeSetMultipleTimes_ExpectLastWins()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithError(TestResultValue);

            //act
            Type type = null;
            TestResult error = null;
            var result = await builder
                .WithErrorType<Uri>()
                .WithErrorType<string>()
                .WithErrorType<TestResult>()
                .Advanced
                .OnError<object>(ctx =>
                {
                    type = ctx.ErrorType;
                    error = ctx.Error as TestResult;
                })
                .WithExceptionFactory(context => null)
                .ResultAsync<TestResult>();

            Assert.NotNull(error);
            Assert.Null(result);
            Assert.AreEqual(typeof(TestResult), type);
        }

        [Test]
        public async Task WhenDependentUriIsNull_ExpectNoException()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var result = await builder
                .WithDependentUri(null)
                .ResultAsync<TestResult>();

            Assert.NotNull(result);
        }

        [Test]
        public async Task WhenDependentUrisIsNull_ExpectNoException()
        {

            var builder = MockTypedHttpCallBuilder.CreateMock(TestUriString).WithResult(TestResultValue);

            //act
            var result = await builder
                .WithDependentUris(null)
                .ResultAsync<TestResult>();

            Assert.NotNull(result);
        }
    }
}