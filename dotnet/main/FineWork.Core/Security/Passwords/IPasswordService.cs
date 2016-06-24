using System;

namespace FineWork.Security.Passwords
{
    public interface IPasswordService
    {
        void Transform(string passwordFormat, string password, out string transformed, out string salt);

        bool Verify(string passwordFormat, string password, string transformed, string passwordSalt);

        String[] GetSupportedFormats();
    }

    public static class PasswordServiceUtil
    {
        public static void SetPassword(this IPasswordService service, 
            IAccount account, String passwordFormat, String password)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (account == null) throw new ArgumentNullException("account");

            String transformed, salt;
            if (password != null)
            {
                service.Transform(passwordFormat, password, out transformed, out salt);
            }
            else
            {
                passwordFormat = PasswordFormats.NoPassword;
                transformed = null;
                salt = null;
            }

            account.PasswordFormat = passwordFormat;
            account.Password = transformed;
            account.PasswordSalt = salt;
        }

        public static bool Verify(this IPasswordService service,
            IAccount account, String password)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (account == null) throw new ArgumentNullException("account");

            return service.Verify(account.PasswordFormat, password, account.Password, account.PasswordSalt);
        }
    }
}