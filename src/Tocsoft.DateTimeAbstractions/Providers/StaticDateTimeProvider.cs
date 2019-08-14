// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal class StaticDateTimeProvider : DateTimeProvider
    {
        private readonly DateTime utcdate;

        public StaticDateTimeProvider(DateTime date)
        {
            this.utcdate = date.ToUniversalTime();
        }

        public override DateTime UtcNow()
        {
            return this.utcdate;
        }
    }
}
