using Zone8.Selection;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class TestSelection : SerializedMonoBehaviour
{
    [SerializeField] private ISelectionHandler _selectionHandler;



    private void Awake()
    {
        _selectionHandler.SelectionCompleted += SelectionHandler_SelectionCompleted;
    }

    private void SelectionHandler_SelectionCompleted(List<ISelectable> selectables)
    {
        Debug.Log($"Selection Completed: {selectables.Count} items selected.");
    }
}
