using ComparatorFileBranch.App.Definitions;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace ComparatorFileBranch.App
{
    public static class Assistant
    {
        public static string ApplicationFileName { get; } = typeof(Assistant).Assembly.Location;
        public static string ApplicationPath { get; } = Path.GetDirectoryName(ApplicationFileName);

        public static Func<Dispatcher> GetDispatcher;

        public static Action<Exception> AsyncError;
        public static Action<bool> ChangeMask;

        private static int _countMask = 0;
        private static object lock_countMask = new object();

        public static AsyncProcessorFn<TR> StartAsync<TR>(Func<TR> procesor)
        {
            var dispatcher = GetDispatcher();

            return new AsyncProcessorFn<TR>(procesor, GetDispatcher(), AsyncError).Start(() => 
            {
                lock(lock_countMask)
                {
                    _countMask++;
                    if (_countMask == 1)
                    {
                        dispatcher.Invoke(() =>
                        {
                            ChangeMask?.Invoke(true);
                        });
                    }
                }
            }, 
            () => 
            {
                lock (lock_countMask)
                {
                    _countMask--;
                    if (_countMask <= 0)
                    {
                        _countMask = 0;
                        dispatcher.Invoke(() =>
                        {
                            ChangeMask?.Invoke(false);
                        });
                    }
                }
            });
        }

        public static AsyncProcessorFn<int> StartAsync(Action procesor)
        {
            return StartAsync<int>(() =>
            {
                procesor?.Invoke();
                return 0;
            });
        }
    }
}
