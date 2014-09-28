using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace journeyofcode.AsyncAwaitPitfalls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var tester = new Tests();

            this.DataContext = new[]
            {
                new Item{ Name = "Simple call", Action = () => tester.SimpleCallFromAsync()},
                new Item{ Name = "ThreadPool.QueueUserWorkItem", Action = () => tester.ThreadPool_QueueUserWorkItem()},
                new Item{ Name = "Task.Factory.StartNew", Action = () => tester.TaskFactory()},
                new Item{ Name = "Task.Run", Action = () => tester.TaskRun()},

                new Item{ Name = "Continue with", Action = () => tester.ContinueWith()},
                new Item{ Name = "Continue with (TaskScheduler)", Action = () => tester.ContinueWith2()},

                new Item{ Name = "StartNew with TaskScheduler", Action = () => tester.TaskScheduler_FromCurrentSynchronizationContext()},
                new Item{ Name = "StartNew with hidden TaskScheduler", Action = () => tester.TaskScheduler_HiddenScheduler()},
                
                new Item{ Name = "Plain Await", Action = () => tester.UsingAwait()},
                new Item{ Name = "Configuring Awaiter", Action = () => tester.ConfiguringAwaiter()},
                new Item{ Name = "Configuring Awaiter with short action", Action = () => tester.ConfiguringAwaiter2()},
                
                new Item{ Name = "Calling an async method (the wrong way)", Action = () => tester.CallingAsyncWrong()},
                new Item{ Name = "Calling an async method (the right way)", Action = () => tester.CallingAsyncRight()},

                new Item{ Name = "async and ContinueWith", Action = () => tester.AsyncAndContinueWith()},
                new Item{ Name = "Nested Tasks", Action = () => tester.NestedTask()},
                new Item{ Name = "Avoid Nested Tasks", Action = () => tester.AvoidNestedTasks()},

                new Item{ Name = "Don't block", Action = () => tester.DontBlock()},
                new Item{ Name = "Deadlock", Action = () => tester.Deadlock()},

                new Item{ Name = "Final example", Action = () => tester.FinalExample()},
            };
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ((Action) ((Button) sender).Tag)();
        }
    }

    public class Item
    {
        public string Name { get; set; }
        public Action Action { get; set; }
    }
}
