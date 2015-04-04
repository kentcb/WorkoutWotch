namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable ContinueOnAnyContext(this Task @this)
        {
            @this.AssertNotNull(nameof(@this));
            return @this.ConfigureAwait(continueOnCapturedContext: false);
        }

        public static ConfiguredTaskAwaitable<T> ContinueOnAnyContext<T>(this Task<T> @this)
        {
            @this.AssertNotNull(nameof(@this));
            return @this.ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}