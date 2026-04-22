using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{

    [Serializable]
    public struct RotationTweenAction : ITweenAction
    {

        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Rotation Settings", Order = 1)]
        [Tooltip("Rotates the target so that it will look towards the given position")]
        [SerializeField] private bool isLookAt;

        [BoxGroup("Rotation Settings", Order = 1), HideIf(nameof(isLookAt))]
        [SerializeField] private bool isLocal;

        [BoxGroup("Rotation Settings", Order = 1)]
        [SerializeField] Vector3 value;

        [BoxGroup("Rotation Settings", Order = 1), ShowIf(nameof(isLookAt))]
        [Tooltip(" Eventual axis constraint for the rotation.\r\nDefault: AxisConstraint.None")]
        [SerializeField] private AxisConstraint axisConstraint;

        [BoxGroup("Rotation Settings", Order = 1), ShowIf(nameof(isLookAt))]
        [Tooltip(" Up vector")]
        [SerializeField] private Vector3 up;

        [BoxGroup("Rotation Settings", Order = 1), HideIf(nameof(isLookAt))]
        [Tooltip(" Indicates the rotation mode.\r\nFast (default): the rotation will take the shortest route and will not rotate more than 360°.\r\nFastBeyond360: The rotation will go beyond 360°.\r\nWorldAxisAdd: Adds the given rotation to the transform using world axis and an advanced precision mode (like when using transform.Rotate(Space.World)). In this mode the end value is always considered relative.\r\nLocalAxisAdd: Adds the given rotation to the transform's local axis (like when rotating an object with the \"local\" switch enabled in Unity's editor or using transform.Rotate(Space.Self)). In this mode the end value is is always considered relative.")]
        [SerializeField] private RotateMode rotateMode;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null");
                return null;
            }

            Tween tween;
            if (isLookAt)
            {
                tween = target.transform.DOLookAt(value, CoreSettings.Duration, axisConstraint, up);
            }
            else
            {
                if (isLocal)
                {
                    tween = target.transform.DOLocalRotate(value, CoreSettings.Duration, rotateMode);
                }
                else
                {
                    tween = target.transform.DORotate(value, CoreSettings.Duration, rotateMode);
                }
            }

            CoreSettings.Apply(tween);

            return tween;
        }
    }
}
