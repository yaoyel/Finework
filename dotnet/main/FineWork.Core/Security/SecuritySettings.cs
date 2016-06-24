using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using FineWork.Net.Mail;
using FineWork.Security.Passwords;
using FineWork.Settings;

namespace FineWork.Security
{
    public static class SecuritySettings
    {
        #region PasswordFormat

        private static readonly String DefaultPasswordFormat = PasswordFormats.SHA256;

        private static readonly String m_DefaultPasswordFormatId =
            SettingRegistry.Instance.Register("FineWork.Security.Account.PasswordFormat",
                DefaultPasswordFormat);

        public static String PasswordFormat(this ISettingManager settingManager)
        {
            return SettingUtil.GetAs<String>(settingManager, m_DefaultPasswordFormatId, DefaultPasswordFormat);
        }

        public static String SetPasswordFormat(this ISettingManager settingManager, String value)
        {
            return SettingUtil.SetAs<String>(settingManager, m_DefaultPasswordFormatId, value);
        }

        #endregion

        #region IsEmailReuseAllowed

        private static readonly bool DefaultIsEmailReuseAllowed = false;

        private static readonly String m_IsEmailReuseAllowedId =
            SettingRegistry.Instance.Register("FineWork.Security.Account.IsEmailReuseAllowed", 
            DefaultIsEmailReuseAllowed.ToString());

        /// <summary> 是否允许多个用户使用同样的邮箱. </summary>
        public static bool IsEmailReuseAllowed(this ISettingManager settingManager)
        {
            return SettingUtil.GetAs<Boolean>(settingManager, m_IsEmailReuseAllowedId, DefaultIsEmailReuseAllowed);
        }

        public static bool SetEmailReuseAllowed(this ISettingManager settingManager, bool value)
        {
            return SettingUtil.SetAs<Boolean>(settingManager, m_IsEmailReuseAllowedId, value);
        }

        #endregion

        #region IsEmailConfirmationEnabled

        private static readonly bool DefaultEmailConfirmationEnabled = true;

        private static readonly String m_IsEmailConfirmationEnabledId =
            SettingRegistry.Instance.Register("FineWork.Security.Account.IsEmailConfirmationEnabled", 
            DefaultEmailConfirmationEnabled.ToString());

        /// <summary> 是否启用邮箱验证. </summary>
        public static bool IsEmailConfirmationEnabled(this ISettingManager settingManager)
        {
            return SettingUtil.GetAs<Boolean>(settingManager, m_IsEmailConfirmationEnabledId, DefaultEmailConfirmationEnabled);
        }

        public static bool SetEmailConfirmationEnabled(this ISettingManager settingManager, bool value)
        {
            return SettingUtil.SetAs<Boolean>(settingManager, m_IsEmailConfirmationEnabledId, value);
        }

        #endregion

        #region EmailFromAddress

        /// <summary> 用于发送邮箱验证等邮件的账号. </summary>
        public static String GetEmailFromAddress(this ISettingManager settingManager)
        {
            return MailUtil.GetDefaultSmtpConfiguration(true).From;
        }

        #endregion

        #region EmailConfirmationTemplate

        private static readonly String DefaultEmailConfirmationTemplate =
            "请占击链接以验证邮箱: <a href=\"{0}\">link</a>";

        private static readonly String m_EmailConfirmationTemplateId =
            SettingRegistry.Instance.Register("FineWork.Security.Account.EmailConfirmationTemplate", 
            DefaultEmailConfirmationTemplate);

        public static String GetEmailConfirmationTemplate(this ISettingManager settingManager)
        {
            return SettingUtil.GetAs<String>(settingManager, m_EmailConfirmationTemplateId, DefaultEmailConfirmationTemplate);
        }

        public static String SetEmailConfirmationTemplate(this ISettingManager settingManager, String value)
        {
            return SettingUtil.SetAs<String>(settingManager, m_EmailConfirmationTemplateId, value);
        }

        #endregion

        public static readonly TimeSpan EmailConfirmationTimeSpan = TimeSpan.FromDays(1);
    }
}
