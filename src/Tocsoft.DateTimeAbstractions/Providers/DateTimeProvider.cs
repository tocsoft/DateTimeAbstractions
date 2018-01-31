// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal abstract class DateTimeProvider
    {
        public abstract DateTime Now();

        public abstract DateTime UtcNow();
    }
}
