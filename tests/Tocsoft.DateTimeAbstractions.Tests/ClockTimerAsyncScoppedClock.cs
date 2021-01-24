// Copyright (c) Tocsoft and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tocsoft.DateTimeAbstractions.Providers;
using Xunit;

namespace Tocsoft.DateTimeAbstractions.Tests
{
    public class ClockTimerAsyncScoppedClock
    {
        [Fact]
        public void PinStaticDateCausesStaticDateTimeProviderToBeUsed()
        {
            var targetTime = TimeSpan.FromMilliseconds(123456);
            using (ClockTimer.Pin(targetTime))
            {
                var staticProvider = Assert.IsType<StaticElapsedClockTimerProvider>(ClockTimer.CurrentProvider);
                Assert.Equal(targetTime, staticProvider.Create().Elapsed);
            }
        }

        [Fact]
        public void UnpinnedDateCausesCurrentDateTimeProviderToBeUsed()
        {
            Assert.IsType<DefaultStopwatchProvider>(ClockTimer.CurrentProvider);
        }

        [Fact]
        public void PinnedDelegateCausesDelegateDateTimeProviderToBeUsed()
        {
            var targetTime = TimeSpan.FromMilliseconds(12312456);

            Func<TimeSpan> targetDateFunc = () =>
            {
                return targetTime;
            };

            using (ClockTimer.Pin(targetDateFunc))
            {
                var provider = Assert.IsType<DelegateClockTimerProvider>(ClockTimer.CurrentProvider);
                Assert.Equal(targetTime, provider.Create().Elapsed);
            }
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

            var targetTime = TimeSpan.FromMilliseconds(12312456);
            targetTime = targetTime + TimeSpan.FromMinutes(count);

            using (ClockTimer.Pin(targetTime))
            {
                Task<TimeSpan> task1 = this.DelayedNow(true);
                Task<TimeSpan> task2 = this.DelayedNow(false);
                Task<TimeSpan> task3 = this.DelayedNow(true);
                TimeSpan[] dates = await Task.WhenAll(task1, task2, task3);
                Assert.All(dates, x =>
                {
                    Assert.Equal(targetTime, x);
                });
            }

            // we move away from the pinned after the using statement
            var timer = ClockTimer.StartNew();
            timer.Stop();
            var now = timer.Elapsed;

            Assert.NotEqual(targetTime, now);

            using (ClockTimer.Pin(targetTime))
            {
                Task<TimeSpan> task1 = this.DelayedNow(true);
                Task<TimeSpan> task2 = this.DelayedNow(false);
                Task<TimeSpan> task3 = this.DelayedNow(true);
                TimeSpan[] dates = await Task.WhenAll(task1, task2, task3);
                Assert.All(dates, x =>
                {
                    Assert.Equal(targetTime, x);
                });
            }
        }

        [Fact]
        public async Task PinSubContext()
        {
            var targetTime = TimeSpan.FromMilliseconds(12312456);
            var targetTime1 = targetTime + TimeSpan.FromMinutes(1);
            var targetTime2 = targetTime + TimeSpan.FromMinutes(2);

            using (ClockTimer.Pin(targetTime))
            {
                Task<TimeSpan> task1 = this.DelayedDate(targetTime1);
                Task<TimeSpan> task2 = this.DelayedDate(targetTime2);
                TimeSpan[] dates = await Task.WhenAll(task1, task2);

                var timer = ClockTimer.StartNew();
                timer.Stop();
                var ambiantDate = timer.Elapsed;

                Assert.Contains(targetTime1, dates);
                Assert.Contains(targetTime2, dates);
                Assert.Equal(targetTime, ambiantDate);
            }

            var nowtimer = ClockTimer.StartNew();
            nowtimer.Stop();
            var now = nowtimer.Elapsed;

            // we move away from the pinned after the using statement
            Assert.NotEqual(targetTime, now);
        }

        public async Task<TimeSpan> DelayedNow(bool continueOnCapturedContext)
        {
            await Task.Delay(1).ConfigureAwait(continueOnCapturedContext); // to force a proper delay
            var timer = ClockTimer.StartNew();
            timer.Stop();
            return timer.Elapsed;
        }

        public async Task<TimeSpan> DelayedDate(TimeSpan pinnedDate)
        {
            using (ClockTimer.Pin(pinnedDate))
            {
                var timer = ClockTimer.StartNew();
                await Task.Delay(1); // to force a proper delay
                timer.Stop();
                return timer.Elapsed;
            }
        }
    }
}
