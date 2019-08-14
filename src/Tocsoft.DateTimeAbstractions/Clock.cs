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
    public static class Clock
    {
        private static AsyncLocal<ImmutableStack<DateTimeProvider>> clockStack = new AsyncLocal<ImmutableStack<DateTimeProvider>>();

        private static DateTimeProvider DefaultProvider { get; set; } = new CurrentDateTimeProvider();

        internal static DateTimeProvider CurrentProvider
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
        /// <param name="date">The date and time to the clock.</param>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin(DateTime date)
        {
            return Pin(new StaticDateTimeProvider(date));
        }

        /// <summary>
        /// Pins the clock to call the function every time it needs access to the current time until the disposable is disposed.
        /// </summary>
        /// <param name="dateFunc">The function that returnes date and time to the clocks.</param>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin(Func<DateTime> dateFunc)
        {
            return Pin(new DelegateDateTimeProvider(dateFunc));
        }

        internal static IDisposable Pin(DateTimeProvider provider)
        {
            ImmutableStack<DateTimeProvider> stack = clockStack.Value ?? ImmutableStack.Create<DateTimeProvider>();
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
        /// Gets the current local DateTime unless pinned then it will returned the pinned time as a local time.
        /// </summary>
        public static DateTime Now => CurrentProvider.UtcNow().ToLocalTime();

        /// <summary>
        /// Gets the current the Date portion of the current local DateTime unless pinned then it will returned the date portion of the pinned time.
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <summary>
        /// Gets the current UTC DateTime unless pinned then it will returned the pinned time as a UTC time.
        /// </summary>
        public static DateTime UtcNow => CurrentProvider.UtcNow();
    }
}
