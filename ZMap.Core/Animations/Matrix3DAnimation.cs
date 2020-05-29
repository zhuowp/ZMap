using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Automation.Text;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace ZMap.Core
{
    public class Matrix3DAnimation : Matrix3DAnimationBase
    {
        #region Fields

        private Matrix3D[] _keyValues;
        private AnimationType _animationType;
        private bool _isAnimationFunctionValid;

        #endregion

        #region Constructors

        static Matrix3DAnimation()
        {
            Type typeofProp = typeof(Matrix3D?);
            Type typeofThis = typeof(Matrix3DAnimation);
            PropertyChangedCallback propCallback = new PropertyChangedCallback(AnimationFunction_Changed);
            ValidateValueCallback validateCallback = new ValidateValueCallback(ValidateFromToOrByValue);

            FromProperty = DependencyProperty.Register(
                "From",
                typeofProp,
                typeofThis,
                new PropertyMetadata((Matrix3D?)null, propCallback),
                validateCallback);

            ToProperty = DependencyProperty.Register(
                "To",
                typeofProp,
                typeofThis,
                new PropertyMetadata((Matrix3D?)null, propCallback),
                validateCallback);

            ByProperty = DependencyProperty.Register(
                "By",
                typeofProp,
                typeofThis,
                new PropertyMetadata((Matrix3D?)null, propCallback),
                validateCallback);

            EasingFunctionProperty = DependencyProperty.Register(
                "EasingFunction",
                typeof(IEasingFunction),
                typeofThis);
        }

        /// <summary>
        /// Creates a new DoubleAnimation with all properties set to
        /// their default values.
        /// </summary>
        public Matrix3DAnimation()
            : base()
        {
        }

        /// <summary>
        /// Creates a new DoubleAnimation that will animate a
        /// Double property from its base value to the value specified
        /// by the "toValue" parameter of this constructor.
        /// </summary>
        public Matrix3DAnimation(Matrix3D toValue, Duration duration)
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
        public Matrix3DAnimation(Matrix3D toValue, Duration duration, FillBehavior fillBehavior)
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
        public Matrix3DAnimation(Matrix3D fromValue, Matrix3D toValue, Duration duration)
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
        public Matrix3DAnimation(Matrix3D fromValue, Matrix3D toValue, Duration duration, FillBehavior fillBehavior)
            : this()
        {
            From = fromValue;
            To = toValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }

        #endregion

        #region Freezable

        public new Matrix3DAnimation Clone()
        {
            return (Matrix3DAnimation)base.Clone();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Matrix3DAnimation();
        }

        #endregion

        #region Methods

        protected override Matrix3D GetCurrentValueCore(Matrix3D defaultOriginValue, Matrix3D defaultDestinationValue, AnimationClock animationClock)
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

            Matrix3D from = new Matrix3D();
            Matrix3D to = new Matrix3D();

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
                    Debug.Fail("Unknown animation type.");
                    break;
            }

            Matrix3D matrix3D = from;
            matrix3D.M11 = from.M11 + (to.M11 - from.M11) * progress;
            matrix3D.M12 = from.M12 + (to.M12 - from.M12) * progress;
            matrix3D.M13 = from.M13 + (to.M13 - from.M13) * progress;
            matrix3D.M14 = from.M14 + (to.M14 - from.M14) * progress;
            matrix3D.M21 = from.M21 + (to.M21 - from.M21) * progress;
            matrix3D.M22 = from.M22 + (to.M22 - from.M22) * progress;
            matrix3D.M23 = from.M23 + (to.M23 - from.M23) * progress;
            matrix3D.M24 = from.M24 + (to.M24 - from.M24) * progress;
            matrix3D.M31 = from.M31 + (to.M31 - from.M31) * progress;
            matrix3D.M32 = from.M32 + (to.M32 - from.M32) * progress;
            matrix3D.M33 = from.M33 + (to.M33 - from.M33) * progress;
            matrix3D.M34 = from.M34 + (to.M34 - from.M34) * progress;
            matrix3D.M44 = from.M44 + (to.M44 - from.M44) * progress;

            matrix3D.OffsetX = from.OffsetX + (to.OffsetX - from.OffsetX) * progress;
            matrix3D.OffsetY = from.OffsetY + (to.OffsetY - from.OffsetY) * progress;
            matrix3D.OffsetZ = from.OffsetZ + (to.OffsetZ - from.OffsetZ) * progress;

            return matrix3D;
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
                    _keyValues = new Matrix3D[2];
                    _keyValues[0] = From.Value;
                    _keyValues[1] = To.Value;
                }
                else if (By.HasValue)
                {
                    _animationType = AnimationType.FromBy;
                    _keyValues = new Matrix3D[2];
                    _keyValues[0] = From.Value;
                    _keyValues[1] = By.Value;
                }
                else
                {
                    _animationType = AnimationType.From;
                    _keyValues = new Matrix3D[1];
                    _keyValues[0] = From.Value;
                }
            }
            else if (To.HasValue)
            {
                _animationType = AnimationType.To;
                _keyValues = new Matrix3D[1];
                _keyValues[0] = To.Value;
            }
            else if (By.HasValue)
            {
                _animationType = AnimationType.By;
                _keyValues = new Matrix3D[1];
                _keyValues[0] = By.Value;
            }

            _isAnimationFunctionValid = true;
        }

        #endregion

        #region Properties

        private static void AnimationFunction_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Matrix3DAnimation a = (Matrix3DAnimation)d;
            a._isAnimationFunctionValid = false;
        }

        private static bool ValidateFromToOrByValue(object value)
        {
            Matrix3D? typedValue = (Matrix3D?)value;

            if (typedValue.HasValue)
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        public static readonly DependencyProperty FromProperty;

        public Matrix3D? From
        {
            get
            {
                return (Matrix3D?)GetValue(FromProperty);
            }
            set
            {
                SetValue(FromProperty, value);
            }
        }

        public static readonly DependencyProperty ToProperty;

        public Matrix3D? To
        {
            get
            {
                return (Matrix3D?)GetValue(ToProperty);
            }
            set
            {
                SetValue(ToProperty, value);
            }
        }

        public static readonly DependencyProperty ByProperty;

        public Matrix3D? By
        {
            get
            {
                return (Matrix3D?)GetValue(ByProperty);
            }
            set
            {
                SetValue(ByProperty, value);
            }
        }

        public static readonly DependencyProperty EasingFunctionProperty;

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
    }

}
