using System;

namespace Tocsoft.DateTimeAbstractions
{
    public class CurrentDateTimeProvider : DateTimeProvider
    {
        public override DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
