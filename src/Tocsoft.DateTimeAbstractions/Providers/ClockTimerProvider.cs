// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal abstract class ClockTimerProvider
    {
        // Summary:
        //     Initializes a new System.Diagnostics.Stopwatch instance, sets the elapsed time
        //     property to zero, and starts measuring elapsed time.
        //
        // Returns:
        //     A System.Diagnostics.Stopwatch that has just begun measuring elapsed time.
        public abstract ClockTimer Create();
    }
}
