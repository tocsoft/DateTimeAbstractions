using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tocsoft.DateTimeAbstractions.Tests
{
    public class AsyncScoppedClock
    {
        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public async Task ConcurrentAsyncThreadsReadingSamePinnedTimeAsync(int concurrentCount)
        {
            List<Task> tasks = new List<Task>();
            for (var i = 0; i < concurrentCount; i++)
            {
                var task = RunTest(i);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public async Task ConcurrentAsyncThreadsReadingSamePinnedTimeAsyncMultithread(int concurrentCount)
        {
            List<Task> tasks = new List<Task>();
            for (var i = 0; i < concurrentCount; i++)
            {
                var task = RunTestForceThread(i);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private Task RunTestForceThread(int count)
        {
            return Task.Run(async () =>
            {
                await RunTest(count);
            });
        }

        private async Task RunTest(int count)
        {
            var date = new DateTime(2000, 01, 01);
            date = date.AddDays(count);
            using (Clock.Pin(new StaticDateTimeProvider(date)))
            {
                var task1 = DelayedNow(true);
                var task2 = DelayedNow(false);
                var task3 = DelayedNow(true);
                var dates = await Task.WhenAll(task1, task2, task3);
                Assert.All(dates, x =>
                {
                    Assert.Equal(date, x);
                });
            }
            // we move away from the pinned after the using statement
            Assert.NotEqual(date, Clock.Now);

            Clock.DefaultProvider = new UtcCurrentDateTimeProvider();

            using (Clock.Pin(new StaticDateTimeProvider(date)))
            {
                var task1 = DelayedNow(true);
                var task2 = DelayedNow(false);
                var task3 = DelayedNow(true);
                var dates = await Task.WhenAll(task1, task2, task3);
                Assert.All(dates, x =>
                {
                    Assert.Equal(date, x);
                });
            }
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
        
        public async Task<DateTime> DelayedNow(bool continueOnCapturedContext)
        {
            await Task.Delay(1).ConfigureAwait(continueOnCapturedContext); // to force a propert delay
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
