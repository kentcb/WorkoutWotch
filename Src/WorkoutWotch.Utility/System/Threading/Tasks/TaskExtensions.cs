using System;
using System.Runtime.CompilerServices;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace System.Threading.Tasks
{
    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable ContinueOnAnyContext(this Task @this)
        {
            @this.AssertNotNull("@this");
            return @this.ConfigureAwait(continueOnCapturedContext: false);
        }

        public static ConfiguredTaskAwaitable<T> ContinueOnAnyContext<T>(this Task<T> @this)
        {
            @this.AssertNotNull("@this");
            return @this.ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}

