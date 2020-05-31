using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZMap.Core;

namespace ZMap.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private Thread _thread = null;
        private bool _panoramaStatusChanged = true;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            _thread = new Thread(ContinuouslyUpdateTagLocation);
            _thread.IsBackground = true;
            _thread.Start();

            CameraRotateAnimation rotateAnimation = new CameraRotateAnimation(
                new CameraLookDirection(new Vector3D(1, 0, 0), new Vector3D()),
                new CameraLookDirection(new Vector3D(1, -1, 1), new Vector3D()),
                new Duration(TimeSpan.FromSeconds(20)), FillBehavior.Stop);
            //rotateAnimation.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut };
            panorama.BeginAnimation(Panorama.LookDirectionProperty, rotateAnimation);
        }

        #endregion

        #region Private Methods

        private void ContinuouslyUpdateTagLocation()
        {
            while (true)
            {
                TryUpadateTagLocations();
                Thread.Sleep(10);
            }
        }

        private bool TryUpadateTagLocations()
        {
            try
            {
                if (panorama.Viewport3D == null || panorama.Camera == null)
                {
                    return false;
                }

                if (!_panoramaStatusChanged)
                {
                    return false;
                }
                _panoramaStatusChanged = false;

                Vector3D cameraLookDirection;
                Matrix3D matrix3D;
                Dispatcher.Invoke(() =>
                {
                    cameraLookDirection = panorama.Camera.LookDirection;
                    Viewport3DVisual vpv = VisualTreeHelper.GetParent(panorama.Viewport3D.Children[0]) as Viewport3DVisual;
                    matrix3D = D3Helper.TryWorldToViewportTransform(vpv, out bool isOk);
                });

                Point3D point;
                Point3D point3D = new Point3D(1, 0, 0);
                if (Vector3D.DotProduct(cameraLookDirection, new Vector3D(point3D.X, point3D.Y, point3D.Z)) <= 0)
                {
                    point = new Point3D(-10000, -10000, -10000);
                }
                else
                {
                    //可能的耗时操作
                    point = matrix3D.Transform(new Point3D(1, 0, 0));
                }

                Dispatcher.Invoke(() =>
                {
                    Canvas.SetLeft(tag, point.X);
                    Canvas.SetTop(tag, point.Y);
                });

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Panorama_CameraStatusChanged(object sender, RoutedEventArgs e)
        {
            _panoramaStatusChanged = true;
        }

        private void Panorama_CameraStatusChanging(object sender, RoutedEventArgs e)
        {
            //_panoramaStatusChanged = true;
        }

        #endregion
    }
}
