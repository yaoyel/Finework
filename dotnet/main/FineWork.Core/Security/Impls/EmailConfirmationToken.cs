using System;
using System.Globalization;

namespace FineWork.Security.Impls
{
    public class EmailConfirmationToken
    {
        public EmailConfirmationToken(DateTime generatedAt)
        {
            this.GeneratedAt = generatedAt;
        }

        public DateTime GeneratedAt { get; private set; }

        internal const String DateTimeFormat = "yyyyMMddHHmmss";

        public String ToUriQueryArg()
        {
            return GeneratedAt.ToString(DateTimeFormat);
        }

        public String GenerateVerificationCode(Guid accountId)
        {
            return accountId.ToString("D") + this.ToUriQueryArg();
        }

        public bool IsBefore(TimeSpan timeSpan)
        {
            return DateTime.Now.Subtract(this.GeneratedAt) > timeSpan;
        }

        public static EmailConfirmationToken Create(IAccount account)
        {
            EmailConfirmationToken token = new EmailConfirmationToken(DateTime.Now);
            return token;
        }

        public static EmailConfirmationToken Parse(String s)
        {
            if (String.IsNullOrEmpty(s)) throw new ArgumentException("s is null or empty.", "s");

            DateTime generatedAt = DateTime.ParseExact(s, EmailConfirmationToken.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
            EmailConfirmationToken token = new EmailConfirmationToken(generatedAt);
            return token;
        }
    }
}
