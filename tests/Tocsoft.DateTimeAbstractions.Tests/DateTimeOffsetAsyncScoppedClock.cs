// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tocsoft.DateTimeAbstractions.Providers;
using Xunit;

namespace Tocsoft.DateTimeAbstractions.Tests
{
    public class DateTimeOffsetAsyncScoppedClock
    {
        [Fact]
        public void LocalTimeConfiguredStaticProviderNowAlwausReturnsLocalTime()
        {
            StaticDateTimeOffsetProvider p = new StaticDateTimeOffsetProvider(new DateTimeOffset(2000, 01, 01, 1, 2, 3, TimeSpan.FromHours(3)));
            DateTimeOffset localTime = p.Now();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task ConcurrentAsyncThreadsReadingSamePinnedTimeAsync(int concurrentCount)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < concurrentCount; i++)
            {
                Task task = this.RunTest(i);
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
            for (int i = 0; i < concurrentCount; i++)
            {
                Task task = this.RunTestForceThread(i);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private Task RunTestForceThread(int count)
        {
            return Task.Run(async () =>
            {
                await this.RunTest(count);
            });
        }

        private async Task RunTest(int count)
        {
            DateTimeOffset date = new DateTimeOffset(2000, 01, 01, 12, 12, 12, TimeSpan.FromHours(2));
            date = date.AddDays(count);
            using (ClockOffset.Pin(date))
            {
                Task<DateTimeOffset> task1 = this.DelayedNow(true);
                Task<DateTimeOffset> task2 = this.DelayedNow(false);
                Task<DateTimeOffset> task3 = this.DelayedNow(true);
                DateTimeOffset[] dates = await Task.WhenAll(task1, task2, task3);
                Assert.All(dates, x =>
                {
                    Assert.Equal(date, x);
                });
            }

            // we move away from the pinned after the using statement
            Assert.NotEqual(date, Clock.Now);

            using (ClockOffset.Pin(date))
            {
                Task<DateTimeOffset> task1 = this.DelayedNow(true);
                Task<DateTimeOffset> task2 = this.DelayedNow(false);
                Task<DateTimeOffset> task3 = this.DelayedNow(true);
                DateTimeOffset[] dates = await Task.WhenAll(task1, task2, task3);
                Assert.All(dates, x =>
                {
                    Assert.Equal(date, x);
                });
            }
        }

        [Fact]
        public async Task PinSubContext()
        {
            DateTimeOffset date = new DateTimeOffset(2000, 01, 01, 1, 2, 3, TimeSpan.FromHours(1));
            DateTimeOffset date1 = new DateTimeOffset(2000, 01, 02, 1, 2, 3, TimeSpan.FromHours(2));
            DateTimeOffset date2 = new DateTimeOffset(2000, 01, 03, 1, 2, 3, TimeSpan.FromHours(3));
            using (ClockOffset.Pin(date))
            {
                Task<DateTimeOffset> task1 = this.DelayedDate(date1);
                Task<DateTimeOffset> task2 = this.DelayedDate(date2);
                DateTimeOffset[] dates = await Task.WhenAll(task1, task2);
                DateTimeOffset ambiantDate = ClockOffset.Now;

                Assert.Contains(date1, dates);
                Assert.Contains(date2, dates);
                Assert.Equal(date, ambiantDate);
            }

            // we move away from the pinned after the using statement
            Assert.NotEqual(date, Clock.Now);
        }

        public async Task<DateTimeOffset> DelayedNow(bool continueOnCapturedContext)
        {
            await Task.Delay(1).ConfigureAwait(continueOnCapturedContext); // to force a proper delay
            return ClockOffset.Now;
        }

        public async Task<DateTimeOffset> DelayedDate(DateTimeOffset pinnedDate)
        {
            using (ClockOffset.Pin(pinnedDate))
            {
                await Task.Delay(1); // to force a proper delay
                return ClockOffset.Now;
            }
        }
    }
}
