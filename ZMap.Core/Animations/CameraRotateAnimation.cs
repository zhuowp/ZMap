using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private double[] _progressBegins;

        private AnimationType _animationType;
        private bool _isAnimationFunctionValid;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty FromProperty;
        public static readonly DependencyProperty ToProperty;
        public static readonly DependencyProperty TosProperty;
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

        public IEnumerable<CameraLookDirection> Tos
        {
            get
            {
                return (IEnumerable<CameraLookDirection>)GetValue(TosProperty);
            }
            set
            {
                SetValue(TosProperty, value);
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

        /// <summary>
        /// 静态构造方法
        /// </summary>
        static CameraRotateAnimation()
        {
            Type typeofLookDirection = typeof(CameraLookDirection?);
            Type typeOfAnimation = typeof(CameraRotateAnimation);

            PropertyChangedCallback propCallback = new PropertyChangedCallback(LookDirectionPropertiesChanged);
            ValidateValueCallback validateCallback = new ValidateValueCallback(ValidateFromToValue);

            FromProperty =
                DependencyProperty.Register("From", typeofLookDirection, typeOfAnimation, new PropertyMetadata(null, propCallback), validateCallback);
            ToProperty =
                DependencyProperty.Register("To", typeofLookDirection, typeOfAnimation, new PropertyMetadata(null, propCallback), validateCallback);
            TosProperty =
                DependencyProperty.Register("Tos", typeof(IEnumerable<CameraLookDirection>), typeOfAnimation,
                new PropertyMetadata(null, propCallback), TosValidateCallback);
            EasingFunctionProperty =
                DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeOfAnimation);
        }

        /// <summary>
        /// 无参构造方法
        /// </summary>
        public CameraRotateAnimation()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toValue"></param>
        /// <param name="duration"></param>
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

        public CameraRotateAnimation(IEnumerable<CameraLookDirection> tosValue, Duration duration)
        {
            Tos = tosValue;
            Duration = duration;
        }

        public CameraRotateAnimation(IEnumerable<CameraLookDirection> tosValue, Duration duration, FillBehavior fillBehavior)
        {
            Tos = tosValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }

        public CameraRotateAnimation(CameraLookDirection fromValue, IEnumerable<CameraLookDirection> tosValue, Duration duration)
        {
            From = fromValue;
            Tos = tosValue;
            Duration = duration;
        }

        public CameraRotateAnimation(CameraLookDirection fromValue, IEnumerable<CameraLookDirection> tosValue, Duration duration, FillBehavior fillBehavior)
        {
            From = fromValue;
            Tos = tosValue;
            Duration = duration;
            FillBehavior = FillBehavior;
        }

        #endregion

        #region Private Methods

        private static void LookDirectionPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CameraRotateAnimation a = (CameraRotateAnimation)d;
            a._isAnimationFunctionValid = false;
        }

        private static bool TosValidateCallback(object value)
        {
            IEnumerable<CameraLookDirection> toDirections = value as IEnumerable<CameraLookDirection>;
            if (toDirections != null)
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        private static bool ValidateFromToValue(object value)
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

        private void ValidateAnimationFunction(CameraLookDirection defaultOriginValue, CameraLookDirection defaultDestinationValue)
        {
            _animationType = AnimationType.Automatic;
            _keyValues = null;
            _progressBegins = null;

            if (From.HasValue)
            {
                if (To.HasValue)
                {
                    _animationType = AnimationType.FromTo;
                    _keyValues = new CameraLookDirection[2];
                    _keyValues[0] = From.Value;
                    _keyValues[1] = To.Value;
                }
                else if (Tos != null && Tos.Count() != 0)
                {
                    _animationType = AnimationType.FromTo;
                    _keyValues = new CameraLookDirection[Tos.Count() + 1];
                    _keyValues[0] = From.Value;

                    int index = 1;
                    foreach (CameraLookDirection lookDirection in Tos)
                    {
                        _keyValues[index] = lookDirection;
                        index++;
                    }
                }
                else
                {
                    _animationType = AnimationType.FromTo;
                    _keyValues = new CameraLookDirection[2];
                    _keyValues[0] = From.Value;
                    _keyValues[1] = defaultDestinationValue;
                }
            }
            else if (To.HasValue)
            {
                _animationType = AnimationType.FromTo;
                _keyValues = new CameraLookDirection[2];
                _keyValues[0] = defaultOriginValue;
                _keyValues[1] = To.Value;
            }
            else if (Tos != null && Tos.Count() != 0)
            {
                _animationType = AnimationType.FromTo;
                _keyValues = new CameraLookDirection[Tos.Count() + 1];
                _keyValues[0] = defaultOriginValue;

                int index = 1;
                foreach (CameraLookDirection lookDirection in Tos)
                {
                    _keyValues[index] = lookDirection;
                    index++;
                }
            }
            else
            {
                _animationType = AnimationType.FromTo;
                _keyValues = new CameraLookDirection[2];
                _keyValues[0] = defaultOriginValue;
                _keyValues[1] = defaultDestinationValue;
            }

            if (_keyValues != null && _keyValues.Length > 0)
            {
                int valueCount = _keyValues.Length;

                //计算总的旋转角度
                double totalAngle = 0;
                double[] anglesToOriginDirection = new double[valueCount];
                for (int i = 1; i < valueCount; i++)
                {
                    double angleToPreviousDirection = Vector3D.AngleBetween(_keyValues[i - 1].LookDirection, _keyValues[i].LookDirection);
                    anglesToOriginDirection[i] = anglesToOriginDirection[i - 1] + angleToPreviousDirection;

                    totalAngle += angleToPreviousDirection;
                }

                _progressBegins = new double[valueCount];
                for (int i = 1; i < valueCount; i++)
                {
                    _progressBegins[i] = anglesToOriginDirection[i] / totalAngle;
                }
            }
            _isAnimationFunctionValid = true;
        }

        #endregion

        #region Protected Methods

        protected override Freezable CreateInstanceCore()
        {
            return new CameraRotateAnimation();
        }

        protected override CameraLookDirection GetCurrentValueCore(CameraLookDirection defaultOriginValue, CameraLookDirection defaultDestinationValue, AnimationClock animationClock)
        {
            if (!_isAnimationFunctionValid)
            {
                ValidateAnimationFunction(defaultOriginValue, defaultDestinationValue);
            }

            double progress = animationClock.CurrentProgress.Value;

            IEasingFunction easingFunction = EasingFunction;
            if (easingFunction != null)
            {
                progress = easingFunction.Ease(progress);
            }

            int sectionIndex = 0;
            if (progress >= 1)
            {
                progress = 1;
                sectionIndex = _progressBegins.Length - 2;
            }
            else
            {
                for (int i = 0; i < _progressBegins.Length - 1; i++)
                {
                    if (_progressBegins[i] <= progress && _progressBegins[i + 1] > progress)
                    {
                        sectionIndex = i;
                        break;
                    }
                }
            }

            CameraLookDirection from = _keyValues[sectionIndex];
            CameraLookDirection to = _keyValues[sectionIndex + 1];
            double progressBetweenCurrentDirections = (progress - _progressBegins[sectionIndex]) / (_progressBegins[sectionIndex + 1] - _progressBegins[sectionIndex]);

            Vector3D rotateAxis = Vector3D.CrossProduct(from.LookDirection, to.LookDirection);
            double fullRotateAngle = Vector3D.AngleBetween(from.LookDirection, to.LookDirection);
            double rotateAngle = fullRotateAngle * progressBetweenCurrentDirections;

            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            rotateTransform3D.Rotation = new AxisAngleRotation3D(rotateAxis, rotateAngle);
            Matrix3D matrix = rotateTransform3D.Value;

            Point3D newCameraLookPoint = matrix.Transform(new Point3D(from.LookDirection.X, from.LookDirection.Y, from.LookDirection.Z));
            Vector3D newCameraLookDirection = new Vector3D(newCameraLookPoint.X, newCameraLookPoint.Y, newCameraLookPoint.Z);

            Vector3D horizontalVector3D = Vector3D.CrossProduct(new Vector3D(0, -1, 0), newCameraLookDirection);
            Vector3D upDirection = Vector3D.CrossProduct(horizontalVector3D, newCameraLookDirection);

            CameraLookDirection cameraLookDirection = new CameraLookDirection(newCameraLookDirection, upDirection);
            return cameraLookDirection;
        }

        private Vector3D _oldLookDirection;

        #endregion

        #region Public Methods

        public new CameraRotateAnimation Clone()
        {
            return (CameraRotateAnimation)base.Clone();
        }

        #endregion
    }
}
