using System;

namespace AppBoot.Security.Passwords
{
    public sealed class NoPasswordAlgorithm : PasswordAlgorithmBase
    {
        protected override void DoTransform(string password, out string transformed, out string salt)
        {
            if (!String.IsNullOrEmpty(password)) throw new ArgumentException("Unexpected password", "password");
            transformed = null;
            salt = null;
        }

        protected override bool DoVerify(string transformed, string salt, string password)
        {
            return String.IsNullOrEmpty(transformed)
                && String.IsNullOrEmpty(salt)
                && String.IsNullOrEmpty(password);
        }
    }
}
