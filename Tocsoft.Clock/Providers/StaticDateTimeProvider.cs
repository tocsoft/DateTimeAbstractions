using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    public class StaticDateTimeProvider : DateTimeProvider
    {
        private readonly DateTime date;

        public StaticDateTimeProvider(DateTime date)
        {
            this.date = date;
        }

        public override DateTime Now()
        {
            return date;
        }
    }
}
