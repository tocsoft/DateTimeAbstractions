// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Diagnostics;
using Tocsoft.DateTimeAbstractions.Providers;

namespace Tocsoft.DateTimeAbstractions
{
    internal sealed class PinnedClockTimerProvider : ClockTimerProvider
    {
        private readonly ClockTimer.IPinnedClockTimer pinnedClockTimer;

        public PinnedClockTimerProvider(ClockTimer.IPinnedClockTimer pinnedClockTimer)
        {
            this.pinnedClockTimer = pinnedClockTimer;
        }

        public override ClockTimer Create() => new ClockTimer(pinnedClockTimer);
    }
}