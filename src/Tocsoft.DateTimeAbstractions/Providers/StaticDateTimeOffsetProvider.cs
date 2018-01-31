// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal class StaticDateTimeOffsetProvider : DateTimeOffsetProvider
    {
        private readonly DateTimeOffset date;
        private readonly DateTimeOffset utcdate;

        public StaticDateTimeOffsetProvider(DateTimeOffset date)
        {
            this.date = date.ToLocalTime();
            this.utcdate = date.ToUniversalTime();
        }

        public override DateTimeOffset Now()
        {
            return this.date;
        }

        public override DateTimeOffset UtcNow()
        {
            return this.utcdate;
        }
    }
}
