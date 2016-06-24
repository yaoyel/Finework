using System;
using System.Security.Cryptography;
using System.Text;
using AppBoot.Common;
using AppBoot.Security.Crypto;

namespace AppBoot.Security.Passwords
{
    public abstract class HashedPasswordAlgorithm : PasswordAlgorithmBase
    {
        protected HashedPasswordAlgorithm(HashAlgorithm hashAlgorithm)
            :base()
        {
            if (hashAlgorithm == null) throw new ArgumentNullException("hashAlgorithm");
            this.HashAlgorithm = hashAlgorithm;
        }

        protected HashAlgorithm HashAlgorithm { get; private set; }

        protected override void DoTransform(string password, out string transformed, out string salt)
        {
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("password is null or empty.", "password");
            var saltBytes = CryptoUtil.CreateRandomBytes(8);
            var hashBytes = ComputeHash(password, saltBytes);

            salt = Convert.ToBase64String(saltBytes);
            transformed = Convert.ToBase64String(hashBytes);
        }

        protected override bool DoVerify(string transformed, string salt, string password)
        {
            if (transformed == null) throw new ArgumentNullException("transformed");
            if (salt == null) throw new ArgumentNullException("salt");

            var saltBytes = Convert.FromBase64String(salt);
            var hashBytes = ComputeHash(password, saltBytes);

            var expectedHashBytes = Convert.FromBase64String(transformed);
            return CollectionUtil.ArrayEquals(expectedHashBytes, hashBytes);
        }
        
        protected virtual byte[] ComputeHash(String password, byte[] saltBytes)
        {
            if (saltBytes == null) throw new ArgumentNullException("saltBytes");
            if (password == null) throw new ArgumentNullException("password");

            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var dataBytes = Concat(saltBytes, passwordBytes);
            var hashBytes = ComputeHashCore(dataBytes);

            return hashBytes;
        }

        protected virtual byte[] ComputeHashCore(byte[] data)
        {
            return HashAlgorithm.ComputeHash(data);
        }

        private static byte[] Concat(Byte[] a, Byte[] b)
        {
            if (a == null) throw new ArgumentNullException("a");
            if (b == null) throw new ArgumentNullException("b");
            
            var result = new byte[a.Length + b.Length];
            Buffer.BlockCopy(a, 0, result, 0, a.Length);
            Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
            return result;
        }
    }
}