using System;

namespace AppBoot.Security.Passwords
{
    public abstract class PasswordAlgorithmBase : IPasswordAlgorithm
    {
        public void Transform(string password, out string transformed, out string salt)
        {
            DoTransform(password, out transformed, out salt);
        }

        public bool Verify(string transformed, string salt, string password)
        {
            return DoVerify(transformed, salt, password);
        }

        protected abstract void DoTransform(string password, out string transformed, out string salt);

        protected abstract bool DoVerify(String transformed, String salt, String password);
    }
}