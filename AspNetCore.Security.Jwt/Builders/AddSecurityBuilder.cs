﻿using AspNetCore.Security.Jwt.AzureAD;
using AspNetCore.Security.Jwt.Facebook;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspNetCore.Security.Jwt
{
    /// <summary>
    /// AddSecurityBuilder class
    /// </summary>
    internal class AddSecurityBuilder : IAddSecurityBuilder
    {
        private static SecuritySettings SecuritySettings;
        private static bool IsJwtSchemeAdded = false;
        private static bool IsDefaultAdded = false;
        private static bool IsCustomAdded = false;
        private static bool IsFacebookAdded = false;
        private static bool IsAzureAdded = false;
        private static bool IsSwaggerAdded = false;
        private static IServiceCollection Services;

        public AddSecurityBuilder(SecuritySettings securitySettings, bool isJwtSchemeAdded, IServiceCollection services, bool addSwaggerSecurity = false)
        {
            SecuritySettings = securitySettings;
            IsJwtSchemeAdded = isJwtSchemeAdded;
            Services = services;

            if (!IsJwtSchemeAdded)
            {
                Services.AddJwtBearerScheme(SecuritySettings);
                IsJwtSchemeAdded = true;
            }

            if (addSwaggerSecurity && !IsSwaggerAdded)
            {
                Services.AddSecureSwaggerDocumentation();

                IsSwaggerAdded = true;
            }
        }

        public IAddSecurityBuilder AddAzureADSecurity()
        {
            if (!IsAzureAdded)
            {
                IdTypeHelpers.LoadClaimTypes();

                Services.AddSingleton(SecuritySettings);
                Services.AddSingleton<AzureADSecuritySettings>(SecuritySettings.AzureADSecuritySettings);
                Services.AddScoped<IAuthentication<AzureADAuthModel, AzureADResponseModel>, AzureAuthenticator>();                

                IsAzureAdded = true;
            }            

            return this;
        }

        public IAddSecurityBuilder AddFacebookSecurity(Action<IIdTypeBuilder<FacebookAuthModel>> addClaims = null)
        {
            if (!IsFacebookAdded)
            {
                IdTypeHelpers.LoadClaimTypes();

                Services.AddSingleton<BaseSecuritySettings>(SecuritySettings);
                if (addClaims != null)
                {
                    Services.AddSingleton<Action<IIdTypeBuilder<FacebookAuthModel>>>(x => addClaims);
                }
                Services.AddScoped<ISecurityService<FacebookAuthModel>, SecurityService<FacebookAuthModel>>();
                Services.AddScoped<IAuthentication<FacebookAuthModel>, FacebookAuthenticator>();                             

                IsFacebookAdded = true;
            }            

            return this;
        }

        IAddSecurityBuilder IAddSecurityBuilder.AddSecurity<TAuthenticator>()
        {
            if (!IsDefaultAdded && !IsCustomAdded)
            {
                IdTypeHelpers.LoadClaimTypes();

                Services.AddScoped<ISecurityService, SecurityService>();
                Services.AddScoped<IAuthentication, TAuthenticator>();                

                IsDefaultAdded = true;
            }            

            return this;
        }

        IAddSecurityBuilder IAddSecurityBuilder.AddSecurity<TAuthenticator, TUserModel>(Action<IIdTypeBuilder<TUserModel>> addClaims)
        {
            if (!IsDefaultAdded && !IsCustomAdded)
            {
                IdTypeHelpers.LoadClaimTypes();

                Services.AddSingleton(SecuritySettings);
                Services.AddSingleton<BaseSecuritySettings>(SecuritySettings);
                if (addClaims != null)
                {
                    Services.AddSingleton<Action<IIdTypeBuilder<TUserModel>>>(x => addClaims);
                }
                Services.AddScoped<ISecurityService<TUserModel>, SecurityService<TUserModel>>();
                Services.AddScoped<IAuthentication<TUserModel>, TAuthenticator>();                

                IsCustomAdded = true;
            }

            return this;
        }
    }
}