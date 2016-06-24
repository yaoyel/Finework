using System;
using System.Collections.Generic;
using FineWork.Common;
using JetBrains.Annotations;
using FineWork.Security.Models;
using System.IO;

namespace FineWork.Security
{
    /// <summary> Represents the service for account management. </summary>
    public interface IAccountManager
    {
        /// <summary> Register a new account with local password authentication. </summary>
        IAccount CreateAccount(CreateAccountModel createAccountModel);

        /// <summary> Register a new account with local password authentication. </summary>
        IAccount CreateAccount(String name, String password, String email, String phoneNumber);

        void UpdateAccount(IAccount account);

        void DeleteAccount(IAccount account);

        void ReloadAccount(IAccount account);

        IAccount FindAccount(Guid id);

        IAccount FindAccountByName(String accountName);

        IAccount FindAccountByPhoneNumber(String phoneNumber);

        ICollection<IAccount> FetchAccountsByEmail(String email);

        ICollection<IAccount> FetchAccounts();

        void ConfirmEmail(IAccount account);

        void ChangePassword(IAccount account, [CanBeNull] string oldPassword, [CanBeNull] String newPassword);

        /// <summary> Register a new account with external login authentication. </summary>
        ILogin CreateAccountWithLogin(String accountName, String email, String provider, String providerKey);

        /// <summary> Associates a new login to an existing account. </summary>
        ILogin CreateLogin(IAccount account, String provider, String providerKey);

        /// <summary> Dessociate an existing login. </summary>
        /// <exception cref="FineWorkException">if to remove the last login for an user without a local password.</exception>
        void DeleteLogin(Guid loginId);

        void ReloadLogin(ILogin login);

        ILogin FindLogin(Guid loginId);

        ILogin FindLogin(String provider, String providerKey);

        void UploadAccountAvatar(Stream stream, Guid accountId, string contentType);
        
    }

    public static class AccountManagerUtil
    {
        /// <summary> Delete the account with the specified name if it exists. </summary>
        /// <returns> The <see cref="IAccount"/> if it exists, or <c>null</c> if no such account. </returns>
        public static IAccount DeleteAccountByName(this IAccountManager accountManager, String accountName)
        {
            if (accountManager == null) throw new ArgumentNullException("accountManager");

            IAccount account = accountManager.FindAccountByName(accountName);
            if (account != null)
            {
                accountManager.DeleteAccount(account);
            }
            return account;
        }
    }
}
