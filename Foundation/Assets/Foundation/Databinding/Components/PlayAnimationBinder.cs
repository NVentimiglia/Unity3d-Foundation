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
    ///     String --> Animation.PlayAnimation
    /// </summary>
    [AddComponentMenu("Foundation/Databinding/PlayAnimationBinder")]
    public class PlayAnimationBinder : BindingBase
    {
        protected Animation Anim;

        [HideInInspector] public BindingInfo AnimationBindingInfo = new BindingInfo
        {
            BindingName = "IsHit",
            Filters = BindingFilter.Properties,
            FilterTypes = new[] {typeof (bool)}
        };

        private void Awake()
        {
            Init();
        }

        public override void Init()
        {
            Anim = GetComponent<Animation>();
            AnimationBindingInfo.Action = UpdatePosition;
        }

        private void UpdatePosition(object arg)
        {
            if ((bool) arg)
            {
                Anim.Rewind();
                Anim.Play();
            }
        }
    }
}