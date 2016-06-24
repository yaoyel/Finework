using System;
using System.Diagnostics;
using System.Linq;
using FineWork.Security.Passwords.Impls;
using NUnit.Framework;

namespace FineWork.Security.Passwords
{
    [TestFixture]
    public class PasswordUtilTests
    {
        private static readonly String[] m_Passwords = {" ", "abc12345", "中文密码"};

        [Test]
        public void TransformedPasswords_SHOULD_be_Verifiable()
        {
            IPasswordService ps = new PasswordService();

            var names = ps.GetSupportedFormats().Where(name => name != PasswordFormats.NoPassword).ToArray();
            foreach (var name in names)
            {
                foreach (var password in m_Passwords)
                {
                    try
                    {
                        String transformed, salt;
                        ps.Transform(name, password, out transformed, out salt);
                        bool isVerified = ps.Verify(name, password, transformed, salt);
                        Assert.IsTrue(isVerified, "Not verified for [{0}] with algorithm [{1}].",
                            password ?? "<null>", name);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Exception in Password [{0}] for [{1}].", password, name);
                        throw;
                    }
                }
            }
        }


        [Test]
        public void Transform_SHOULD_return_Empty_TransformedPassword_WHEN_format_is_NoPassword()
        {
            IPasswordService ps = new PasswordService();

            String transformed, salt;
            ps.Transform(PasswordFormats.NoPassword, null, out transformed, out salt);
            Assert.IsNull(transformed);
            Assert.IsNull(salt);
        }
    }
}
