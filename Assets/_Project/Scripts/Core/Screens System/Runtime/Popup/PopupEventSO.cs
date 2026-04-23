using UnityEngine;
using Zone8.SOAP.Events;

namespace Zone8.Screens
{
    [CreateAssetMenu(fileName = "PopupEvent", menuName = "Screens/Popup Event")]
    public class PopupEventSO : GameEvent<PopupEventArgs>
    {

    }

    public struct PopupEventArgs
    {
        public PopupSO Popup;
        public bool Show;

        public PopupEventArgs(PopupSO popup, bool show)
        {
            Popup = popup;
            Show = show;
        }
    }
}