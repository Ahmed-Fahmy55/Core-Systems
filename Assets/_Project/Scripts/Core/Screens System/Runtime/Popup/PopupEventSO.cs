using Zone8.SOAP.Events;
using UnityEngine;

namespace Zone8.Screens
{
    [CreateAssetMenu(fileName = "PopupEvent", menuName = "Bltzo/Screens/Popup Event")]
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