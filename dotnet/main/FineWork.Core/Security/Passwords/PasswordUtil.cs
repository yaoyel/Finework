using System;

namespace FineWork.Security.Passwords
{
    public static class PasswordFormats
    {
        public const String NoPassword = "NoPassword";
        public const String ClearText = "ClearText";
        public const String TripleDES = "TripleDES";
        public const String SHA256 = "SHA256";
        public const String AspNetIdentity = "AspNetIdentity";
    }
}