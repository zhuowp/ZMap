using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace ZMap.Core
{
    public abstract class CameraRotateAnimationBase : AnimationTimeline
    {
        #region Fields
        #endregion

        #region Properties

        /// <summary>
        /// Returns the type of the target property
        /// </summary>
        public override sealed Type TargetPropertyType
        {
            get
            {
                ReadPreamble();

                return typeof(CameraLookDirection);
            }
        }

        #endregion

        #region Constructors

        protected CameraRotateAnimationBase() : base()
        {

        }

        #endregion

        #region Private Methods

        #endregion

        #region Protected Methods

        protected abstract CameraLookDirection GetCurrentValueCore(CameraLookDirection defaultOriginValue, CameraLookDirection defaultDestinationValue, AnimationClock animationClock);

        #endregion

        #region Public Methods

        public new CameraRotateAnimationBase Clone()
        {
            return (CameraRotateAnimationBase)base.Clone();
        }

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

            return GetCurrentValue((CameraLookDirection)defaultOriginValue, (CameraLookDirection)defaultDestinationValue, animationClock);
        }

        public CameraLookDirection GetCurrentValue(CameraLookDirection defaultOriginValue, CameraLookDirection defaultDestinationValue, AnimationClock animationClock)
        {
            ReadPreamble();

            if (animationClock == null)
            {
                throw new ArgumentNullException("animationClock");
            }

            if (animationClock.CurrentState == ClockState.Stopped)
            {
                return new CameraLookDirection();
            }

            return GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
        }

        #endregion
    }
}
