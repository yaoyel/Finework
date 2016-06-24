using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace AppBoot.Common
{
    public static class MoqUtil
    {
        #region CheckCall for Action => Expression<Action<T>>

        public static void CheckCall<T>(this Mock<T> mock,
            Action caller, Expression<Action<T>> expression
            ) where T : class
        {
            CheckCall(mock, caller, expression, Times.Once);
        }

        public static void CheckCall<T>(this Mock<T> mock,
            Action caller, Expression<Action<T>> expression,
            Func<Times> times) where T : class
        {
            if (mock == null) throw new ArgumentNullException("mock");
            if (caller == null) throw new ArgumentNullException("caller");

            mock.Setup(expression).Verifiable();
            caller();
            mock.Verify(expression, times);
        }

        #endregion

        #region CheckCall for Func<TResult> => Expression<Func<T, TResult>>

        public static TResult CheckCall<T, TResult>(this Mock<T> mock,
            Func<TResult> caller, Expression<Func<T, TResult>> expression,
            TResult returns) where T : class
        {
            return CheckCall(mock, caller, expression, returns, Times.Once);
        }

        public static TResult CheckCall<T, TResult>(this Mock<T> mock,
            Func<TResult> caller, Expression<Func<T, TResult>> expression,
            TResult returns, Func<Times> times) where T : class
        {
            if (mock == null) throw new ArgumentNullException("mock");
            if (caller == null) throw new ArgumentNullException("caller");
            if (expression == null) throw new ArgumentNullException("expression");

            mock.Setup(expression).Returns(returns).Verifiable();
            var result = caller();
            mock.Verify(expression, times);
            return result;
        }

        #endregion

        #region CheckCall for Action => Expression<Func<T, Task>>

        public static void CheckCall<T>(this Mock<T> mock,
            Action caller, Expression<Func<T, Task>> expression
            ) where T : class
        {
            CheckCall(mock, caller, expression, Times.Once);
        }

        public static void CheckCall<T>(this Mock<T> mock,
            Action caller, Expression<Func<T, Task>> expression,
            Func<Times> times) where T : class
        {
            if (mock == null) throw new ArgumentNullException("mock");
            if (caller == null) throw new ArgumentNullException("caller");

            mock.Setup(expression).Returns(Task.FromResult(0)).Verifiable();
            caller();
            mock.Verify(expression, times);
        }

        #endregion

        #region CheckCall for Func<TResult> => Expression<Func<T, Task<TResult>>

        public static TResult CheckCall<T, TResult>(this Mock<T> mock,
            Func<TResult> caller, Expression<Func<T, Task<TResult>>> expression,
            TResult returns) where T : class
        {
            return CheckCall(mock, caller, expression, returns, Times.Once);
        }

        public static TResult CheckCall<T, TResult>(this Mock<T> mock,
            Func<TResult> caller, Expression<Func<T, Task<TResult>>> expression,
            TResult returns, Func<Times> times) where T : class
        {
            if (mock == null) throw new ArgumentNullException("mock");
            if (caller == null) throw new ArgumentNullException("caller");
            if (expression == null) throw new ArgumentNullException("expression");

            mock.Setup(expression).Returns(Task.FromResult(returns)).Verifiable();
            var result = caller();
            mock.Verify(expression, times);
            return result;
        }

        #endregion

        /// <summary> Specifies a sequence of return values. </summary>
        /// <remarks> <see cref="ReturnsSequence{T, TResult}"/>
        /// supports setup a sequence of return values and then make it 
        /// <see cref="IVerifies.Verifiable()"/> in a fluent way:
        /// <code>
        /// someMock.Setup(someLambda).ReturnsSequence(a, b).Verifiable();
        /// </code>
        /// The Moq framework's <see cref="ISetupSequentialResult{TResult}"/> 
        /// does NOT implement <see cref="IVerifies"/>, hence we have to 
        /// write another line of code for <see cref="IVerifies.Verifiable()"/>:
        /// <code>
        /// someMock.SetupSequence(someLambda).Returns(a).Returns(b);
        /// someMock.Verifiable();
        /// </code>
        /// </remarks>
        public static IReturnsResult<T> ReturnsSequence<T, TResult>(
            this ISetup<T, TResult> setup, params TResult[] results) where T : class
        {
            return setup.Returns(new Queue<TResult>(results).Dequeue);
        }
    }
}