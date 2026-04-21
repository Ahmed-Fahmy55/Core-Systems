using UnityEngine;

namespace Zone8.Screens
{
    [CreateAssetMenu(fileName = "Popup", menuName = "Bltzo/Screens/Popup")]
    public class PopupSO : ScriptableObject
    {
        public string Name;
        public Popup PopupPrefab;
        public PopupEventSO PopupEvent;

        public void Show()
        {
            PopupEvent.Raise(new(this, true));
        }

        public void Hide()
        {
            PopupEvent.Raise(new(this, false));
        }
    }
}
