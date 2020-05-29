using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace ZMap.Core
{
    public abstract class Matrix3DAnimationBase : AnimationTimeline
    {
        #region Constructors

        protected Matrix3DAnimationBase() : base()
        {
        }

        #endregion

        #region Freezable

        public new Matrix3DAnimationBase Clone()
        {
            return (Matrix3DAnimationBase)base.Clone();
        }

        #endregion

        #region IAnimation

        public override sealed object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (defaultOriginValue == null)
            {
                throw new ArgumentNullException("defaultOriginValue");
            }
            if (defaultDestinationValue == null)
            {
                throw new ArgumentNullException("defaultDestinationValue");
            }
            return GetCurrentValue((Matrix3D)defaultOriginValue, (Matrix3D)defaultDestinationValue, animationClock);
        }

        /// <summary>
        /// Returns the type of the target property
        /// </summary>
        public override sealed Type TargetPropertyType
        {
            get
            {
                ReadPreamble();

                return typeof(Matrix3D);
            }
        }

        #endregion

        #region Methods

        public Matrix3D GetCurrentValue(Matrix3D defaultOriginValue, Matrix3D defaultDestinationValue, AnimationClock animationClock)
        {
            ReadPreamble();

            if (animationClock == null)
            {
                throw new ArgumentNullException("animationClock");
            }

            if (animationClock.CurrentState == ClockState.Stopped)
            {
                return defaultDestinationValue;
            }

            return GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
        }

        protected abstract Matrix3D GetCurrentValueCore(Matrix3D defaultOriginValue, Matrix3D defaultDestinationValue, AnimationClock animationClock);

        #endregion
    }
}
