﻿using AonWeb.FluentHttp.Autofac;
using AonWeb.FluentHttp.Caching;
using AonWeb.FluentHttp.Tests.AutofacTests;
using Autofac;

namespace AonWeb.FluentHttp.Tests.Helpers
{
    public static class RegistrationHelpers
    {
        public static IContainer CreateContainer(bool singleInstanceCache = true)
        {
            var builder = new ContainerBuilder();

            if (singleInstanceCache)
                builder.RegisterType<CustomCacheProvider>()
                    .As<ICacheProvider>()
                    .InstancePerMatchingLifetimeScope(Constants.BuilderScopeTag);

            Registration.Register(builder, typeof(RegistrationHelpers).Assembly);
            return builder.Build();
        }
    }
}