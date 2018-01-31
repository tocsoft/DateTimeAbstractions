using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    public class CurrentDateTimeProvider : DateTimeProvider
    {
        public override DateTime Now()
        {
            return DateTime.Now;
        }

        public override DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
