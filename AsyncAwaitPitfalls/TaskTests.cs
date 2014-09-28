using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace journeyofcode.AsyncAwaitPitfalls
{
    public sealed class Tests
    {
        private readonly Thread _uiThread;

        public Tests()
        {
            this._uiThread = Thread.CurrentThread;
        }

        private void PrintInfo(string marker)
        {
            var currentThread = Thread.CurrentThread == this._uiThread ? "UI Thread" : "Threadpool Thread";

            Console.WriteLine("{0} ({1})", marker, currentThread);
        }

        public async void SimpleCallFromAsync()
        {
            //UI Thread
            this.PrintInfo("SimpleCall");
        }
        
        public void ThreadPool_QueueUserWorkItem()
        {
            //Background Thread
            ThreadPool.QueueUserWorkItem(o => this.PrintInfo("ThreadPool"));
        }

        public void TaskFactory()
        {
            //Background Thread *
            //(* when not called from within a Task)
            Task.Factory.StartNew(() => this.PrintInfo("TaskFactory 0"));
        }

        public void TaskScheduler_FromCurrentSynchronizationContext()
        {
            //UI Thread
            this.PrintInfo("0");

            Task.Factory.StartNew(() =>
            {
                //UI Thread
                this.PrintInfo("1");

                //UI Thread
                Task.Factory.StartNew(() => this.PrintInfo("3"));

            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void TaskScheduler_HiddenScheduler()
        {
            this.PrintInfo("0");

            Task.Factory.StartNew(() =>
            {
                //UI Thread
                this.PrintInfo("1");

                //Background Thread
                Task.Factory.StartNew(() => this.PrintInfo("3"));

            }, CancellationToken.None, TaskCreationOptions.HideScheduler, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void TaskRun()
        {
            //Background thread
            Task.Run(() => this.PrintInfo("0"));
        }

        public void ContinueWith()
        {
            //Background thread *
            //(* when not called from within a Task)
            Task.Run(() => this.PrintInfo("0")).ContinueWith(task => this.PrintInfo("1"));
        }

        public void ContinueWith2()
        {
            //UI thread
            Task.Run(() => this.PrintInfo("0")).ContinueWith(task => this.PrintInfo("1"), TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async void UsingAwait()
        {
            //UI Thread
            this.PrintInfo("0");

            //Background Thread
            await Task.Run(() => this.PrintInfo("1"));

            //UI Thread
            this.PrintInfo("2");
        }

        public async void ConfiguringAwaiter()
        {
            //UI Thread
            this.PrintInfo("0");

            await Task.Delay(500).ConfigureAwait(false);

            //Background Thread
            this.PrintInfo("1");

            await Task.Delay(500);

            //Background Thread
            this.PrintInfo("2");
        }

        public async void ConfiguringAwaiter2()
        {
            //UI Thread
            this.PrintInfo("0");

            await Task.Delay(0).ConfigureAwait(false);

            //UI thread
            this.PrintInfo("1");

            await Task.Delay(0);

            //UI thread
            this.PrintInfo("2");
        }

        public void CallingAsyncWrong()
        {
            this.PrintInfo("0");

            this.Other();

            this.PrintInfo("3");
        }


        public async void CallingAsyncRight()
        {
            this.PrintInfo("0");

            await this.Other();

            this.PrintInfo("3");
        }

        public void AsyncAndContinueWith()
        {
            this.PrintInfo("0");

            this.Other().ContinueWith(task => this.PrintInfo("3"));

            this.PrintInfo("4");
        }

        public async void NestedTask()
        {
            this.PrintInfo("0");

            //0 - 4 - 1 - 3 - 2
            await Task.Factory.StartNew(() => this.Other());

            this.PrintInfo("4");
        }

        public async void AvoidNestedTasks()
        {
            this.PrintInfo("0");

            await await Task.Factory.StartNew(() => this.Other());
            await Task.Factory.StartNew(() => this.Other()).Unwrap();
            await Task.Run(() => this.Other());

            this.PrintInfo("4");
        }

        private async Task Other()
        {
            this.PrintInfo("1");

            await Task.Delay(500);

            this.PrintInfo("2");
        }

        public void DontBlock()
        {
            this.PrintInfo("1");

            var result = Task.Run(() =>
            {
                Thread.Sleep(5000);

                return 42;
            }).Result;

            this.PrintInfo("2");
        }

        public void Deadlock()
        {
            this.PrintInfo("1");

            var result = this.deadlock().Result;

            this.PrintInfo("2");
        }

        private async Task<string> deadlock()
        {
            var a = await Task.Run(() => "A");
            var b = await Task.Run(() => "B");

            return a + b;
        }

        public string UIProperty { get; set; }

        public async void FinalExample()
        {
            //Access the UI from the UI thread
            this.UIProperty = "Starting...";

            //Do some work (in background, without blocking the UI thread)
            await Task.Run(() => Thread.Sleep(5000));

            //Access the UI from the UI thread
            this.UIProperty = "Almost done!";

            //Do some work (in background, without blocking the UI thread)
            await Task.Run(() => Thread.Sleep(5000));

            //Access the UI from the UI thread
            this.UIProperty = "Done";
        }
    }
}
