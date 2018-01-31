using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    public class StaticDateTimeProvider : DateTimeProvider
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
            return date;
        }
        public override DateTime UtcNow()
        {
            return utcdate;
        }
    }
}
