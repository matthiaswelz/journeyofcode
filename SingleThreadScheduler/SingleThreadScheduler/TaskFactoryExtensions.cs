using System;
using System.Linq;
using System.Threading.Tasks;

namespace journeyofcode.Threading
{
    public static class TaskFactoryExtensions
    {
        public static Task StartNew(this TaskFactory factory, Action action, TaskScheduler scheduler)
        {
            return factory.StartNew(action, factory.CancellationToken, factory.CreationOptions, scheduler);
        }
        public static Task<T> StartNew<T>(this TaskFactory factory, Func<T> func, TaskScheduler scheduler)
        {
            return factory.StartNew(func, factory.CancellationToken, factory.CreationOptions, scheduler);
        }
    }
}
