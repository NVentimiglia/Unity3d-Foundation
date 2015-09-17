// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------


using UnityEngine;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Vector3 --> Transform
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/TransformBinder")]
    public class TransformBinder : BindingBase
    {
        protected bool _doPosition;
        protected bool _doRotation;
        protected bool _doScale;
        protected Vector3 _position;
        protected Quaternion _rotation;
        protected Vector3 _scale;

        [HideInInspector] public BindingInfo PositionBinding = new BindingInfo
        {
            BindingName = "Position",
            Filters = BindingFilter.Properties,
            FilterTypes = new[] {typeof (Vector3)}
        };

        [HideInInspector] public BindingInfo RotationBinding = new BindingInfo
        {
            BindingName = "Rotation",
            Filters = BindingFilter.Properties,
            FilterTypes = new[] {typeof (Quaternion)}
        };

        [HideInInspector] public BindingInfo ScaleBinding = new BindingInfo
        {
            BindingName = "Scale",
            Filters = BindingFilter.Properties,
            FilterTypes = new[] {typeof (Vector3)}
        };

        protected Transform Transform;

        private void Awake()
        {
            Init();
        }

        public override void Init()
        {
            Transform = transform;
            PositionBinding.Action = UpdatePosition;
            PositionBinding.Action = UpdateRotation;
            PositionBinding.Action = UpdateScale;
        }

        private void UpdatePosition(object arg)
        {
            Transform.position = (Vector3) arg;
        }

        private void UpdateRotation(object arg)
        {
            Transform.rotation = (Quaternion) arg;
        }

        private void UpdateScale(object arg)
        {
            Transform.localScale = (Vector3) arg;
        }
    }
}