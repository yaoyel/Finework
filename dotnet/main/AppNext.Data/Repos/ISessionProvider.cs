namespace AppBoot.Repos
{
    //TODO: rename ISessionProvider to IDbSessionProvider
    /// <summary> Represents the provider of <see cref="ISession"/>. </summary>
    public interface ISessionProvider
    {
        /// <summary> Gets the session. </summary>
        /// <remarks> The session instance, or <c>null</c>.</remarks>
        ISession GetSession();
    }

    /// <summary> The generic provider for <see cref="ISession"/>. </summary>
    /// <typeparam name="TSession"> The <see cref="ISession"/> type. </typeparam>
    public interface ISessionProvider<out TSession>
        where TSession : class, ISession
    {
        /// <summary> Gets the session. </summary>
        /// <returns> The session instance, or <c>null</c>. </returns>
        TSession GetSession();
    }
}