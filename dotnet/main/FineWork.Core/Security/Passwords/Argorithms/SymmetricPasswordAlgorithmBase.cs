using System;
using System.Security.Cryptography;
using AppBoot.Security.Crypto;

namespace AppBoot.Security.Passwords
{
    public abstract class SymmetricPasswordAlgorithmBase : PasswordAlgorithmBase
    {
        protected SymmetricPasswordAlgorithmBase(SymmetricAlgorithm symmetricAlgorithm)
            :base()
        {
            if (symmetricAlgorithm == null) throw new ArgumentNullException("symmetricAlgorithm");
            this.SymmetricAlgorithm = symmetricAlgorithm;
        }

        protected SymmetricAlgorithm SymmetricAlgorithm { get; private set; }

        protected override void DoTransform(string password, out string transformed, out string salt)
        {
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("password is null or empty.", "password");
            var transformedBytes = CryptoUtil.Encrypt(password, this.SymmetricAlgorithm.CreateEncryptor());
            transformed = Convert.ToBase64String(transformedBytes);
            salt = null;
        }

        protected override bool DoVerify(string transformed, string salt, string password)
        {
            if (transformed == null) throw new ArgumentNullException("transformed");

            var transformedBytes = Convert.FromBase64String(transformed);
            var decrypted = CryptoUtil.Decrypt(transformedBytes, this.SymmetricAlgorithm.CreateDecryptor());
            return password.Equals(decrypted);
        }
    }
}