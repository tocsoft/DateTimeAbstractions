using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    public class UtcCurrentDateTimeProvider : DateTimeProvider
    {
        public override DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
