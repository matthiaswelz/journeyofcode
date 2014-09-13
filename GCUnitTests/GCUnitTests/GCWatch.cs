using System;
using System.Linq;

namespace GCUnitTests
{
    /// <summary>
    ///     Allows determining the GC eligibility of an object.
    /// </summary>
    /// <remarks>
    ///     This class is intended for usage in aumtomatic tests only as it triggers garbage collections.
    /// </remarks>
    public struct GCWatch
    {
        private readonly WeakReference _reference;

        /// <summary>
        ///     Initializes a new instance of the <c>GCWatch</c> class passing the object to watch.
        /// </summary>
        /// <param name="value">
        ///     The object whose GC eligibility to test.
        /// </param>
        public GCWatch(object value)
        {
            this._reference = new WeakReference(value, false);
        }

        /// <summary>
        ///     Determines whether the object passed when creating this <c>GCWatch</c> is currently eligible for garbage collection.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the object is currently eligible, otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        ///     Each call to this method triggers a full garbage collection by invoking <c>GC.Collect</c>.
        /// </remarks>
        public bool IsEligibleForGC()
        {
            if (!this._reference.IsAlive)
                return true;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            return !this._reference.IsAlive;
        }
    }
}
