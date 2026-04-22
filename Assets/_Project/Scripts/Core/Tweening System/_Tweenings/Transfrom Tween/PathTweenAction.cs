using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Tweening
{
    public struct PathTweenAction : ITweenAction
    {
        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [InfoBox("CUBIC BEZIER PATHS\r\nCubicBezier path waypoints must be in multiple of threes, where each group-of-three represents: 1) path waypoint, 2) IN control point (the control point on the previous waypoint), 3) OUT control point (the control point on the new waypoint). Remember that the first waypoint is always auto-added and determined by the target's current position (and has no control points).")]
        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip(" If TRUE the tween will smoothly snap all values to integers.")]
        [SerializeField] bool _isLocal;

        [BoxGroup("Path Settings", Order = 1)]
        [SerializeField] Vector3[] _wayPoints;

        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip(" The type of path: Linear (straight path), CatmullRom (curved CatmullRom path) or CubicBezier (curved path with 2 control points per each waypoint).")]
        [SerializeField] PathType _pathType;

        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip("The path mode, used to determine correct LookAt options: Ignore (ignores any lookAt option passed), 3D, side-scroller 2D, top-down 2D.")]
        [SerializeField] PathMode _pathMode;

        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip(" The resolution of the path (useless in case of Linear paths): higher resolutions make for more detailed curved paths but are more expensive. Defaults to 10, but a value of 5 is usually enough if you don't have dramatic long curves between waypoints.")]
        [SerializeField] int _resolution;

        [BoxGroup("Path Settings", Order = 1)]
        [Tooltip("The color of the path (shown when gizmos are active in the Play panel and the tween is running).")]
        [SerializeField] Color _gizomColor;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null");
                return null;
            }


            Tween tween;
            if (_isLocal)
            {
                tween = target.transform.DOLocalPath(_wayPoints, CoreSettings.Duration, _pathType, _pathMode, _resolution, _gizomColor);
            }
            else
            {
                tween = target.transform.DOPath(_wayPoints, CoreSettings.Duration, _pathType, _pathMode, _resolution, _gizomColor);
            }

            CoreSettings.Apply(tween);

            return tween;
        }

    }
}
