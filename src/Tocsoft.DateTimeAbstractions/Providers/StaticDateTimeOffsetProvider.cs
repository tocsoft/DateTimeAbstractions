// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal class StaticDateTimeOffsetProvider : DateTimeOffsetProvider
    {
        private readonly DateTimeOffset utcdate;

        public StaticDateTimeOffsetProvider(DateTimeOffset date)
        {
            this.utcdate = date.ToUniversalTime();
        }

        public override DateTimeOffset UtcNow()
        {
            return this.utcdate;
        }
    }
}
