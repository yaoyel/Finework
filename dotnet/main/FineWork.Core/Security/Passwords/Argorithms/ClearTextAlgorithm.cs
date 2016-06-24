namespace AppBoot.Security.Passwords
{
    public sealed class ClearTextAlgorithm : PasswordAlgorithmBase
    {
        protected override void DoTransform(string password, out string transformed, out string salt)
        {
            transformed = password;
            salt = null;
        }

        protected override bool DoVerify(string transformed, string salt, string password)
        {
            return transformed == password;
        }
    }
}