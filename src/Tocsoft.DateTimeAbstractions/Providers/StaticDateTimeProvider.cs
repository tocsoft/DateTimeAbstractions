// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal class StaticDateTimeProvider : DateTimeProvider
    {
        private readonly DateTime utcdate;

        private readonly DateTime date;

        public StaticDateTimeProvider(DateTime date)
        {
            if (date.Kind == DateTimeKind.Utc)
            {
                this.utcdate = date;
                this.date = date.ToLocalTime();
            }
            else
            {
                this.date = date;
                this.utcdate = date.ToUniversalTime();
            }
        }

        public override DateTime Now()
        {
            return this.date;
        }

        public override DateTime UtcNow()
        {
            return this.utcdate;
        }
    }
}
