﻿using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AspNetCore.Security.Jwt.UnitTests")]
namespace AspNetCore.Security.Jwt
{
    using AspNetCore.Security.Jwt.AzureAD;
    using AspNetCore.Security.Jwt.Facebook;
    using AspNetCore.Security.Jwt.Google;
    using AspNetCore.Security.Jwt.Twitter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Text;

    /// <summary>
    /// SecurityExtensions static class
    /// </summary>
    public static class SecurityExtensions
    {
        private static bool IsJwtSchemeAdded = false;
        internal static bool IsSecurityUsed { get; set; }

        /// <summary>
        /// Add Security extensions - Used to wire up the dependency injection
        /// </summary>
        /// <typeparam name="TAuthenticator">The authenticator - Used to authenticate the default authentication</typeparam>
        /// <param name="services">The services collection</param>
        /// <param name="configuration">The configurations -- appsettings</param>
        /// <param name="addSwaggerSecurity">Enable security in Swagger UI</param>
        /// <returns>The services collection</returns>
        public static IServiceCollection AddSecurity<TAuthenticator>(this IServiceCollection services, 
                                                                        IConfiguration configuration, 
                                                                        bool addSwaggerSecurity = false)
            where TAuthenticator : class, IAuthentication
        {
            var securitySettings = configuration.SecuritySettings();
            IdTypeHelpers.LoadClaimTypes();

            services.AddSingleton(securitySettings);            
            services.AddScoped<ISecurityService, SecurityService>();
            services.AddScoped<IAuthentication, TAuthenticator>();
            services.AddScoped<IHttpClient, HttpClientHandler>();

            services.AddSwaggerAndJwtBearerScheme(addSwaggerSecurity, securitySettings);

            return services;
        }

        /// <summary>
        /// Add Security extensions - Used to wire up the dependency injection
        /// </summary>
        /// <typeparam name="TAuthenticator">The authenticator - Used to authenticate the custom User model</typeparam>
        /// <typeparam name="TUserModel">The custom User model</typeparam>
        /// <param name="services">The services collection</param>
        /// <param name="configuration">The configurations -- appsettings</param>
        /// <param name="addClaims">Add the Claims using the IdTypeBuilder (<see cref="IdTypeBuilder{TUserModel}"/>)</param>
        /// <param name="addSwaggerSecurity">Enable security in Swagger UI</param>
        /// <returns>The services collection</returns>
        public static IServiceCollection AddSecurity<TAuthenticator, TUserModel>(this IServiceCollection services,
                                                                IConfiguration configuration,
                                                                Action<IIdTypeBuilder<TUserModel>> addClaims = null,
                                                                bool addSwaggerSecurity = false)
            where TAuthenticator : class, IAuthentication<TUserModel>
            where TUserModel : class, IAuthenticationUser
        {
            var securitySettings = configuration.SecuritySettings();
            IdTypeHelpers.LoadClaimTypes();

            services.AddSingleton(securitySettings);
            services.AddSingleton<BaseSecuritySettings>(securitySettings);
            if (addClaims != null)
            {
                services.AddSingleton<Action<IIdTypeBuilder<TUserModel>>>(x => addClaims);
            }
            services.AddScoped<ISecurityService<TUserModel>, SecurityService<TUserModel>>();
            services.AddScoped<IAuthentication<TUserModel>, TAuthenticator>();
            services.AddScoped<IHttpClient, HttpClientHandler>();

            services.AddSwaggerAndJwtBearerScheme(addSwaggerSecurity, securitySettings);

            return services;
        }

        /// <summary>
        /// Add Facebook security extension
        /// </summary>
        /// <param name="services">The services collection</param>
        /// <param name="configuration">The configurations -- appsettings</param>
        /// <param name="addClaims">Add the Claims using the IdTypeBuilder (<see cref="IdTypeBuilder{TUserModel}"/>)</param>
        /// <param name="addSwaggerSecurity">Enable security in Swagger UI</param>
        /// <returns>The services collection</returns>
        public static IServiceCollection AddFacebookSecurity(this IServiceCollection services,
                                                                IConfiguration configuration,
                                                                Action<IIdTypeBuilder<FacebookAuthModel>> addClaims = null,
                                                                bool addSwaggerSecurity = false)
        {
            var securitySettings = configuration.SecuritySettings();
            IdTypeHelpers.LoadClaimTypes();

            services.AddSingleton(securitySettings);
            services.AddSingleton<BaseSecuritySettings>(securitySettings);
            if (addClaims != null)
            {
                services.AddSingleton<Action<IIdTypeBuilder<FacebookAuthModel>>>(x => addClaims);
            }
            services.AddScoped<ISecurityService<FacebookAuthModel>, SecurityService<FacebookAuthModel>>();           
            services.AddScoped<IAuthentication<FacebookAuthModel>, FacebookAuthenticator>();
            services.AddScoped<ISecurityClient<FacebookAuthModel, bool>, FacebookClient>();
            services.AddScoped<IHttpClient, HttpClientHandler>();

            services.AddSwaggerAndJwtBearerScheme(addSwaggerSecurity, securitySettings);

            return services;
        }

        /// <summary>
        /// Add Facebook security extension
        /// </summary>
        /// <param name="services">The services collection</param>
        /// <param name="configuration">The configurations -- appsettings</param>
        /// <param name="addSwaggerSecurity">Enable security in Swagger UI</param>
        /// <returns>The services collection</returns>
        public static IServiceCollection AddGoogleSecurity(this IServiceCollection services,
                                                                IConfiguration configuration,
                                                                bool addSwaggerSecurity = false)
        {
            var securitySettings = configuration.SecuritySettings();
            IdTypeHelpers.LoadClaimTypes();

            services.AddSingleton(securitySettings);

            services.AddSingleton<BaseSecuritySettings>(securitySettings);
            services.AddSingleton<GoogleSecuritySettings>(securitySettings.GoogleSecuritySettings);
            services.AddScoped<IAuthentication<GoogleAuthModel, GoogleResponseModel>, GoogleAuthenticator>();
            services.AddScoped<ISecurityClient<GoogleAuthModel, GoogleResponseModel>, GoogleClient>();
            services.AddScoped<IHttpClient, HttpClientHandler>();

            services.AddSwaggerAndJwtBearerScheme(addSwaggerSecurity, securitySettings);

            return services;
        }

        /// <summary>
        /// Add Facebook security extension
        /// </summary>
        /// <param name="services">The services collection</param>
        /// <param name="configuration">The configurations -- appsettings</param>
        /// <param name="addClaims">Add the Claims using the IdTypeBuilder (<see cref="IdTypeBuilder{TUserModel}"/>)</param>
        /// <param name="addSwaggerSecurity">Enable security in Swagger UI</param>
        /// <returns>The services collection</returns>
        public static IServiceCollection AddTwitterSecurity(this IServiceCollection services,
                                                                IConfiguration configuration,
                                                                bool addSwaggerSecurity = false)
        {
            var securitySettings = configuration.SecuritySettings();
            IdTypeHelpers.LoadClaimTypes();

            services.AddSingleton(securitySettings);

            services.AddSingleton<BaseSecuritySettings>(securitySettings);

            services.AddSingleton<TwitterSecuritySettings>(securitySettings.TwitterSecuritySettings);
            services.AddScoped<IAuthentication<TwitterAuthModel, TwitterResponseModel>, TwitterAuthenticator>();
            services.AddScoped<ISecurityClient<TwitterResponseModel>, TwitterClient>();
            services.AddScoped<IHttpClient, HttpClientHandler>();

            services.AddSwaggerAndJwtBearerScheme(addSwaggerSecurity, securitySettings);

            return services;
        }

        /// <summary>
        /// Add Azure AD security extension
        /// </summary>
        /// <param name="services">The services collection</param>
        /// <param name="configuration">The configurations -- appsettings</param>
        /// <param name="addSwaggerSecurity">Enable security in Swagger UI</param>
        /// <returns>The services collection</returns>
        public static IServiceCollection AddAzureADSecurity(this IServiceCollection services,
                                                                IConfiguration configuration,
                                                                bool addSwaggerSecurity = false)
        {
            var securitySettings = configuration.SecuritySettings();
            IdTypeHelpers.LoadClaimTypes();

            services.AddSingleton(securitySettings);
            services.AddSingleton<AzureADSecuritySettings>(securitySettings.AzureADSecuritySettings);            
            services.AddScoped<IAuthentication<AzureADAuthModel, AzureADResponseModel>, AzureAuthenticator>();
            services.AddScoped<ISecurityClient<AzureADResponseModel>, AzureClient>();
            services.AddScoped<IHttpClient, HttpClientHandler>();

            if (addSwaggerSecurity)
            {
                services.AddSecureSwaggerDocumentation();
            }            

            return services;
        } 

        internal static IServiceCollection AddJwtBearerScheme(this IServiceCollection services, BaseSecuritySettings securitySettings)
        {
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securitySettings.Secret)),

                    ValidateIssuer = true,
                    ValidIssuer = securitySettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = securitySettings.Audience,

                    ValidateLifetime = true, //validate the expiration and not before values in the token

                    ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
                };
            });

            return services;
        }

        /// <summary>
        /// Get security settings from appsettings.json
        /// </summary>
        /// <param name="configuration">The configuration<see cref="IConfiguration"/></param>
        /// <returns><see cref="SecuritySettings"/></returns>
        internal static SecuritySettings SecuritySettings(this IConfiguration configuration)
        {
            var securitySettings = new SecuritySettings();
            configuration.Bind("SecuritySettings", securitySettings);

            return securitySettings;
        }

        /// <summary>
        /// Add swagger and jwt bearer scheme
        /// </summary>
        /// <param name="services">The services collection</param>
        /// <param name="addSwaggerSecurity">Add swagger security</param>
        /// <param name="securitySettings">The security settings</param>
        internal static void AddSwaggerAndJwtBearerScheme(this IServiceCollection services, bool addSwaggerSecurity, SecuritySettings securitySettings)
        {
            if (addSwaggerSecurity)
            {
                services.AddSecureSwaggerDocumentation();
            }

            if (!IsJwtSchemeAdded)
            {
                services.AddJwtBearerScheme(securitySettings);
                IsJwtSchemeAdded = true;
            }
        }

        /// <summary>
        /// Add Security extension - Invokes the AddSecurityBuilder
        /// </summary>
        /// <param name="services">The services collection</param>
        /// <param name="settings">The settings <see cref="SecuritySettings"/></param>
        /// <param name="addSwaggerSecurity">Enable security in Swagger UI</param>
        /// <returns><see cref="IAddSecurityBuilder"/></returns>
        public static IAddSecurityBuilder AddSecurity(this IServiceCollection services, SecuritySettings settings, bool addSwaggerSecurity = false)
        {
            var securitySettings = settings;
            services.AddSingleton(securitySettings);

            AddSecurityBuilder.Create(securitySettings, IsJwtSchemeAdded, services, addSwaggerSecurity);

            IAddSecurityBuilder addSecurityBuilder = AddSecurityBuilder.TheInstance();

            return addSecurityBuilder;
        }        

        public static IApplicationBuilder UseSecurity(this IApplicationBuilder app, bool addSwaggerSecurity = false)
        {
            if (!IsSecurityUsed)
            {
                if (addSwaggerSecurity)
                {
                    app.UseSwaggerDocumentation();
                }
                app.UseAuthentication();

                IsSecurityUsed = true;
            }            

            return app;
        }
    }
}
