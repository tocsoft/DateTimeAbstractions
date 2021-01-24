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
        /// Pins the specified date/time retuned to the current time until the disposable is disposed.
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
        public TimeSpan Elapsed => stopwatch?.Elapsed ?? stopwatchInterface.Elapsed;

        /// <summary>
        /// Gets the total elapsed time measured by the current instance, in milliseconds.
        /// </summary>
        public long ElapsedMilliseconds => stopwatch?.ElapsedMilliseconds ?? stopwatchInterface.ElapsedMilliseconds;

        /// <summary>
        /// Gets the total elapsed time measured by the current instance, in timer ticks.
        /// </summary>
        public long ElapsedTicks => stopwatch?.ElapsedTicks ?? stopwatchInterface.ElapsedTicks;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ClockTimer"/> timer is running.
        /// </summary>
        public bool IsRunning => stopwatch?.IsRunning ?? stopwatchInterface.IsRunning;

        /// <summary>
        /// Stops time interval measurement and resets the elapsed time to zero.
        /// </summary>
        public void Reset()
        {
            if (stopwatch is object)
            {
                stopwatch.Reset();
            }
            else
            {
                stopwatchInterface.Reset();
            }
        }

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero, and starts measuring elapsed time.
        /// </summary>
        public void Restart()
        {
            if (stopwatch is object)
            {
                stopwatch.Restart();
            }
            else
            {
                stopwatchInterface.Restart();
            }
        }

        /// <summary>
        /// Starts, or resumes, measuring elapsed time for an interval.
        /// </summary>
        public void Start()
        {
            if (stopwatch is object)
            {
                stopwatch.Start();
            }
            else
            {
                stopwatchInterface.Start();
            }
        }

        /// <summary>
        /// Stops measuring elapsed time for an interval.
        /// </summary>
        public void Stop()
        {
            if (stopwatch is object)
            {
                stopwatch.Stop();
            }
            else
            {
                stopwatchInterface.Stop();
            }
        }

        /// <summary>
        /// Interface representing the concretet implmentaion of a ClockTimer.
        /// </summary>
        public interface IPinnedClockTimer
        {
            //
            // Summary:
            //     Gets the total elapsed time measured by the current instance.
            //
            // Returns:
            //     A read-only System.TimeSpan representing the total elapsed time measured by the
            //     current instance.
            public TimeSpan Elapsed { get; }

            //
            // Summary:
            //     Gets the total elapsed time measured by the current instance, in milliseconds.
            //
            // Returns:
            //     A read-only long integer representing the total number of milliseconds measured
            //     by the current instance.
            public long ElapsedMilliseconds { get; }

            //
            // Summary:
            //     Gets the total elapsed time measured by the current instance, in timer ticks.
            //
            // Returns:
            //     A read-only long integer representing the total number of timer ticks measured
            //     by the current instance.
            public long ElapsedTicks { get; }

            //
            // Summary:
            //     Gets a value indicating whether the System.Diagnostics.Stopwatch timer is running.
            //
            // Returns:
            //     true if the System.Diagnostics.Stopwatch instance is currently running and measuring
            //     elapsed time for an interval; otherwise, false.
            public bool IsRunning { get; }

            //
            // Summary:
            //     Stops time interval measurement and resets the elapsed time to zero.
            public void Reset();

            //
            // Summary:
            //     Stops time interval measurement, resets the elapsed time to zero, and starts
            //     measuring elapsed time.
            public void Restart();

            //
            // Summary:
            //     Starts, or resumes, measuring elapsed time for an interval.
            public void Start();

            //
            // Summary:
            //     Stops measuring elapsed time for an interval.
            public void Stop();
        }
    }
}
