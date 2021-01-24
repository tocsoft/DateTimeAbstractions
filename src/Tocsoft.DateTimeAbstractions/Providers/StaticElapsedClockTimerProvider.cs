// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using Tocsoft.DateTimeAbstractions.Providers;

namespace Tocsoft.DateTimeAbstractions
{
    internal sealed class StaticElapsedClockTimerProvider : ClockTimerProvider
    {
        private readonly TimeSpan elapsed;

        public StaticElapsedClockTimerProvider(TimeSpan elapsed)
        {
            this.elapsed = elapsed;
        }

        public override ClockTimer Create() => new ClockTimer(new StaticElapsedClockTimer(this.elapsed));

        internal sealed class StaticElapsedClockTimer : ClockTimer.IPinnedClockTimer
        {
            private readonly TimeSpan elapsed;

            public StaticElapsedClockTimer(TimeSpan elapsed)
            {
                this.elapsed = elapsed;
            }

            public TimeSpan Elapsed => this.elapsed;

            public long ElapsedMilliseconds => (long)this.elapsed.TotalMilliseconds;

            public long ElapsedTicks => this.elapsed.Ticks;

            public bool IsRunning { get; private set; } = false;

            public void Reset()
            {
                this.Stop();
            }

            public void Restart()
            {
                this.Start();
            }

            public void Start()
            {
                this.IsRunning = true;
            }

            public void Stop()
            {
                this.IsRunning = false;
            }
        }
    }
}