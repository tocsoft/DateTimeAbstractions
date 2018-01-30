using System;
using System.Collections.Generic;
using System.Threading;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock
    {
        private static AsyncLocal<Stack<DateTimeProvider>> clockStack = new AsyncLocal<Stack<DateTimeProvider>>();

        public static DateTimeProvider DefaultProvider { get; set; } = new CurrentDateTimeProvider();

        static Clock()
        {
            clockStack.Value = new Stack<DateTimeProvider>();
        }

        private static DateTimeProvider Current
        {
            get
            {
                if (clockStack.Value.Count > 0)
                {
                    return clockStack.Value.Peek();
                }
                else
                {
                    return DefaultProvider;
                }
            }
        }

        public static IDisposable Pin()
        {
            return Pin(new StaticDateTimeProvider(Now));
        }

        internal static IDisposable Pin(DateTimeProvider provider)
        {
            clockStack.Value.Push(provider);
            return new PopWhenDisposed();
        }

        private static void Pop()
        {
            clockStack.Value.Pop();
        }

        private sealed class PopWhenDisposed : IDisposable
        {
            private bool disposed;

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                Pop();
                disposed = true;
            }
        }

        public static DateTime Now => Current.Now();
    }
}
