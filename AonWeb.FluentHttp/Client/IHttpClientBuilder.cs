﻿using AonWeb.FluentHttp.Settings;

namespace AonWeb.FluentHttp.Client
{
    public interface IHttpClientBuilder : 
        IFluentConfigurable<IHttpClientBuilder, IHttpClientSettings>,
        IFluentConfigurable<IHttpClientBuilder, IHttpClient>
    {
        /// <summary>
        /// Builds configured instance of <see cref="IHttpClient"/> using the specified settings.
        /// </summary>
        /// <returns>An configured <see cref="IHttpClient"/>.</returns>
        IHttpClient Build();
    }
}