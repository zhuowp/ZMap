using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZMap.Core
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ZMap.Core.Panorama"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ZMap.Core.Panorama;assembly=ZMap.Core.Panorama"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:Panorama/>
    ///
    /// </summary>
    public class Panorama : Control
    {
        #region Fields

        private Grid _root = null;
        private PerspectiveCamera _camera = null;
        private ModelVisual3D _content = null;

        private Point _mousePosition;
        private bool _isMouseLeftButtonDown = false;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty MaxFieldOfViewProperty =
            DependencyProperty.Register("MaxFieldOfView", typeof(double), typeof(Panorama), new PropertyMetadata(150.0));

        public static readonly DependencyProperty MinFieldOfViewProperty =
            DependencyProperty.Register("MinFieldOfView", typeof(double), typeof(Panorama), new PropertyMetadata(15.0));

        public static readonly DependencyProperty PanProperty =
            DependencyProperty.Register("Pan", typeof(double), typeof(Panorama), new PropertyMetadata(0.0, OnPanChanged));

        public static readonly DependencyProperty TiltProperty =
            DependencyProperty.Register("Tilt", typeof(double), typeof(Panorama), new PropertyMetadata(0.0, OnTiltChanged));

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(Panorama), new PropertyMetadata(1.0, OnZoomChanged));

        public static readonly DependencyProperty ResourceProperty =
            DependencyProperty.Register("Resource", typeof(string), typeof(Panorama), new PropertyMetadata(string.Empty));

        #endregion

        #region Dependency Property Wrappers

        public double MaxFieldOfView
        {
            get { return (double)GetValue(MaxFieldOfViewProperty); }
            set { SetValue(MaxFieldOfViewProperty, value); }
        }

        public double MinFieldOfView
        {
            get { return (double)GetValue(MinFieldOfViewProperty); }
            set { SetValue(MinFieldOfViewProperty, value); }
        }

        public double Pan
        {
            get { return (double)GetValue(PanProperty); }
            set { SetValue(PanProperty, value); }
        }

        public double Tilt
        {
            get { return (double)GetValue(TiltProperty); }
            set { SetValue(TiltProperty, value); }
        }

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public string Resource
        {
            get { return (string)GetValue(ResourceProperty); }
            set { SetValue(ResourceProperty, value); }
        }

        #endregion

        #region Constructors

        static Panorama()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Panorama), new FrameworkPropertyMetadata(typeof(Panorama)));
        }

        #endregion

        #region Private Methods

        private void InitMapTiles()
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            _content.Content = model3DGroup;

            List<GeometryModel3D> geometries = CreateAllMapTileGeometryModel3Ds();
            int i = 1;
            foreach (GeometryModel3D geometry in geometries)
            {
                string sourceUrl = string.Format(@"C:\Users\dell\Downloads\2020427115041502\2020427115041502PanoramaDemo1_{0}.png", i);
                geometry.BackMaterial = new DiffuseMaterial(new ImageBrush(new BitmapImage(new Uri(sourceUrl))));
                geometry.Material = new DiffuseMaterial(new ImageBrush(new BitmapImage(new Uri(sourceUrl))));
                model3DGroup.Children.Add(geometry);
                i++;
            }
        }

        private double Radius = 1;
        private Point3D Center = new Point3D(0, 0, 0);

        private int _mapTileRowCount = 8;
        private int _mapTileColumnCount = 8;

        private int _stackCountPerTile = 8;
        private int _sliceCountPerTile = 8;

        private List<GeometryModel3D> CreateAllMapTileGeometryModel3Ds()
        {
            List<GeometryModel3D> geometryModel3DList = new List<GeometryModel3D>();
            for (int tileRow = 0; tileRow < _mapTileRowCount; tileRow++)
            {
                for (int tileColumn = 0; tileColumn < _mapTileColumnCount; tileColumn++)
                {
                    GeometryModel3D geometryModel3D = CreateMapTileGeometryModel3D(tileRow, tileColumn);
                    geometryModel3DList.Add(geometryModel3D);
                }
            }

            return geometryModel3DList;
        }

        /// <summary>
        /// 初始化地图瓦片3D模型
        /// </summary>
        /// <param name="tileRow"></param>
        /// <param name="tileColumn"></param>
        /// <returns></returns>
        private GeometryModel3D CreateMapTileGeometryModel3D(int tileRow, int tileColumn)
        {
            GeometryModel3D geometryModel3D = new GeometryModel3D();
            geometryModel3D.Material = new DiffuseMaterial(Brushes.Blue);
            geometryModel3D.BackMaterial = new DiffuseMaterial(Brushes.Red);

            MeshGeometry3D mesh = new MeshGeometry3D();
            geometryModel3D.Geometry = mesh;

            int stackCount = _stackCountPerTile * _mapTileRowCount;
            int sliceCount = _sliceCountPerTile * _mapTileColumnCount;

            int beginStack = tileRow * _stackCountPerTile;
            int endStack = (tileRow + 1) * _stackCountPerTile;

            int beginSlice = tileColumn * _sliceCountPerTile;
            int endSlice = (tileColumn + 1) * _sliceCountPerTile;

            for (int stack = beginStack; stack <= endStack; stack++)
            {
                double phi = Maths.HALF_PI - Maths.PI * stack / stackCount;
                double y = Radius * Math.Sin(phi);

                double scale = Radius * Math.Cos(phi);
                for (int slice = beginSlice; slice <= endSlice; slice++)
                {
                    double theta = Maths.DOUBLE_PI * slice / sliceCount;
                    double x = scale * Math.Sin(theta);
                    double z = scale * Math.Cos(theta);

                    Point3D position = new Point3D(Center.X - x, Center.Y + y, Center.Z + z);
                    mesh.Positions.Add(position);

                    Vector3D normal = new Vector3D(x, y, z);
                    mesh.Normals.Add(normal);

                    mesh.TextureCoordinates.Add(new Point(slice / (double)sliceCount, stack / (double)stackCount));
                }
            }

            for (int stack = 0; stack < _stackCountPerTile; stack++)
            {
                int topPointStartIndex = stack * (_sliceCountPerTile + 1);
                int bottomPointStartIndex = (stack + 1) * (_sliceCountPerTile + 1);

                for (int slice = 0; slice < _sliceCountPerTile; slice++)
                {
                    mesh.TriangleIndices.Add(topPointStartIndex + slice);
                    mesh.TriangleIndices.Add(topPointStartIndex + slice + 1);
                    mesh.TriangleIndices.Add(bottomPointStartIndex + slice);

                    if (stack < _sliceCountPerTile)
                    {
                        mesh.TriangleIndices.Add(topPointStartIndex + slice + 1);
                        mesh.TriangleIndices.Add(bottomPointStartIndex + slice + 1);
                        mesh.TriangleIndices.Add(bottomPointStartIndex + slice);
                    }
                }
            }
            mesh.Freeze();
            return geometryModel3D;
        }

        private static void OnPanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;
            if (panorama == null)
            {
                return;
            }
        }

        private static void OnTiltChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;
            if (panorama == null)
            {
                return;
            }
        }

        private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;
            if (panorama == null)
            {
                return;
            }
        }

        private void Sphere_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mousePosition = e.GetPosition(null);
            _isMouseLeftButtonDown = true;

            if (_root != null)
            {
                Mouse.Capture(_root);
            }
        }

        private void Sphere_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseLeftButtonDown)
            {
                Point mousePotion = e.GetPosition(null);

                double verticalMouseMove = mousePotion.Y - _mousePosition.Y;
                double horizontalMouseMove = mousePotion.X - _mousePosition.X;
                if (Math.Abs(verticalMouseMove) > Math.Abs(horizontalMouseMove))
                {
                    _camera?.VerticalRotateInSitu(verticalMouseMove);
                }
                else
                {
                    _camera?.HorizontalRotateInSitu(horizontalMouseMove);
                }

                Pan = Vector3D.AngleBetween(new Vector3D(_camera.LookDirection.X, 0, _camera.LookDirection.Z), new Vector3D(1, 0, 0)) * (_camera.LookDirection.Z >= 0 ? 1 : -1);
                Tilt = 90 - Vector3D.AngleBetween(_camera.LookDirection, new Vector3D(0, 1, 0));
                _mousePosition = e.GetPosition(null);
            }
        }

        private void Sphere_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseLeftButtonDown = false;
            if (_root != null)
            {
                _root.ReleaseMouseCapture();
            }
        }

        private void Sphere_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1 : -1;
            _camera?.ZoomIn(-zoomFactor, MinFieldOfView, MaxFieldOfView);
        }

        #endregion

        #region Public Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _root = GetTemplateChild("PART_Root") as Grid;
            _camera = GetTemplateChild("PART_Camera") as PerspectiveCamera;
            _content = GetTemplateChild("PART_Content") as ModelVisual3D;

            MouseWheel += Sphere_MouseWheel;

            MouseLeftButtonDown += Sphere_MouseLeftButtonDown;
            MouseMove += Sphere_MouseMove;
            MouseLeftButtonUp += Sphere_MouseLeftButtonUp;

            InitMapTiles();
        }

        #endregion

    }
}
