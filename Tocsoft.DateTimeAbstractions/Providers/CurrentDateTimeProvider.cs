using System;

namespace Tocsoft.DateTimeAbstractions.Providers
{
    public class CurrentDateTimeProvider : DateTimeProvider
    {
        public override DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
