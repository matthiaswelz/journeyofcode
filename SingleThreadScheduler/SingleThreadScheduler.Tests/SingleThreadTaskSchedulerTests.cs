using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using journeyofcode.Threading;
using NUnit.Framework;

namespace Tests.journeyofcode.Threading
{
    [TestFixture]
    public sealed class SingleThreadTaskSchedulerTests
    {
        public ApartmentState ApartementState { get; set; }

        public SingleThreadTaskScheduler Scheduler { get; set; }

        [SetUp]
        public void SetUp()
        {
            this.ApartementState = ApartmentState.STA;

            this.Create();
        }

        [TearDown]
        public void TearDown()
        {
            if (this.Scheduler != null)
                this.Scheduler.Dispose();

            this.Scheduler = null;
        }

        [Test]
        public void Executes_Task()
        {
            bool executed = false;

            this.ExecuteAndWait(() => executed = true);

            Assert.IsTrue(executed);
        }

        [Test]
        public void Maximum_ConcurrencyLevel_Is_One()
        {
            Assert.AreEqual(1, this.Scheduler.MaximumConcurrencyLevel);
        }

        [Test]
        public void Calls_Init_Action_At_Start()
        {
            bool called = false;
            this.Scheduler = new SingleThreadTaskScheduler(() => called = true);

            this.ExecuteAndWait(() => Assert.IsTrue(called));

            Assert.IsTrue(called);
        }

        [Test]
        public void CanConcurrentlyAddTasks()
        {
            int count = 0;
            var tasks = Enumerable.Range(0, 10000).AsParallel().Select(i => this.Execute(() => Interlocked.Increment(ref count))).ToArray();

            Task.WaitAll(tasks);

            Assert.AreEqual(10000, count);
            Assert.IsTrue(tasks.All(t => t.IsCompleted));
        }

        [Test]
        public void Executes_Async()
        {
            var tasks = Enumerable.Range(0, 5).Select(i => this.Execute(() => Thread.Sleep(200))).ToArray();

            Assert.IsTrue(!tasks.Any(t => t.IsCompleted));
        }

        [Test]
        public void Dispose_Cancels_Further_Tasks()
        {
            var tasks = Enumerable.Range(0, 5).Select(i => this.Execute(() => Thread.Sleep(50))).ToArray();
            this.Scheduler.Dispose();

            Thread.Sleep(500);

            Assert.IsTrue(tasks.First().IsCompleted);
            Assert.IsTrue(tasks.Skip(1).All(t => t.Status == TaskStatus.WaitingToRun));
        }

        [Test]
        public void Can_Dispose_Multiple_Times()
        {
            this.ExecuteAndWait(() => { });

            this.Scheduler.Dispose();
            this.Scheduler.Dispose();
        }

        [Test]
        public void Can_Dispose_After_Wait()
        {
            this.ExecuteAndWait(() => { });

            this.Scheduler.Wait();
            this.Scheduler.Dispose();
        }

        [Test]
        public void Cannot_Wait_Twice()
        {
            this.ExecuteAndWait(() => { });

            this.Scheduler.Wait();
            Assert.Catch<TaskSchedulerException>(() => this.Scheduler.Wait());
        }

        [Test]
        public void Cannot_Schedule_Work_After_Dispose()
        {
            this.Scheduler.Dispose();

            Assert.Catch<TaskSchedulerException>(() => this.Execute(() => { }));
        }


        [Test]
        public void Cannot_Schedule_Work_After_Wait()
        {
            this.Scheduler.Wait();

            Assert.Catch<TaskSchedulerException>(() => this.Execute(() => { }));
        }

        [Test]
        public void Wait_Waits_For_All_Tasks()
        {
            var tasks = Enumerable.Range(0, 10).Select(i => this.Execute(() => Thread.Sleep(50))).ToArray();

            this.Scheduler.Wait();

            Assert.IsTrue(tasks.All(t => t.IsCompleted));
        }

        [Test]
        public void Supports_Inlining()
        {
            bool executed = false;
            this.ExecuteAndWait(() => this.ExecuteAndWait(() => executed = true));

            Assert.IsTrue(executed);
        }

        [Test]
        public void Supports_Inlining_Multiple_Tasks()
        {
            int count = 0;
            var tasks = Enumerable.Range(0, 10).Select(i => this.Execute(() =>
            {
                for (int j = 0; j < 10; j++)
                    this.ExecuteAndWait(() => Interlocked.Increment(ref count));

            })).ToArray();

            Task.WaitAll(tasks);

            Assert.AreEqual(100, count);
        }

        [Test]
        public void Cannot_Construct_Using_Invalid_Apartement_State()
        {
            Assert.Catch<ArgumentException>(() => new SingleThreadTaskScheduler(ApartmentState.Unknown));
            Assert.Catch<ArgumentException>(() => new SingleThreadTaskScheduler(() => { }, ApartmentState.Unknown));
        }

        [TestCase(ApartmentState.MTA)]
        [TestCase(ApartmentState.STA)]
        public void Uses_Passed_Apartement_State(ApartmentState state)
        {
            this.ApartementState = state;
            this.Create();

            this.ExecuteAndWait(() => Assert.AreEqual(state, Thread.CurrentThread.GetApartmentState()));

            Assert.AreEqual(state, this.Scheduler.ApartmentState);
        }

        [Test]
        public void Uses_Same_Thread_For_Multiple_Actions()
        {
            var threads = new ConcurrentQueue<Thread>();

            var tasks = Enumerable.Range(0, 5000).Select(i => this.Execute(() => threads.Enqueue(Thread.CurrentThread))).ToArray();
            Task.WaitAll(tasks);

            Assert.AreEqual(5000, threads.Count);
            Assert.AreEqual(1, threads.Distinct().Count());
        }

        private SingleThreadTaskScheduler Create()
        {
            this.Scheduler = new SingleThreadTaskScheduler(this.ApartementState);

            return this.Scheduler;
        }


        private void ExecuteAndWait(Action action)
        {
            this.Execute(action).Wait();
        }
        private Task Execute(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, this.Scheduler);
        }
    }
}
