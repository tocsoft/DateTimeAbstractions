// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using Tocsoft.DateTimeAbstractions.Providers;

namespace Tocsoft.DateTimeAbstractions
{

    internal sealed class DelegateClockTimerProvider : ClockTimerProvider
    {
        private readonly Func<TimeSpan> elapsed;

        public DelegateClockTimerProvider(Func<TimeSpan> elapsed)
        {
            this.elapsed = elapsed;
        }

        public override ClockTimer Create() => new ClockTimer(new DelegatePinnedClockTimer(elapsed));

        internal sealed class DelegatePinnedClockTimer : ClockTimer.IPinnedClockTimer
        {
            private readonly Func<TimeSpan> elapsed;

            public DelegatePinnedClockTimer(Func<TimeSpan> elapsed)
            {
                this.elapsed = elapsed;
            }

            public TimeSpan Elapsed => elapsed();

            public long ElapsedMilliseconds => (long)elapsed().TotalMilliseconds;

            public long ElapsedTicks => elapsed().Ticks;

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
                IsRunning = true;
            }

            public void Stop()
            {
                IsRunning = false;
            }
        }
    }
}