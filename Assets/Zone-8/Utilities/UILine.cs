using UnityEngine;
using UnityEngine.UI;

namespace Zone8.Utilities
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UILine : MaskableGraphic
    {

        [SerializeField] private RectTransform _startPoint;
        [SerializeField] private RectTransform _endPoint;

        public float Thickness = 4f;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_startPoint == null || _endPoint == null)
                return;

            Vector2 startPos = WorldToCanvasPosition(_startPoint);
            Vector2 endPos = WorldToCanvasPosition(_endPoint);

            Vector2 direction = (endPos - startPos).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (Thickness / 2f);

            UIVertex[] verts = new UIVertex[4];
            verts[0].position = startPos + normal;
            verts[1].position = startPos - normal;
            verts[2].position = endPos - normal;
            verts[3].position = endPos + normal;

            for (int i = 0; i < 4; i++)
                verts[i].color = color;

            vh.AddUIVertexQuad(verts);
        }

        private Vector2 WorldToCanvasPosition(RectTransform target)
        {
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform,
                screenPos,
                cam,
                out Vector2 localPos);
            return localPos;
        }



        public void SetPoints(RectTransform start, RectTransform end)
        {
            _startPoint = start;
            _endPoint = end;
            SetVerticesDirty();
        }
    }
}
