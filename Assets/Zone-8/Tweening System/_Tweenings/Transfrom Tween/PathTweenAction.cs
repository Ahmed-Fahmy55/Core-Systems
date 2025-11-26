using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Tweening
{
    public struct PathTweenAction : ITweenAction
    {
        #region CoreSettings
        [field: SerializeField, BoxGroup("Core Settings")]
        public float Duration { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public float Delay { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool Loop { get; set; }

        [field: SerializeField, BoxGroup("Core Settings"), ShowIf(nameof(Loop))]
        public int LoopCount { get; set; }

        [field: SerializeField, BoxGroup("Core Settings"), ShowIf(nameof(Loop))]
        public LoopType LoopType { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool CustomEase { get; set; }

        [field: SerializeField, BoxGroup("Core Settings"), ShowIf(nameof(CustomEase))]
        public AnimationCurve EaseCurve { get; set; }

        [field: SerializeField, BoxGroup("Core Settings"), HideIf(nameof(CustomEase))]
        public Ease Ease { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public UpdateType UpdateType { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool AutoKill { get; set; }
        #endregion

        /////////////////////////////////////////////////////
        [InfoBox("CUBIC BEZIER PATHS\r\nCubicBezier path waypoints must be in multiple of threes, where each group-of-three represents: 1) path waypoint, 2) IN control point (the control point on the previous waypoint), 3) OUT control point (the control point on the new waypoint). Remember that the first waypoint is always auto-added and determined by the target's current position (and has no control points).")]
        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip(" If TRUE the tween will smoothly snap all values to integers.")]
        [SerializeField] bool isLocal;

        [BoxGroup("Path Settings", Order = 1)]
        [SerializeField] Vector3[] wayPoints;

        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip(" The type of path: Linear (straight path), CatmullRom (curved CatmullRom path) or CubicBezier (curved path with 2 control points per each waypoint).")]
        [SerializeField] PathType pathType;

        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip("The path mode, used to determine correct LookAt options: Ignore (ignores any lookAt option passed), 3D, side-scroller 2D, top-down 2D.")]
        [SerializeField] PathMode pathMode;

        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip(" The resolution of the path (useless in case of Linear paths): higher resolutions make for more detailed curved paths but are more expensive. Defaults to 10, but a value of 5 is usually enough if you don't have dramatic long curves between waypoints.")]
        [SerializeField] int resolution;

        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip("The color of the path (shown when gizmos are active in the Play panel and the tween is running).")]
        [SerializeField] Color gizomColor;

        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null");
                return null;
            }


            Tween tween;
            if (isLocal)
            {
                tween = target.transform.DOLocalPath(wayPoints, Duration, pathType, pathMode, resolution, gizomColor);
            }
            else
            {
                tween = target.transform.DOPath(wayPoints, Duration, pathType, pathMode, resolution, gizomColor);
            }

            tween.SetDelay(Delay).SetUpdate(UpdateType).SetAutoKill(AutoKill);

            if (CustomEase)
            {
                tween.SetEase(EaseCurve);
            }
            else
            {
                tween.SetEase(Ease);
            }

            if (Loop) tween.SetLoops(LoopCount, LoopType);

            return tween;
        }

    }
}
