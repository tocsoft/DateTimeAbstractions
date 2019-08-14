// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal class DelegateDateTimeProvider : DateTimeProvider
    {
        private readonly Func<DateTime> datefunc;

        public DelegateDateTimeProvider(Func<DateTime> datefunc)
        {
            this.datefunc = datefunc;
        }

        public override DateTime UtcNow()
        {
            return this.datefunc().ToUniversalTime();
        }
    }
}
