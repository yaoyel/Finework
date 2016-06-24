using System;
using AppBoot.Security.Crypto;

namespace AppBoot.Security.Passwords
{
    // ReSharper disable once InconsistentNaming
    public sealed class TripleDESPasswordAlgorithm : SymmetricPasswordAlgorithmBase
    {
        public TripleDESPasswordAlgorithm(String key) :
            base(TripleDESUtil.CreateProvider(key))
        {
        }
    }
}