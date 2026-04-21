using System;
using System.Collections.Generic;

namespace Zone8.Selection
{
    public interface ISelectionHandler
    {
        event Action NoItemSelected;
        event Action<ISelectable> ItemSelected;
        event Action<ISelectable> ItemDeselected;
        event Action<List<ISelectable>> SelectionCompleted;
        void CompleteSelection();
    }
}
