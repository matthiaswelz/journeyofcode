using System;
using System.Linq;
using System.Threading.Tasks;
using journeyofcode.Threading;
using NUnit.Framework;

namespace Tests.journeyofcode.Threading
{
    [TestFixture]
    public sealed class TaskLazyTests
    {
        [Test]
        public void Can_Be_Constructed_From_Task()
        {
            var task = Task.Run(() => 42);
            var taskLazy = new TaskLazy<int>(task);

            Assert.AreEqual(task, taskLazy.Task);
        }
        [Test]
        public void Can_Be_Assigned_From_Task()
        {
            var task = Task.Run(() => 42);
            TaskLazy<int> taskLazy = task;

            Assert.AreEqual(task, taskLazy.Task);
        }

        [Test]
        public void Can_Be_Constructed_From_Constant()
        {
            var taskLazy = new TaskLazy<int>(42);

            Assert.IsNotNull(taskLazy.Task);
            Assert.IsTrue(taskLazy.IsReady);
            Assert.AreEqual(42, taskLazy.Value);
        }

        [Test]
        public void Can_Be_Assigned_From_Constant()
        {
            TaskLazy<int> taskLazy = 42;

            Assert.IsNotNull(taskLazy.Task);
            Assert.IsTrue(taskLazy.IsReady);
            Assert.AreEqual(42, taskLazy.Value);
        }

        [Test]
        public void Can_Be_Constructed_From_Func()
        {
            var taskLazy = new TaskLazy<int>(() => 42);

            Assert.IsNotNull(taskLazy.Task);

            taskLazy.Task.Wait();

            Assert.IsTrue(taskLazy.IsReady);
            Assert.AreEqual(42, taskLazy.Value);
        }

        [Test]
        public void Executes_Func_On_Default_TaskScheduler()
        {
            var taskLazy = new TaskLazy<int>(() =>
            {
                Assert.AreEqual(TaskScheduler.Current, TaskScheduler.Default);

                return 42;
            });

            taskLazy.Task.Wait();

            Assert.AreEqual(42, taskLazy.Value);
        }

        [Test]
        public void Can_Access_Value_After_Completion()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            taskCompletionSource.SetResult(42);

            Assert.AreEqual(42, taskLazy.Value);
        }

        [Test]
        public void Cannot_Access_Value_Before_Completion()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            Assert.Catch<InvalidOperationException>(() =>
            {
                var temp = taskLazy.Value;
            });
        }


        [Test]
        public void Implicit_Conversion_Returns_Task()
        {
            var task = Task.Run(() => 42);
            TaskLazy<int> taskLazy = task;

            Assert.AreEqual(task, (Task)taskLazy);
        }

        [Test]
        public void Implicit_Conversion_Returns_Value_When_Ready()
        {

            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);
            taskCompletionSource.SetResult(42);

            int actual = taskLazy;

            Assert.AreEqual(42, actual);
        }

        [Test]
        public void Implicit_Conversion_Returns_Default_When_Not_Ready()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            int actual = taskLazy;

            Assert.AreEqual(0, actual);
        }

        [Test]
        public void IsReady_Returns_Correct_Value()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            Assert.IsFalse(taskLazy.IsReady);

            taskCompletionSource.SetResult(42);

            Assert.IsTrue(taskLazy.IsReady);
        }

        [Test]
        public void ValueOrDefault_Retuns_Value_When_Ready()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);
            taskCompletionSource.SetResult(42);

            var actual = taskLazy.ValueOrDefault();

            Assert.AreEqual(42, actual);
        }

        [Test]
        public void ValueOrDefault_Retuns_Default_When_Not_Ready()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            var actual = taskLazy.ValueOrDefault(52);

            Assert.AreEqual(52, actual);
        }

        [Test]
        public void TryGetValue_Returns_Value_When_Ready()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);
            taskCompletionSource.SetResult(42);

            int actual;
            var result = taskLazy.TryGetValue(out actual);

            Assert.IsTrue(result);
            Assert.AreEqual(42, actual);
        }

        [Test]
        public void TryGetValue_Returns_False_When_Not_Ready()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            int actual;
            var result = taskLazy.TryGetValue(out actual);

            Assert.IsFalse(result);
            Assert.AreEqual(0, actual);
        }

        [Test]
        public void ActionOrDefault_Uses_Action_When_Value_IsReady()
        {

            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);
            taskCompletionSource.SetResult(42);

            int actual = taskLazy.ActionOrDefault(value =>
            {
                Assert.AreEqual(42, value);

                return 52;
            });

            Assert.AreEqual(52, actual);
        }

        [Test]
        public void ActionOrDefault_Returns_Default_When_Value_Is_Not_Ready()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            int actual = taskLazy.ActionOrDefault(value =>
            {
                Assert.Fail();
                return 0;
            }, 42);

            Assert.AreEqual(42, actual);
        }

        [Test]
        public void WhenValueAvailable_Calls_Action_Immediately_If_Value_Already_Is_Available()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);
            taskCompletionSource.SetResult(42);

            bool called = false;
            taskLazy.WhenValueAvailableAsync(result =>
            {
                Assert.AreEqual(42, result);

                called = true;
            }).Wait();

            Assert.IsTrue(called);
        }

        [Test]
        public void WhenValueAvailableAsync_Calls_Action_In_Correct_TaskScheduler_If_Value_Already_Is_Available()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);
            taskCompletionSource.SetResult(42);

            bool called = false;
            taskLazy.WhenValueAvailableAsync(result =>
            {
                Assert.AreEqual(42, result);

                called = true;
            }, TaskScheduler.Default).Wait();

            Assert.IsTrue(called);
        }

        [Test]
        public void When_Value_Available_Executes_When_Value_Becomes_Available()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            bool called = false;
            var task = taskLazy.WhenValueAvailableAsync(result =>
            {
                Assert.AreEqual(42, result);

                called = true;
            });

            Assert.IsFalse(called);

            taskCompletionSource.SetResult(42);
            task.Wait();

            Assert.IsTrue(called);
        }

        [Test]
        public void Value_Does_Not_Become_Available_On_Exception()
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var taskLazy = new TaskLazy<int>(taskCompletionSource.Task);

            taskCompletionSource.SetException(new Exception());

            Assert.IsFalse(taskLazy.IsReady);
            Assert.Catch<InvalidOperationException>(() =>
            {
                var temp = taskLazy.Value;
            });
            Assert.AreEqual(42, taskLazy.ValueOrDefault(42));
            Assert.AreEqual(0, (int)taskLazy);
        }
    }
}
