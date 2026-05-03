using UnityEngine;
using UnityEngine.EventSystems;
using Zone8.Audio.Data;

namespace Zone8.Audio.Components
{
    [RequireComponent(typeof(RectTransform))]
    public class UISounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [Header("Audio Data")]
        [SerializeField] private SFXClipSo _hoverSound;
        [SerializeField] private SFXClipSo _clickSound;

        [field: SerializeField] public bool Mute { get; set; }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Mute) return;

            _clickSound?.Play();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Mute) return;

            _hoverSound?.Play();
        }
    }
}