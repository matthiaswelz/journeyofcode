using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace GCUnitTests
{
    [TestFixture]
    public class FooTests
    {
        [Test]
        public void Usable_Reference_Keeps_Object_Alive()
        {
            var obj = new Object();
            var gcTester = new GCWatch(obj);

            Assert.IsFalse(gcTester.IsEligibleForGC());

            GC.KeepAlive(obj);
        }

        [Test]
        public void No_Reference_Allows_GC()
        {
            var obj = new Object();
            var gcTester = new GCWatch(obj);

            obj = null;

            Assert.IsTrue(gcTester.IsEligibleForGC());
        }

        [Test]
        public void GC_Collects_Graph()
        {
            var foo1 = new Foo();
            var foo2 = new Foo();

            var gcTester1 = new GCWatch(foo1);
            var gcTester2 = new GCWatch(foo2);

            foo1.objects.Add(foo2);
            foo2.objects.Add(foo1);

            foo1 = null;
            Assert.IsFalse(gcTester1.IsEligibleForGC());
            Assert.IsFalse(gcTester2.IsEligibleForGC());
            GC.KeepAlive(foo2);

            foo2 = null;
            Assert.IsTrue(gcTester1.IsEligibleForGC());
            Assert.IsTrue(gcTester2.IsEligibleForGC());
        }

        [Test]
        public void Reference_Keeps_Object_Alive_Or_Not()
        {
            var obj = new Object();
            var gcTester = new GCWatch(obj);

#if DEBUG
            Assert.IsFalse(gcTester.IsEligibleForGC());
#else
            Assert.IsTrue(gcTester.IsEligibleForGC());
#endif
        }

        [Test]
        public void Two_Collections_Are_Required()
        {
            var foo = new Foo();
            var bar = new Bar();
            bar.objects.Add(foo);

            var weakReference = new WeakReference(foo, true);
            foo = null;
            bar = null;

            Assert.IsTrue(weakReference.IsAlive);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            //At this point, bar has been collected, but placed into the FReachable queue. Thus, foo is still accessible (from the finalizer of foo)
            //Note: Might also be false (when additional, non-forced GC has occured - very unlikely, however)
            Assert.IsTrue(weakReference.IsAlive);

            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            //Now that bar has been completely collected, foo could also be collected
            Assert.IsFalse(weakReference.IsAlive);
        }
    }

    public class Foo
    {
        public List<object> objects = new List<object>();
    }

    public class Bar : Foo
    {
        public bool FinalizerCalled;

        ~Bar()
        {
            FinalizerCalled = true;
        }
    }
}
