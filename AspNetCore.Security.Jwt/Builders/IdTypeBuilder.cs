﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;

namespace AspNetCore.Security.Jwt
{
    internal class IdTypeBuilder: IIdTypeBuilder, IIdTypeBuilderToClaims
    {
        private readonly List<Claim> claims = new List<Claim>();

        public IIdTypeBuilder AddClaim(string type, string value)
        {
            claims.Add(new Claim(type, value));

            return this;
        }

        public IIdTypeBuilder AddClaim(IdType idType, string value)
        {
            claims.Add(new Claim(idType.ToClaimTypes(), value));

            return this;
        }

        public List<Claim> ToClaims()
        {
            return this.claims;
        }
    }

    /// <inheritdoc />
    internal class IdTypeBuilder<TUserModel> : IIdTypeBuilder<TUserModel>, IIdTypeBuilderToClaims
        where TUserModel : class, IAuthenticationUser
    {
        private readonly List<Claim> claims = new List<Claim>();
        private readonly TUserModel user;

        public IdTypeBuilder(TUserModel user)
        {
            this.user = user;
        }        

        public IIdTypeBuilder<TUserModel> AddClaim(string type, string value)
        {
            claims.Add(new Claim(type, value));

            return this;
        }

        public IIdTypeBuilder<TUserModel> AddClaim(IdType idType, string value)
        {
            claims.Add(new Claim(idType.ToClaimTypes(), value));

            return this;
        }

        public IIdTypeBuilder<TUserModel> AddClaim(string idType, Func<TUserModel, string> value)
        {
            claims.Add(new Claim(idType, value(user)));

            return this;
        }

        public IIdTypeBuilder<TUserModel> AddClaim(IdType idType, Func<TUserModel, string> value)
        {
            claims.Add(new Claim(idType.ToClaimTypes(), value(user)));

            return this;
        }

        public IIdTypeBuilder<TUserModel> AddClaim(BinaryReader binaryReader)
        {
            claims.Add(new Claim(binaryReader));

            return this;
        }

        public List<Claim> ToClaims()
        {
            return this.claims;
        }
    }
}
