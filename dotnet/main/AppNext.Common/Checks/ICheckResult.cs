using System;

namespace AppBoot.Checks
{
    /// <summary> Represents the result of rule checking. </summary>
    /// <remarks> 
    /// <see cref="ICheckResult"/> encapsulate the rule checking result 
    /// that can be lately processed by various error handling strategies.
    /// </remarks>
    public interface ICheckResult
    {
        /// <summary> Indicates if the rule is checked successfully. </summary>
        bool IsSucceed { get; }

        /// <summary> Gets the message if the rule is checked unsuccessfully. </summary>
        String Message { get; }

        /// <summary> Creates an exception. </summary>
        Exception CreateException(String message);
    }
}