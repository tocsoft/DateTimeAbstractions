using System;

namespace Tocsoft.DateTimeAbstractions
{
    public class UtcCurrentDateTimeProvider : DateTimeProvider
    {
        public override DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
