using UnityEngine;
using UnityEngine.UI;

namespace Zone8.Utilities
{
    [RequireComponent(typeof(Image))]
    public class ImageLine : MonoBehaviour
    {

        private RectTransform line;

        private void Awake()
        {
            line = GetComponent<RectTransform>();
        }

        public void DrawUILine(RectTransform start, RectTransform end)
        {
            Vector2 startPos = start.position;
            Vector2 endPos = end.position;
            Vector2 dir = endPos - startPos;

            line.sizeDelta = new Vector2(dir.magnitude, line.sizeDelta.y);
            line.position = startPos + dir / 2f;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            line.rotation = Quaternion.Euler(0, 0, angle);
        }

    }
}
