using System;

namespace AppBoot.Security.Passwords
{
    public interface IPasswordAlgorithm
    {
        void Transform(string password, out string transformed, out string salt);

        bool Verify(String transformed, String salt, String password);
    }
}