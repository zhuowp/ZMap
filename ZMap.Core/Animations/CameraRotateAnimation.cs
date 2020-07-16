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
        private Vector3D[] _rotateAxesOfSection;
        private double[] _anglesOfSection;

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

        private void TryValidateAnimationFunction(CameraLookDirection defaultOriginValue, CameraLookDirection defaultDestinationValue)
        {
            if (_isAnimationFunctionValid)
            {
                return;
            }

            InitAnimationKeyValues(defaultOriginValue, defaultDestinationValue);

            if (_keyValues != null && _keyValues.Length > 0)
            {
                int keyValueCount = _keyValues.Length;
                int sectionCount = keyValueCount - 1;
                _rotateAxesOfSection = new Vector3D[sectionCount];
                for (int i = 0; i < sectionCount; i++)
                {
                    _rotateAxesOfSection[i] = Vector3D.CrossProduct(_keyValues[i].LookDirection, _keyValues[i + 1].LookDirection);
                }

                _anglesOfSection = new double[sectionCount];

                //计算每一个旋转片段的旋转角
                for (int i = 0; i < sectionCount; i++)
                {
                    _anglesOfSection[i] = Vector3D.AngleBetween(_keyValues[i].LookDirection, _keyValues[i + 1].LookDirection);
                }

                //计算总的旋转角度
                double totalAngle = _anglesOfSection.Sum();

                //计算每一个关键点的指向与初值关键点间的旋转角的总和
                double[] rotateAnglesToOriginDirection = new double[keyValueCount];
                for (int i = 0; i < sectionCount; i++)
                {
                    rotateAnglesToOriginDirection[i + 1] = rotateAnglesToOriginDirection[i] + _anglesOfSection[i];
                }

                _progressBegins = new double[keyValueCount];
                for (int i = 1; i < keyValueCount; i++)
                {
                    _progressBegins[i] = rotateAnglesToOriginDirection[i] / totalAngle;
                }
            }

            _isAnimationFunctionValid = true;
        }

        /// <summary>
        /// 初始化动画关键点值
        /// </summary>
        /// <param name="defaultOriginValue"></param>
        /// <param name="defaultDestinationValue"></param>
        private void InitAnimationKeyValues(CameraLookDirection defaultOriginValue, CameraLookDirection defaultDestinationValue)
        {
            if (From.HasValue)
            {
                if (To.HasValue)
                {
                    _keyValues = new CameraLookDirection[2];
                    _keyValues[0] = From.Value;
                    _keyValues[1] = To.Value;
                }
                else if (Tos != null && Tos.Count() != 0)
                {
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
                    _keyValues = new CameraLookDirection[2];
                    _keyValues[0] = From.Value;
                    _keyValues[1] = defaultDestinationValue;
                }
            }
            else if (To.HasValue)
            {
                _keyValues = new CameraLookDirection[2];
                _keyValues[0] = defaultOriginValue;
                _keyValues[1] = To.Value;
            }
            else if (Tos != null && Tos.Count() != 0)
            {
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
                _keyValues = new CameraLookDirection[2];
                _keyValues[0] = defaultOriginValue;
                _keyValues[1] = defaultDestinationValue;
            }
        }

        private CameraLookDirection GetCameraLookDirectionAfterRotate(CameraLookDirection originalLookDirection, Vector3D rotateAxis, double rotateAngle)
        {
            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            rotateTransform3D.Rotation = new AxisAngleRotation3D(rotateAxis, rotateAngle);
            Matrix3D matrix = rotateTransform3D.Value;

            Point3D newCameraLookPoint = matrix.Transform(new Point3D(originalLookDirection.LookDirection.X, originalLookDirection.LookDirection.Y, originalLookDirection.LookDirection.Z));
            Vector3D newCameraLookDirection = new Vector3D(newCameraLookPoint.X, newCameraLookPoint.Y, newCameraLookPoint.Z);

            Vector3D horizontalVector3D = Vector3D.CrossProduct(new Vector3D(0, -1, 0), newCameraLookDirection);
            Vector3D upDirection = Vector3D.CrossProduct(horizontalVector3D, newCameraLookDirection);

            CameraLookDirection cameraLookDirection = new CameraLookDirection(newCameraLookDirection, upDirection);
            return cameraLookDirection;
        }

        /// <summary>
        /// 根据总的动画进度获取在当前旋转片段内的进度
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="sectionIndex"></param>
        /// <returns></returns>
        private double GetProgressInSection(double progress, int sectionIndex)
        {
            double totalProgressSpanInSection = _progressBegins[sectionIndex + 1] - _progressBegins[sectionIndex];
            if (totalProgressSpanInSection == 0)
            {
                return 0;
            }

            return (progress - _progressBegins[sectionIndex]) / totalProgressSpanInSection;
        }

        /// <summary>
        /// 根据进度获取当前旋转所在片段的索引
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        private int GetRotateSectionIndex(ref double progress)
        {
            int sectionIndex = 0;

            for (int i = 0; i < _progressBegins.Length - 1; i++)
            {
                if (_progressBegins[i] <= progress && _progressBegins[i + 1] >= progress)
                {
                    sectionIndex = i;
                    break;
                }
            }

            return sectionIndex;
        }

        /// <summary>
        /// 获取当前动画进度
        /// </summary>
        /// <param name="animationClock"></param>
        /// <returns></returns>
        private double GetCurrentProgress(AnimationClock animationClock)
        {
            double progress = animationClock.CurrentProgress.Value;

            //获取经缓动函数变换后的进度
            IEasingFunction easingFunction = EasingFunction;
            if (easingFunction != null)
            {
                progress = easingFunction.Ease(progress);
            }

            //进度不大于1
            if (progress > 1)
            {
                progress = 1;
            }

            return progress;
        }

        #endregion

        #region Protected Methods

        protected override Freezable CreateInstanceCore()
        {
            return new CameraRotateAnimation();
        }

        protected override CameraLookDirection GetCurrentValueCore(CameraLookDirection defaultOriginValue, CameraLookDirection defaultDestinationValue, AnimationClock animationClock)
        {
            TryValidateAnimationFunction(defaultOriginValue, defaultDestinationValue);

            double progress = GetCurrentProgress(animationClock);
            int sectionIndex = GetRotateSectionIndex(ref progress);

            double progressInSection = GetProgressInSection(progress, sectionIndex);

            CameraLookDirection rotateFromDirectionInSection = _keyValues[sectionIndex];

            Vector3D rotateAxis = _rotateAxesOfSection[sectionIndex];
            double fullRotateAngle = _anglesOfSection[sectionIndex];
            double rotateAngle = fullRotateAngle * progressInSection;

            CameraLookDirection cameraLookDirection = GetCameraLookDirectionAfterRotate(rotateFromDirectionInSection, rotateAxis, rotateAngle);
            return cameraLookDirection;
        }

        #endregion

        #region Public Methods

        public new CameraRotateAnimation Clone()
        {
            return (CameraRotateAnimation)base.Clone();
        }

        #endregion
    }
}
