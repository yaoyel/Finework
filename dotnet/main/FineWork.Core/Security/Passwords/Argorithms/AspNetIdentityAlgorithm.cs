using System;
using AppBoot.Security.Crypto;

namespace AppBoot.Security.Passwords
{
    /// <summary> Represents the password algorithm used by ASP.NET Identity. </summary>
    public sealed class AspNetIdentityAlgorithm : PasswordAlgorithmBase
    {
        protected override void DoTransform(string password, out string transformed, out string salt)
        {
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("password is null or empty.", "password");
            transformed = Rfc2898Util.HashPassword(password);
            salt = null;
        }

        protected override bool DoVerify(string transformed, string salt, string password)
        {
            return Rfc2898Util.VerifyHashedPassword(transformed, password);
        }
    }
}