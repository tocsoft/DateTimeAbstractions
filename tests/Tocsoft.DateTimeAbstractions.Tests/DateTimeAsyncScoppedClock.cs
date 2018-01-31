// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tocsoft.DateTimeAbstractions.Providers;
using Xunit;

namespace Tocsoft.DateTimeAbstractions.Tests
{
    public class DateTimeAsyncScoppedClock
    {
        [Fact]
        public void LocalTimeConfiguredStaticProviderNowAlwausReturnsLocalTime()
        {
            StaticDateTimeProvider p = new StaticDateTimeProvider(new DateTime(2000, 01, 01, 1, 2, 3, DateTimeKind.Local));
            DateTime localTime = p.Now();
            Assert.Equal(DateTimeKind.Local, localTime.Kind);
        }

        [Fact]
        public void UtcTimeConfiguredStaticProviderNowAlwausReturnsLocalTime()
        {
            StaticDateTimeProvider p = new StaticDateTimeProvider(new DateTime(2000, 01, 01, 1, 2, 3, DateTimeKind.Utc));
            DateTime localTime = p.Now();
            Assert.Equal(DateTimeKind.Local, localTime.Kind);
        }

        [Fact]
        public void LocalTimeConfiguredStaticProviderUtcNowAlwausReturnsUtcTime()
        {
            StaticDateTimeProvider p = new StaticDateTimeProvider(new DateTime(2000, 01, 01, 1, 2, 3, DateTimeKind.Local));
            DateTime localTime = p.UtcNow();
            Assert.Equal(DateTimeKind.Utc, localTime.Kind);
        }

        [Fact]
        public void UtcTimeConfiguredStaticProviderUtcNowAlwausReturnsUtcTime()
        {
            StaticDateTimeProvider p = new StaticDateTimeProvider(new DateTime(2000, 01, 01, 1, 2, 3, DateTimeKind.Utc));
            DateTime localTime = p.UtcNow();
            Assert.Equal(DateTimeKind.Utc, localTime.Kind);
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
            DateTime date = new DateTime(2000, 01, 01);
            date = date.AddDays(count);
            using (Clock.Pin(date))
            {
                Task<DateTime> task1 = this.DelayedNow(true);
                Task<DateTime> task2 = this.DelayedNow(false);
                Task<DateTime> task3 = this.DelayedNow(true);
                DateTime[] dates = await Task.WhenAll(task1, task2, task3);
                Assert.All(dates, x =>
                {
                    Assert.Equal(date, x);
                });
            }

            // we move away from the pinned after the using statement
            Assert.NotEqual(date, Clock.Now);

            using (Clock.Pin(new StaticDateTimeProvider(date)))
            {
                Task<DateTime> task1 = this.DelayedNow(true);
                Task<DateTime> task2 = this.DelayedNow(false);
                Task<DateTime> task3 = this.DelayedNow(true);
                DateTime[] dates = await Task.WhenAll(task1, task2, task3);
                Assert.All(dates, x =>
                {
                    Assert.Equal(date, x);
                });
            }
        }

        [Fact]
        public async Task PinSubContext()
        {
            DateTime date = new DateTime(2000, 01, 01);
            DateTime date1 = new DateTime(2000, 01, 02);
            DateTime date2 = new DateTime(2000, 01, 03);
            using (Clock.Pin(new StaticDateTimeProvider(date)))
            {
                Task<DateTime> task1 = this.DelayedDate(date1);
                Task<DateTime> task2 = this.DelayedDate(date2);
                DateTime[] dates = await Task.WhenAll(task1, task2);
                DateTime ambiantDate = Clock.Now;

                Assert.Contains(date1, dates);
                Assert.Contains(date2, dates);
                Assert.Equal(date, ambiantDate);
            }

            // we move away from the pinned after the using statement
            Assert.NotEqual(date, Clock.Now);
        }

        public async Task<DateTime> DelayedNow(bool continueOnCapturedContext)
        {
            await Task.Delay(1).ConfigureAwait(continueOnCapturedContext); // to force a proper delay
            return Clock.Now;
        }

        public async Task<DateTime> DelayedDate(DateTime pinnedDate)
        {
            using (Clock.Pin(new StaticDateTimeProvider(pinnedDate)))
            {
                await Task.Delay(1); // to force a proper delay
                return Clock.Now;
            }
        }
    }
}
