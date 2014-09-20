using System;
using System.Linq;
using System.Threading.Tasks;

namespace journeyofcode.Threading
{
    /// <summary>
    ///     Provides access to a value that will become available at some point in the future.
    /// </summary>
    /// <remarks>
    ///     This type provides implicit conversions to <typeparamref name="T"/> and <see cref="Task&lt;T&gt;"/>. The implicit conversion to <typeparamref name="T"/> calls <see cref="ValueOrDefault"/>.
    /// </remarks>
    /// <typeparam name="T">
    ///     The type of the value.
    /// </typeparam>
    public struct TaskLazy<T>
    {
        /// <summary>
        ///     Returns <see cref="ValueOrDefault"/>.
        /// </summary>
        public static implicit operator T(TaskLazy<T> taskLazy)
        {
            return taskLazy.ValueOrDefault();
        }
        /// <summary>
        ///     Returns <see cref="Task"/>.
        /// </summary>
        public static implicit operator Task<T>(TaskLazy<T> taskLazy)
        {
            return taskLazy.Task;
        }

        /// <summary>
        ///     Returns a new <see cref="TaskLazy&lt;T&gt;" /> based on <paramref name="task"/>.
        /// </summary>
        public static implicit operator TaskLazy<T>(Task<T> task)
        {
            return new TaskLazy<T>(task);
        }

        /// <summary>
        ///     Returns a new <see cref="TaskLazy&lt;T&gt;" /> which is initialized with the value <paramref name="value"/>.
        /// </summary>
        public static implicit operator TaskLazy<T>(T value)
        {
            return System.Threading.Tasks.Task.FromResult(value);
        }

        /// <summary>
        ///     Returns the <see cref="Task"/> which is used to retrieve the value of this <see cref="TaskLazy&lt;T&gt;"/>
        /// </summary>
        public Task<T> Task { get; private set; }

        /// <summary>
        ///     Returns a value indicating whether the <see cref="Value"/> is available.
        /// </summary>
        public bool IsReady
        {
            get { return this.Task.IsCompleted && !this.Task.IsFaulted; }
        }

        /// <summary>
        ///     Returns the Value or throws an exception if it is not yet available.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when <see cref="IsReady"/> was <c>false</c> when calling this property.
        /// </exception>
        public T Value
        {
            get
            {
                if (!this.IsReady)
                    throw new InvalidOperationException("Cannot access Value before it is available.");

                return this.Task.Result;
            }
        }

        /// <summary>
        ///     Creates a new <see cref="TaskLazy&lt;T&gt;" /> based on a constant value.
        /// </summary>
        /// <param name="value">
        ///     The value to use for <see cref="Value"/>.
        /// </param>
        /// <remarks>
        ///     <see cref="IsReady"/> will be <c>true</c>.
        /// </remarks>
        public TaskLazy(T value)
            : this(System.Threading.Tasks.Task.FromResult(value))
        {
            
        }
        /// <summary>
        ///     Creates a new <see cref="TaskLazy&lt;T&gt;" /> based on a <see cref="Func&lt;T&gt;"/> which will be invoked in a new <see cref="Task"/>.
        /// </summary>
        /// <param name="func">
        ///     The function to execute.
        /// </param>
        /// <remarks>
        ///     The function will run on <see cref="TaskScheduler.Default"/>.
        /// </remarks>
        public TaskLazy(Func<T> func)
            : this(System.Threading.Tasks.Task.Run(func))
        {

        }
        /// <summary>
        ///     Creates a new <see cref="TaskLazy&lt;T&gt;" /> based on an existing <see cref="Task&lt;T&gt;"/>.
        /// </summary>
        /// <param name="task">
        ///     The task to use.
        /// </param>
        /// <remarks>
        ///     You can also directly assign a <see cref="Task&lt;T&gt;"/> to a <see cref="TaskLazy&lt;T&gt;"/> variable.
        /// </remarks>
        public TaskLazy(Task<T> task)
            : this()
        {
            this.Task = task;
        }

        /// <summary>
        ///     Calls an <see cref="Action"/> in the current synchronisation context after the <see cref="Value"/> has become available.
        /// </summary>
        /// <param name="action">
        ///     The <see cref="Action"/> to perform.
        /// </param>
        /// <remarks>
        ///     This method uses <see cref="TaskScheduler.Current"/>.
        /// </remarks>
        public Task WhenValueAvailable(Action action)
        {
            return this.WhenValueAvailable(t => action());
        }
        /// <summary>
        ///     Calls an <see cref="Action"/> in the current synchronisation context after the <see cref="Value"/> has become available.
        /// </summary>
        /// <param name="action">
        ///     The <see cref="Action"/> to perform (<see cref="Value"/> is passed as a parameter).
        /// </param>
        /// <remarks>
        ///     This method uses <see cref="TaskScheduler.Current"/>.
        /// </remarks>
        public Task WhenValueAvailable(Action<T> action)
        {
            return this.WhenValueAvailableAsync(action, TaskScheduler.Current);
        }

        /// <summary>
        ///     Calls an <see cref="Action"/> using a specified <see cref="TaskScheduler"/> after the <see cref="Value"/> has become available.
        /// </summary>
        /// <param name="action">
        ///     The <see cref="Action"/> to perform (<see cref="Value"/> is passed as a parameter).
        /// </param>
        /// <param name="taskScheduler">
        ///     The <see cref="TaskScheduler"/> to perform <paramref name="action"/> on or <c>null</c> to use <see cref="TaskScheduler.Default"/>.
        /// </param>
        public Task WhenValueAvailableAsync(Action<T> action, TaskScheduler taskScheduler = null)
        {
            return this.Task.ContinueWith(task => action(task.Result), taskScheduler ?? TaskScheduler.Default);
        }

        /// <summary>
        ///     Tries to retrieve the <see cref="Value"/> if it is available.
        /// </summary>
        /// <param name="result">
        ///     Is set to <see cref="Value"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> when <see cref="IsReady"/>, otherwise <c>false</c>.
        /// </returns>
        public bool TryGetValue(out T result)
        {
            if (!this.IsReady)
            {
                result = default(T);
                return false;
            }

            result = this.Value;
            return true;
        }
        /// <summary>
        ///     Returns <see cref="Value"/> if it is available or <paramref name="def"/> otherwise.
        /// </summary>
        /// <param name="def">
        ///     The value ro return when <see cref="Value"/> is not available (defaults to <c>default(T)</c>.
        /// </param>
        /// <returns>
        ///     <see cref="Value"/> or <paramref name="def"/>.
        /// </returns>
        public T ValueOrDefault(T def = default(T))
        {
            T result;
            if (!this.TryGetValue(out result))
                return def;

            return result;
        }

        /// <summary>
        ///     Returns the value of a <see cref="Func&lt;T, T2&gt;"/> which receives <see cref="Value"/> as a paremeter or <paramref name="def"/> if the value is not available yet.
        /// </summary>
        /// <typeparam name="T2">
        ///     The return value.
        /// </typeparam>
        /// <param name="func">
        ///     The function to call when the value is available.
        /// </param>
        /// <param name="def">
        ///     The value to return when the value is not available yet.
        /// </param>
        /// <returns>
        ///     The return value of <paramref name="func"/> or <paramref name="def"/>.
        /// </returns>
        /// <example>
        /// <![CDATA[
        /// TaskLazy<DateTime> taskLazy = //...
        /// string value = taskLazy.ActionOrDefault(date => date.ToString(), "Loading...");
        /// ]]>
        /// </example>
        public T2 ActionOrDefault<T2>(Func<T, T2> func, T2 def = default(T2))
        {
            if (!this.Task.IsCompleted)
                return def;

            return func(this.Task.Result);
        }
    }
}
