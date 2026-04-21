using NUnit.Framework;
using System;
using System.Linq;
using Zone8.Events;

public class DummyEvent : IEvent { }

public class Tests
{
    [SetUp]
    public void SetUp()
    {
        // Use reflection to clear bindings before each test
        var clearMethod = typeof(EventBus<DummyEvent>).GetMethod("ClearBindings", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        clearMethod.Invoke(null, null);
    }

    [Test]
    public void Register_AddsBinding()
    {
        var binding = new EventBinding<DummyEvent>(_ => { });
        EventBus<DummyEvent>.Register(binding);

        var bindings = EventBus<DummyEvent>.GetBindings();
        Assert.Contains(binding, (System.Collections.ICollection)bindings);
    }

    [Test]
    public void Deregister_RemovesBinding()
    {
        var binding = new EventBinding<DummyEvent>(_ => { });
        EventBus<DummyEvent>.Register(binding);
        EventBus<DummyEvent>.Deregister(binding);

        var bindings = EventBus<DummyEvent>.GetBindings();
        Assert.IsFalse(bindings.Contains(binding));
    }

    [Test]
    public void Raise_InvokesOnEvent()
    {
        bool wasCalled = false;
        var binding = new EventBinding<DummyEvent>(_ => wasCalled = true);
        EventBus<DummyEvent>.Register(binding);

        EventBus<DummyEvent>.Raise(new DummyEvent());
        Assert.IsTrue(wasCalled);
    }

    [Test]
    public void RaiseNoArgs_InvokesOnEventNoArgs()
    {
        bool wasCalled = false;
        var binding = new EventBinding<DummyEvent>(() => wasCalled = true);
        EventBus<DummyEvent>.Register(binding);

        EventBus<DummyEvent>.Raise();
        Assert.IsTrue(wasCalled);
    }

    [Test]
    public void DeregisteredBinding_IsNotInvoked_OnRaise()
    {
        bool wasCalled = false;
        var binding = new EventBinding<DummyEvent>(_ => wasCalled = true);
        EventBus<DummyEvent>.Register(binding);
        EventBus<DummyEvent>.Deregister(binding);

        // Should not throw and should not call the handler
        Assert.DoesNotThrow(() => EventBus<DummyEvent>.Raise(new DummyEvent()));
        Assert.IsFalse(wasCalled);
    }

    [Test]
    public void DeregisteredBinding_IsNotInvoked_OnRaiseNoArgs()
    {
        bool wasCalled = false;
        var binding = new EventBinding<DummyEvent>(() => wasCalled = true);
        EventBus<DummyEvent>.Register(binding);
        EventBus<DummyEvent>.Deregister(binding);

        // Should not throw and should not call the handler
        Assert.DoesNotThrow(() => EventBus<DummyEvent>.Raise());
        Assert.IsFalse(wasCalled);
    }

}
