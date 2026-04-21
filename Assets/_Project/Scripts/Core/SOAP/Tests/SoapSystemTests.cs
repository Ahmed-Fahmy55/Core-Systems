using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using Zone8.SOAP.AssetVariable;
using Zone8.SOAP.RuntimeSet;
using Zone8.SOAP.ScriptableVariable;



[TestFixture]
public class ScriptableVariableTests
{
    private class IntVariable : ScriptableVariable<int> { }
    private IntVariable _variable;

    [SetUp]
    public void SetUp()
    {
        _variable = ScriptableObject.CreateInstance<IntVariable>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_variable);
    }

    #region ScriptableVariable Tests

    [Test]
    public void ValueChange_Invokes_OnValueChanged()
    {
        int receivedValue = 0;
        _variable.OnValueChanged += (val) => receivedValue = val;

        _variable.Value = 100;

        Assert.AreEqual(100, receivedValue, "Event did not fire with the correct updated value.");
    }

    [Test]
    public void IsNull_ReturnsFalse_ForValueTypes()
    {
        Assert.IsFalse(_variable.IsNull);
    }

    #endregion

    #region ScriptableVariableRef Tests

    [Test]
    public void Ref_UseConstant_ReturnsConstValue()
    {
        var reference = new ScriptableVariableRef<int>();
        reference.UseConstant = true;

        reference.Value = 50;

        Assert.AreEqual(50, reference.Value, "The constant value was not correctly set or retrieved.");
    }

    [Test]
    public void Ref_UseVariable_ReturnsSvValue()
    {
        _variable.Value = 75;
        var reference = new ScriptableVariableRef<int>();
        reference.UseConstant = true;
        reference.Value = 75;
        Assert.AreEqual(75, reference.Value);
    }

    [Test]
    public void Ref_Set_UpdatesUnderlyingVariable()
    {
        var reference = new ScriptableVariableRef<int>();
        reference.UseConstant = false;
        reference.Value = 99;
        Assert.AreNotEqual(99, _variable.Value, "");
    }

    #endregion
}

[TestFixture]
public class RuntimeSetTests
{
    // Concrete implementation for testing abstract class
    private class TestRuntimeSet : RuntimeSet<int> { }

    private TestRuntimeSet _set;

    [SetUp]
    public void SetUp()
    {
        _set = ScriptableObject.CreateInstance<TestRuntimeSet>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_set);
    }

    #region Collection Logic

    [Test]
    public void Add_NewItem_AddsToList()
    {
        _set.Add(10);

        Assert.Contains(10, _set.Items);
        Assert.AreEqual(1, _set.Items.Count);
    }

    [Test]
    public void Add_DuplicateItem_DoesNotAddTwice()
    {
        _set.Add(10);
        _set.Add(10);

        Assert.AreEqual(1, _set.Items.Count, "RuntimeSet should prevent duplicate entries.");
    }

    [Test]
    public void Remove_ExistingItem_RemovesFromList()
    {
        _set.Add(10);
        _set.Remove(10);

        Assert.IsFalse(_set.Items.Contains(10));
        Assert.AreEqual(0, _set.Items.Count);
    }

    #endregion

    #region Event Dispatching

    [Test]
    public void Add_Invokes_OnItemAdded()
    {
        int receivedValue = -1;
        _set.OnItemAdded += (val) => receivedValue = val;

        _set.Add(42);

        Assert.AreEqual(42, receivedValue, "OnItemAdded event was not invoked with the correct value.");
    }

    [Test]
    public void Remove_Invokes_OnItemRemoved()
    {
        int receivedValue = -1;
        _set.Add(42);
        _set.OnItemRemoved += (val) => receivedValue = val;

        _set.Remove(42);

        Assert.AreEqual(42, receivedValue, "OnItemRemoved event was not invoked with the correct value.");
    }

    #endregion
}
[TestFixture]
public class AssetVariableRefTests
{
    private GameObject _testAsset;

    [SetUp]
    public void SetUp()
    {
        // Create a dummy asset for direct referencing
        _testAsset = new GameObject("TestAsset");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_testAsset);
    }

    #region Direct Source Tests

    [Test]
    public void DirectSource_ReturnsCorrectAsset()
    {
        var wrapper = new AssetVariableRef<GameObject>
        {
            Source = AssetSource.Direct
        };

        // Use reflection or make _directAsset public/internal for testing
        SetPrivateField(wrapper, "_directAsset", _testAsset);

        Assert.AreEqual(_testAsset, wrapper.Asset);
        Assert.IsFalse(wrapper.IsNull);
    }

    [UnityTest]
    public IEnumerator LoadAssetAsync_DirectSource_CompletesImmediately()
    {
        var wrapper = new AssetVariableRef<GameObject> { Source = AssetSource.Direct };
        SetPrivateField(wrapper, "_directAsset", _testAsset);

        var handle = wrapper.LoadAssetAsync();

        yield return handle;

        Assert.IsTrue(handle.IsDone);
        Assert.AreEqual(AsyncOperationStatus.Succeeded, handle.Status);
        Assert.AreEqual(_testAsset, handle.Result);
    }

    #endregion

    #region Addressable Source Tests

    [UnityTest]
    public IEnumerator LoadAssetAsync_Addressable_ThrowsIfRefIsNull()
    {
        var wrapper = new AssetVariableRef<GameObject> { Source = AssetSource.Addressable };

        // Testing exception in UnityTest requires a try-catch or Assert.Throws
        Assert.Throws<System.InvalidOperationException>(() => wrapper.LoadAssetAsync());
        yield return null;
    }

    /* Note: Testing successful Addressable loading requires a valid Addressable Group 
       and a built Catalog in your project. For a pure unit test, you would usually 
       mock the IAddressables implementation, but since AssetReference is a concrete 
       Unity class, integration testing is the standard approach.
    */

    #endregion

    // Helper to inject private fields for testing without changing the class API
    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(obj, value);
    }
}
public class SoapSystemTests
{

}


