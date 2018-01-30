using System;
using System.Threading.Tasks;
using Xunit;

namespace Tocsoft.DateTimeAbstractions.Tests
{
    public class AsyncScoppedClock
    {
        [Fact]
        public async Task ConcurrentAsyncThreadsReadingSamePinnedTimeAsync()
        {
            var date = new DateTime(2000, 01, 01);
            using (Clock.Pin(new StaticDateTimeProvider(date)))
            {
                var task1 = DelayedNow();
                var task2 = DelayedNow();
                var dates = await Task.WhenAll(task1, task2);
                Assert.All(dates, x =>
                {
                    Assert.Equal(date, x);
                });
            }
            // we move away from the pinned after the using statement
            Assert.NotEqual(date, Clock.Now);
        }

        [Fact]
        public async Task PinSubContext()
        {
            var date = new DateTime(2000, 01, 01);
            var date1 = new DateTime(2000, 01, 02);
            var date2 = new DateTime(2000, 01, 03);
            using (Clock.Pin(new StaticDateTimeProvider(date)))
            {
                var task1 = DelayedDate(date1);
                var task2 = DelayedDate(date2);
                var dates = await Task.WhenAll(task1, task2);
                var ambiantDate = Clock.Now;

                Assert.Contains(date1, dates);
                Assert.Contains(date2, dates);
                Assert.Equal(date, ambiantDate);
            }
            // we move away from the pinned after the using statement
            Assert.NotEqual(date, Clock.Now);
        }


        public async Task<DateTime> DelayedNow()
        {
            await Task.Delay(1); // to force a propert delay
            return Clock.Now;
        }

        public async Task<DateTime> DelayedDate(DateTime pinnedDate)
        {
            using (Clock.Pin(new StaticDateTimeProvider(pinnedDate)))
            {
                await Task.Delay(1); // to force a propert delay
                return Clock.Now;
            }
        }
    }
}
