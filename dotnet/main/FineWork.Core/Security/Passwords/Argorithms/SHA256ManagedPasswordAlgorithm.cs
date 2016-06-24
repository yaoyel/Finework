using System;
using System.Security.Cryptography;

namespace AppBoot.Security.Passwords
{
    // ReSharper disable once InconsistentNaming
    public sealed class SHA256ManagedPasswordAlgorithm : HashedPasswordAlgorithm
    {
        public const String Name = "SHA256Managed";

        public SHA256ManagedPasswordAlgorithm()
            : base(new SHA256Managed())
        {
        }
    }
}