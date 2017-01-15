using System;
using System.Collections.Generic;

namespace Napack.Server.Utils
{
    /// <summary>
    /// Defines an <see cref="IComparer{T}"/> that can compute comparisons using a lambda instead of interface implementation.
    /// </summary>
    /// <typeparam name="T">The type being compared.</typeparam>
    internal class LambdaComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> lambdaFunction;

        /// <summary>
        /// Creates a new <see cref="LambdaComparer{T}"/>
        /// </summary>
        /// <param name="lambdaFunction">The function to use. Should return &lt; zero if the first parameter is less than the second, zero if they're equal, greater than zero otherwise.</param>
        public LambdaComparer(Func<T, T, int> lambdaFunction)
        {
            this.lambdaFunction = lambdaFunction;
        }

        public int Compare(T x, T y)
        {
            return lambdaFunction(x, y);
        }
    }
}
