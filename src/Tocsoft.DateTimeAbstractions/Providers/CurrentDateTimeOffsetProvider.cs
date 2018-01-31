// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    internal class CurrentDateTimeOffsetProvider : DateTimeOffsetProvider
    {
        public override DateTimeOffset Now()
        {
            return DateTimeOffset.Now;
        }

        public override DateTimeOffset UtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
