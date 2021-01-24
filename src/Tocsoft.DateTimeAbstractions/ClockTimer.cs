// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using System.Threading;
using Tocsoft.DateTimeAbstractions.Providers;

namespace Tocsoft.DateTimeAbstractions
{
    /// <summary>
    /// Provides pinable and testable scoped access to DateTime static properties.
    /// </summary>
    public class ClockTimer
    {
        private static AsyncLocal<ImmutableStack<ClockTimerProvider>> clockStack = new AsyncLocal<ImmutableStack<ClockTimerProvider>>();
        private readonly Stopwatch stopwatch;
        private readonly IPinnedClockTimer stopwatchInterface;

        private static ClockTimerProvider DefaultProvider { get; set; } = new DefaultStopwatchProvider();

        internal static ClockTimerProvider CurrentProvider
        {
            get
            {
                if (clockStack.Value == null)
                {
                    return DefaultProvider;
                }

                clockStack.Value = clockStack.Value ?? ImmutableStack.Create<ClockTimerProvider>();
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
        /// Pins the specified date/time returned to the current time until the disposable is disposed.
        /// </summary>
        /// <param name="elapsed">The date and time to the clock.</param>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin(TimeSpan elapsed)
        {
            return Pin(new StaticElapsedClockTimerProvider(elapsed));
        }

        /// <summary>
        /// Pins the time to call the function every time it needs access to the elapsed time until the disposable is disposed.
        /// </summary>
        /// <param name="elapsedFunc">The function that returns the TimeSpan for the elapsed time.</param>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin(Func<TimeSpan> elapsedFunc)
        {
            return Pin(new DelegateClockTimerProvider(elapsedFunc));
        }

        /// <summary>
        /// Pins the implementation of the timer returned to use for all callers until the disposable is disposed.
        /// </summary>
        /// <param name="timer">The date and time to the clock.</param>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin(IPinnedClockTimer timer)
        {
            return Pin(new PinnedClockTimerProvider(timer));
        }

        /// <summary>
        /// Pins the time to call the function every time it needs access to the elapsed time until the disposable is disposed.
        /// </summary>
        /// <param name="timerFunc">The function that returns the timer implementation for the elapsed timer.</param>
        /// <returns>The disposer that manages the lifetime of the scoped pinned value.</returns>
        public static IDisposable Pin(Func<IPinnedClockTimer> timerFunc)
        {
            return Pin(new DelegatePinnedClockTimerProvider(timerFunc));
        }

        internal static IDisposable Pin(ClockTimerProvider provider)
        {
            ImmutableStack<ClockTimerProvider> stack = clockStack.Value ?? ImmutableStack.Create<ClockTimerProvider>();
            clockStack.Value = stack.Push(provider);
            return new PopWhenDisposed();
        }

        private static void Pop()
        {
            clockStack.Value = clockStack.Value?.Pop();
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
        /// Initializes a new instance of the <see cref="ClockTimer"/> class.
        /// </summary>
        public ClockTimer()
        {
            var timer = CurrentProvider.Create();
            this.stopwatch = timer.stopwatch;
            this.stopwatchInterface = timer.stopwatchInterface;
        }

        internal ClockTimer(Stopwatch stopwatch)
        {
            this.stopwatch = stopwatch;
            this.stopwatchInterface = null;
        }

        internal ClockTimer(IPinnedClockTimer stopwatch)
        {
            this.stopwatch = null;
            this.stopwatchInterface = stopwatch;
        }

        /// <summary>
        /// Initializes a new System.Diagnostics.Stopwatch instance, sets the elapsed time property to zero, and starts measuring elapsed time.
        /// </summary>
        /// <returns>A <see cref="ClockTimer"/> that has just begun measuring elapsed time.</returns>
        public static ClockTimer StartNew()
        {
            var timer = CurrentProvider.Create();
            timer.Start();
            return timer;
        }

        /// <summary>
        /// Gets the total elapsed time measured by the current instance.
        /// </summary>
        public TimeSpan Elapsed => this.stopwatch?.Elapsed ?? this.stopwatchInterface.Elapsed;

        /// <summary>
        /// Gets the total elapsed time measured by the current instance, in milliseconds.
        /// </summary>
        public long ElapsedMilliseconds => this.stopwatch?.ElapsedMilliseconds ?? this.stopwatchInterface.ElapsedMilliseconds;

        /// <summary>
        /// Gets the total elapsed time measured by the current instance, in timer ticks.
        /// </summary>
        public long ElapsedTicks => this.stopwatch?.ElapsedTicks ?? this.stopwatchInterface.ElapsedTicks;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ClockTimer"/> timer is running.
        /// </summary>
        public bool IsRunning => this.stopwatch?.IsRunning ?? this.stopwatchInterface.IsRunning;

        /// <summary>
        /// Stops time interval measurement and resets the elapsed time to zero.
        /// </summary>
        public void Reset()
        {
            if (this.stopwatch is object)
            {
                this.stopwatch.Reset();
            }
            else
            {
                this.stopwatchInterface.Reset();
            }
        }

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
        /// </summary>
        public void Restart()
        {
            if (this.stopwatch is object)
            {
                this.stopwatch.Restart();
            }
            else
            {
                this.stopwatchInterface.Restart();
            }
        }

        /// <summary>
        /// Starts, or resumes, measuring elapsed time for an interval.
        /// </summary>
        public void Start()
        {
            if (this.stopwatch is object)
            {
                this.stopwatch.Start();
            }
            else
            {
                this.stopwatchInterface.Start();
            }
        }

        /// <summary>
        /// Stops measuring elapsed time for an interval.
        /// </summary>
        public void Stop()
        {
            if (this.stopwatch is object)
            {
                this.stopwatch.Stop();
            }
            else
            {
                this.stopwatchInterface.Stop();
            }
        }

        /// <summary>
        /// Interface representing a concrete implementation of a ClockTimer.
        /// </summary>
        public interface IPinnedClockTimer
        {
            /// <summary>
            /// Gets the total elapsed time measured by the current instance.
            /// </summary>
            public TimeSpan Elapsed { get; }

            /// <summary>
            /// Gets the total elapsed time measured by the current instance, in milliseconds.
            /// </summary>
            public long ElapsedMilliseconds { get; }

            /// <summary>
            /// Gets the total elapsed time measured by the current instance, in timer ticks.
            /// </summary>
            public long ElapsedTicks { get; }

            /// <summary>
            ///  Gets a value indicating whether the System.Diagnostics.Stopwatch timer is running.
            /// </summary>
            public bool IsRunning { get; }

            /// <summary>
            /// Stops time interval measurement and resets the elapsed time to zero.
            /// </summary>
            public void Reset();

            /// <summary>
            /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
            /// </summary>
            public void Restart();

            /// <summary>
            /// Starts, or resumes, measuring elapsed time for an interval.
            /// </summary>
            public void Start();

            /// <summary>
            /// Stops measuring elapsed time for an interval.
            /// </summary>
            public void Stop();
        }
    }
}
