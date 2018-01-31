using System;
using System.Collections.Generic;
using System.Threading;
using Tocsoft.DateTimeAbstractions.Providers;

namespace Tocsoft.DateTimeAbstractions
{
    public static class Clock
    {
        private static AsyncLocal<ImmutableStack<DateTimeProvider>> clockStack = new AsyncLocal<ImmutableStack<DateTimeProvider>>();

        public static DateTimeProvider DefaultProvider { get; set; } = new CurrentDateTimeProvider();

        public static DateTimeProvider CurrentProvider
        {
            get
            {
                clockStack.Value = clockStack.Value ?? ImmutableStack.Create<DateTimeProvider>();
                if (!clockStack.Value.IsEmpty)
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
            return Pin(Now);
        }
        public static IDisposable Pin(DateTime date)
        {
            return Pin(new StaticDateTimeProvider(date));
        }

        internal static IDisposable Pin(DateTimeProvider provider)
        {
            var stack = clockStack.Value ?? ImmutableStack.Create<DateTimeProvider>();
            clockStack.Value = stack.Push(provider);
            return new PopWhenDisposed();
        }

        private static void Pop()
        {
            clockStack.Value = clockStack.Value.Pop();
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

        public static DateTime Now => CurrentProvider.Now();
        public static DateTime Today => CurrentProvider.Now().Date;
        public static DateTime UtcNow => CurrentProvider.UtcNow();
    }
}
