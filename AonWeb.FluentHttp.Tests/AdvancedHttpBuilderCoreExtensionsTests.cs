﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using AonWeb.FluentHttp.Autofac;
using AonWeb.FluentHttp.Mocks.WebServer;
using AonWeb.FluentHttp.Tests.Helpers;
using Autofac;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AonWeb.FluentHttp.Tests
{
    [Collection("LocalWebServer Tests")]
    public class AdvancedHttpBuilderCoreExtensionsTests
    {
        private readonly ITestOutputHelper _logger;

        public AdvancedHttpBuilderCoreExtensionsTests(ITestOutputHelper logger)
        {
            _logger = logger;
            Cache.Clear();
        }

        private IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            Registration.Register(builder, GetType().Assembly);
            return builder.Build();
        }

        private IHttpBuilder CreateBuilder()
        {
            var container = CreateContainer();

            return container.Resolve<IHttpBuilder>();
        }


        #region WithMethod

        [Fact]
        public async Task WithMethod_WhenValidString_ExpectResultUsesMethod()
        {
            using (var server = LocalWebServer.ListenInBackground(new XUnitMockLogger(_logger)))
            {
                //arrange
                var method = HttpMethod.Get;
                var builder = CreateBuilder().WithUri(server.ListeningUri).Advanced.WithMethod(method);

                HttpMethod actual = null;
                server.WithRequestInspector(r => actual = r.Method);

                //act
                await builder.ResultAsync();

                method.ShouldBe(actual);
            }
        }

        [Fact]
        public void WithMethod_WhenNullString_ExpectException()
        {
            //arrange
            string method = null;

            //act
            Should.Throw<ArgumentException>(() => CreateBuilder().Advanced.WithMethod(method));
        }

        [Fact]
        public void WithMethod_WhenEmptyString_ExpectException()
        {
            //arrange
            var method = string.Empty;
            var builder = CreateBuilder();

            //act
            Should.Throw<ArgumentException>(() => builder.Advanced.WithMethod(method));
        }

        [Fact]
        public async Task WithMethod_WhenValidMethod_ExpectResultUsesMethod()
        {
            using (var server = LocalWebServer.ListenInBackground(new XUnitMockLogger(_logger)))
            {
                //arrange
                var method = HttpMethod.Get;
                var builder = CreateBuilder().WithUri(server.ListeningUri).Advanced.WithMethod(method);

                HttpMethod actual = null;
                server.WithRequestInspector(r => actual = r.Method);

                //act
                await builder.ResultAsync();

                method.ShouldBe(actual);
            }
        }

        [Fact]
        public void WithMethod_WhenNullMethod_ExpectException()
        {
            //arrange
            HttpMethod method = null;

            Should.Throw<ArgumentNullException>(() => CreateBuilder().Advanced.WithMethod(method));
        }

        [Fact]
        public async Task WithMethod_WhenCalledMultipleTimes_ExpectLastWins()
        {
            using (var server = LocalWebServer.ListenInBackground(new XUnitMockLogger(_logger)))
            {
                //arrange
                var method1 = HttpMethod.Post;
                var method2 = HttpMethod.Get;
                var builder = CreateBuilder().WithUri(server.ListeningUri).Advanced.WithMethod(method1).Advanced.WithMethod(method2);

                HttpMethod actual = null;
                server.WithRequestInspector(r => actual = r.Method);

                //act
                await builder.ResultAsync();

                method2.ShouldBe(actual);
            }
        }

        #endregion
        //IHttpBuilder WithScheme(string scheme);
        //IHttpBuilder WithHost(string host);
        //IHttpBuilder WithPort(int port);
    }
}