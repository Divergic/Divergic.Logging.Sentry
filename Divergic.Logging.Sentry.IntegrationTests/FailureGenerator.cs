namespace Divergic.Logging.Sentry.IntegrationTests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class FailureGenerator
    {
        private static readonly Dictionary<int, int> _dict = new Dictionary<int, int>();

        public static async Task Execute()
        {
            var value = await MethodAsync(1, 2, 3, 4);

            Trace.WriteLine(value);
        }

        private static async Task<int> MethodAsync(int v0, int v1, int v2, int v3)
            => await MethodAsync(v0, v1, v2);

        private static async Task<int> MethodAsync(int v0, int v1, int v2)
            => await MethodAsync(v0, v1);

        private static async Task<int> MethodAsync(int v0, int v1)
            => await MethodAsync(v0);

        private static async Task<int> MethodAsync(int v0)
            => await MethodAsync();

        private static async Task<int> MethodAsync()
        {
            await Task.Delay(1000);

            var value = 0;

            foreach (var i in Sequence(0, 5))
            {
                value += i;
            }

            return value;
        }

        private static IEnumerable<int> Sequence(int start, int end)
        {
            for (var i = start; i <= end; i++)
            {
                foreach (var item in Sequence(i))
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable<int> Sequence(int start)
        {
            var end = start + 10;
            for (var i = start; i <= end; i++)
            {
                _dict[i] = _dict[i] + 1; // Throws exception
                yield return i;
            }
        }
    }
}