// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal class DelegateDateTimeOffsetProvider : DateTimeOffsetProvider
    {
        private readonly Func<DateTimeOffset> dateFunc;

        public DelegateDateTimeOffsetProvider(Func<DateTimeOffset> dateFunc)
        {
            this.dateFunc = dateFunc;
        }

        public override DateTimeOffset UtcNow()
        {
            return this.dateFunc().ToUniversalTime();
        }
    }
}
