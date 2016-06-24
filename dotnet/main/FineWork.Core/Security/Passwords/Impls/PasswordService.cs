using System;
using AppBoot.Security.Passwords;

namespace FineWork.Security.Passwords.Impls
{
    public class PasswordService : IPasswordService
    {
        private static readonly PasswordAlgorithmRegistry m_Registry = CreateDefault();

        private static PasswordAlgorithmRegistry CreateDefault()
        {
            var result = new PasswordAlgorithmRegistry();

            result.Register(PasswordFormats.NoPassword, () => new NoPasswordAlgorithm());
            result.Register(PasswordFormats.ClearText, () => new ClearTextAlgorithm());
            result.Register(PasswordFormats.TripleDES, () => new TripleDESPasswordAlgorithm(GetSymmetricPasswordKey()));
            result.Register(PasswordFormats.SHA256, () => new SHA256ManagedPasswordAlgorithm());
            result.Register(PasswordFormats.AspNetIdentity, () => new AspNetIdentityAlgorithm());
            return result;
        }

        public void Transform(string passwordFormat, string password, out string transformed, out string salt)
        {
            IPasswordAlgorithm algorithm = m_Registry.Lookup(passwordFormat, true);
            algorithm.Transform(password, out transformed, out salt);
        }

        public bool Verify(string passwordFormat, string password, string transformed, string passwordSalt)
        {
            IPasswordAlgorithm algorithm = m_Registry.Lookup(passwordFormat, true);
            return algorithm.Verify(transformed, passwordSalt, password);
        }

        public String[] GetSupportedFormats()
        {
            return m_Registry.GetNames();
        }

        private static String GetSymmetricPasswordKey()
        {
            return "abcdefgh"; //todo: get the key from configuration
        }
    }
}