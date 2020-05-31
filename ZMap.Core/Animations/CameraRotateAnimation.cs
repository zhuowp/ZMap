using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace ZMap.Core
{
    public class CameraRotateAnimation : CameraRotateAnimationBase
    {
        #region Fields

        private CameraLookDirection[] _keyValues;
        private AnimationType _animationType;
        private bool _isAnimationFunctionValid;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty FromProperty;
        public static readonly DependencyProperty ToProperty;
        public static readonly DependencyProperty ByProperty;
        public static readonly DependencyProperty EasingFunctionProperty;

        #endregion

        #region Dependency Property Wrappers

        public CameraLookDirection? From
        {
            get
            {
                return (CameraLookDirection?)GetValue(FromProperty);
            }
            set
            {
                SetValue(FromProperty, value);
            }
        }

        public CameraLookDirection? To
        {
            get
            {
                return (CameraLookDirection?)GetValue(ToProperty);
            }
            set
            {
                SetValue(ToProperty, value);
            }
        }

        public CameraLookDirection? By
        {
            get
            {
                return (CameraLookDirection?)GetValue(ByProperty);
            }
            set
            {
                SetValue(ByProperty, value);
            }
        }

        public IEasingFunction EasingFunction
        {
            get
            {
                return (IEasingFunction)GetValue(EasingFunctionProperty);
            }
            set
            {
                SetValue(EasingFunctionProperty, value);
            }
        }

        public bool IsAdditive
        {
            get
            {
                return (bool)GetValue(IsAdditiveProperty);
            }
            set
            {
                SetValue(IsAdditiveProperty, value);
            }
        }

        public bool IsCumulative
        {
            get
            {
                return (bool)GetValue(IsCumulativeProperty);
            }
            set
            {
                SetValue(IsCumulativeProperty, value);
            }
        }

        #endregion

        #region Constructors

        static CameraRotateAnimation()
        {
            Type typeofProp = typeof(CameraLookDirection?);
            Type typeofThis = typeof(CameraRotateAnimation);

            PropertyChangedCallback propCallback = new PropertyChangedCallback(AnimationFunction_Changed);
            ValidateValueCallback validateCallback = new ValidateValueCallback(ValidateFromToOrByValue);

            FromProperty =
                DependencyProperty.Register("From", typeofProp, typeofThis, new PropertyMetadata(null, propCallback), validateCallback);
            ToProperty =
                DependencyProperty.Register("To", typeofProp, typeofThis, new PropertyMetadata(null, propCallback), validateCallback);
            ByProperty =
                DependencyProperty.Register("By", typeofProp, typeofThis, new PropertyMetadata(null, propCallback), validateCallback);
            EasingFunctionProperty =
                DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeofThis);
        }

        /// <summary>
        /// Creates a new DoubleAnimation with all properties set to
        /// their default values.
        /// </summary>
        public CameraRotateAnimation()
            : base()
        {
        }

        /// <summary>
        /// Creates a new DoubleAnimation that will animate a
        /// Double property from its base value to the value specified
        /// by the "toValue" parameter of this constructor.
        /// </summary>
        public CameraRotateAnimation(CameraLookDirection toValue, Duration duration)
            : this()
        {
            To = toValue;
            Duration = duration;
        }

        /// <summary>
        /// Creates a new DoubleAnimation that will animate a
        /// Double property from its base value to the value specified
        /// by the "toValue" parameter of this constructor.
        /// </summary>
        public CameraRotateAnimation(CameraLookDirection toValue, Duration duration, FillBehavior fillBehavior)
            : this()
        {
            To = toValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }

        /// <summary>
        /// Creates a new DoubleAnimation that will animate a
        /// Double property from the "fromValue" parameter of this constructor
        /// to the "toValue" parameter.
        /// </summary>
        public CameraRotateAnimation(CameraLookDirection fromValue, CameraLookDirection toValue, Duration duration)
            : this()
        {
            From = fromValue;
            To = toValue;
            Duration = duration;
        }

        /// <summary>
        /// Creates a new DoubleAnimation that will animate a
        /// Double property from the "fromValue" parameter of this constructor
        /// to the "toValue" parameter.
        /// </summary>
        public CameraRotateAnimation(CameraLookDirection fromValue, CameraLookDirection toValue, Duration duration, FillBehavior fillBehavior)
            : this()
        {
            From = fromValue;
            To = toValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }

        #endregion

        #region Private Methods

        private static void AnimationFunction_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CameraRotateAnimation a = (CameraRotateAnimation)d;
            a._isAnimationFunctionValid = false;
        }

        private static bool ValidateFromToOrByValue(object value)
        {
            CameraLookDirection? typedValue = (CameraLookDirection?)value;

            if (typedValue.HasValue)
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Freezable

        public new CameraRotateAnimation Clone()
        {
            return (CameraRotateAnimation)base.Clone();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CameraRotateAnimation();
        }

        #endregion

        #region Methods

        protected override CameraLookDirection GetCurrentValueCore(CameraLookDirection defaultOriginValue, CameraLookDirection defaultDestinationValue, AnimationClock animationClock)
        {
            Debug.Assert(animationClock.CurrentState != ClockState.Stopped);

            if (!_isAnimationFunctionValid)
            {
                ValidateAnimationFunction();
            }

            double progress = animationClock.CurrentProgress.Value;

            IEasingFunction easingFunction = EasingFunction;
            if (easingFunction != null)
            {
                progress = easingFunction.Ease(progress);
            }

            CameraLookDirection from = new CameraLookDirection();
            CameraLookDirection to = new CameraLookDirection();

            switch (_animationType)
            {
                case AnimationType.Automatic:
                    from = defaultOriginValue;
                    to = defaultDestinationValue;
                    break;
                case AnimationType.From:
                    from = _keyValues[0];
                    to = defaultDestinationValue;
                    break;
                case AnimationType.To:
                    from = defaultOriginValue;
                    to = _keyValues[0];
                    break;
                case AnimationType.By:
                    to = _keyValues[0];
                    break;
                case AnimationType.FromTo:
                    from = _keyValues[0];
                    to = _keyValues[1];
                    break;
                default:
                    throw new Exception("Unknown animation type.");
            }

            Vector3D lookDirection = from.LookDirection + (to.LookDirection - from.LookDirection) * progress;
            Vector3D horizontalVector3D = Vector3D.CrossProduct(new Vector3D(0, -1, 0), lookDirection);
            Vector3D upDirection = Vector3D.CrossProduct(horizontalVector3D, lookDirection);

            CameraLookDirection cameraLookDirection = new CameraLookDirection(lookDirection, upDirection);
            return cameraLookDirection;
        }

        private void ValidateAnimationFunction()
        {
            _animationType = AnimationType.Automatic;
            _keyValues = null;

            if (From.HasValue)
            {
                if (To.HasValue)
                {
                    _animationType = AnimationType.FromTo;
                    _keyValues = new CameraLookDirection[2];
                    _keyValues[0] = From.Value;
                    _keyValues[1] = To.Value;
                }
                else if (By.HasValue)
                {
                    _animationType = AnimationType.FromBy;
                    _keyValues = new CameraLookDirection[2];
                    _keyValues[0] = From.Value;
                    _keyValues[1] = By.Value;
                }
                else
                {
                    _animationType = AnimationType.From;
                    _keyValues = new CameraLookDirection[1];
                    _keyValues[0] = From.Value;
                }
            }
            else if (To.HasValue)
            {
                _animationType = AnimationType.To;
                _keyValues = new CameraLookDirection[1];
                _keyValues[0] = To.Value;
            }
            else if (By.HasValue)
            {
                _animationType = AnimationType.By;
                _keyValues = new CameraLookDirection[1];
                _keyValues[0] = By.Value;
            }

            _isAnimationFunctionValid = true;
        }

        #endregion

        #region Properties

        #endregion
    }
}
