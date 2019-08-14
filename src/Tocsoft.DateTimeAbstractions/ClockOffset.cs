// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Threading;
using Tocsoft.DateTimeAbstractions.Providers;

namespace Tocsoft.DateTimeAbstractions
{
    /// <summary>
    /// Provides pinnable and testable scoped access to DateTime static properties.
    /// </summary>
    public static class ClockOffset
    {
        private static AsyncLocal<ImmutableStack<DateTimeOffsetProvider>> clockStack = new AsyncLocal<ImmutableStack<DateTimeOffsetProvider>>();

        private static DateTimeOffsetProvider DefaultProvider { get; set; } = new CurrentDateTimeOffsetProvider();

        internal static DateTimeOffsetProvider CurrentProvider
        {
            get
            {
                clockStack.Value = clockStack.Value ?? ImmutableStack.Create<DateTimeOffsetProvider>();
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

        /// <summary>
        /// Pins the current date/time retuned to the current time until the disposable is disposed.
        /// </summary>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin()
        {
            return Pin(Now);
        }

        /// <summary>
        /// Pins the specified date/time retuned to the current time until the disposable is disposed.
        /// </summary>
        /// <param name="date">The date and time to the clocks time to.</param>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin(DateTimeOffset date)
        {
            return Pin(new StaticDateTimeOffsetProvider(date));
        }

        /// <summary>
        /// Pins the clock to call the function every time it needs access to the current time until the disposable is disposed.
        /// </summary>
        /// <param name="dateFunc">The function that returnes date and time to the clocks.</param>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin(Func<DateTimeOffset> dateFunc)
        {
            return Pin(new DelegateDateTimeOffsetProvider(dateFunc));
        }

        internal static IDisposable Pin(DateTimeOffsetProvider provider)
        {
            ImmutableStack<DateTimeOffsetProvider> stack = clockStack.Value ?? ImmutableStack.Create<DateTimeOffsetProvider>();
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
                if (this.disposed)
                {
                    return;
                }

                Pop();
                this.disposed = true;
            }
        }

        /// <summary>
        /// Gets the current local DateTimeOffset unless pinned then it will returned the pinned time as a local time.
        /// </summary>
        public static DateTimeOffset Now => CurrentProvider.UtcNow().ToLocalTime();

        /// <summary>
        /// Gets the current UTC DateTimeOffset unless pinned then it will returned the pinned time as a UTC time.
        /// </summary>
        public static DateTimeOffset UtcNow => CurrentProvider.UtcNow();
    }
}
