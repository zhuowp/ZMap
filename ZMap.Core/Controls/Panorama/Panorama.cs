using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
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
    [TemplatePart(Name = "PART_Root", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_Viewport3D", Type = typeof(Viewport3D))]
    [TemplatePart(Name = "PART_Camera", Type = typeof(Camera))]
    [TemplatePart(Name = "PART_Content", Type = typeof(ModelVisual3D))]
    public class Panorama : Control
    {
        #region Fields

        private bool _hasInitializedComponet = false;
        private Grid _root = null;
        private ModelVisual3D _content = null;

        private PanoramaResourceConfig _resourceConfig = null;
        private string _fullResourceDirectoryPath = string.Empty;
        private int _layerCount = 0;
        private double _angleRangePerLayer = 0;

        private Point _mousePosition;
        private bool _isMouseLeftButtonDown = false;
        private bool _isCameraRotated = false;

        #endregion

        #region Properties

        public Viewport3D Viewport3D { get; private set; }
        public PerspectiveCamera Camera { get; private set; }
        public int CurrentLayerLevel { get; private set; }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty MaxFieldOfViewProperty =
            DependencyProperty.Register("MaxFieldOfView", typeof(double), typeof(Panorama), new PropertyMetadata(120.0, OnRangeOfFieldOfViewChanged));

        public static readonly DependencyProperty MinFieldOfViewProperty =
            DependencyProperty.Register("MinFieldOfView", typeof(double), typeof(Panorama), new PropertyMetadata(5.0, OnRangeOfFieldOfViewChanged));

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Panorama), new PropertyMetadata(1.0, OnRadiusChanged));

        public static readonly DependencyProperty StackCountProperty =
            DependencyProperty.Register("StackCount", typeof(int), typeof(Panorama), new PropertyMetadata(64, OnSpereModelTriangulationChanged));

        public static readonly DependencyProperty SliceCountProperty =
            DependencyProperty.Register("SliceCount", typeof(int), typeof(Panorama), new PropertyMetadata(64, OnSpereModelTriangulationChanged));

        public static readonly DependencyProperty ResourceProperty =
            DependencyProperty.Register("Resource", typeof(string), typeof(Panorama), new PropertyMetadata(string.Empty, OnResourceChanged));

        #endregion

        #region Dependency Property Wrappers

        /// <summary>
        /// 最大视场角
        /// </summary>
        public double MaxFieldOfView
        {
            get { return (double)GetValue(MaxFieldOfViewProperty); }
            set { SetValue(MaxFieldOfViewProperty, value); }
        }

        /// <summary>
        /// 最小视场角
        /// </summary>
        public double MinFieldOfView
        {
            get { return (double)GetValue(MinFieldOfViewProperty); }
            set { SetValue(MinFieldOfViewProperty, value); }
        }

        /// <summary>
        /// 球体模型半径
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        /// <summary>
        /// 球体模型横向切片数量
        /// </summary>
        public int StackCount
        {
            get { return (int)GetValue(StackCountProperty); }
            set { SetValue(StackCountProperty, value); }
        }

        /// <summary>
        /// 球体模型纵向切片数量
        /// </summary>
        public int SliceCount
        {
            get { return (int)GetValue(SliceCountProperty); }
            set { SetValue(SliceCountProperty, value); }
        }

        /// <summary>
        /// 全景地图资源地址
        /// </summary>
        public string Resource
        {
            get { return (string)GetValue(ResourceProperty); }
            set { SetValue(ResourceProperty, value); }
        }

        #endregion

        #region Routed Events

        public static readonly RoutedEvent CameraStatusChangedEvent =
            EventManager.RegisterRoutedEvent("CameraStatusChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Panorama));

        public static readonly RoutedEvent CameraStatusChangingEvent =
            EventManager.RegisterRoutedEvent("CameraStatusChanging", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Panorama));

        #endregion

        #region Routed Event Wrappers

        public event RoutedEventHandler CameraStatusChanged
        {
            add { AddHandler(CameraStatusChangedEvent, value); }
            remove { RemoveHandler(CameraStatusChangedEvent, value); }
        }

        public event RoutedEventHandler CameraStatusChanging
        {
            add { AddHandler(CameraStatusChangingEvent, value); }
            remove { RemoveHandler(CameraStatusChangingEvent, value); }
        }

        #endregion

        #region Constructors

        static Panorama()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Panorama), new FrameworkPropertyMetadata(typeof(Panorama)));
        }

        #endregion

        #region Private Methods

        #region Dependency Property Changed Callbacks

        private static void OnRangeOfFieldOfViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;

            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;

            if (oldValue != newValue)
            {
                panorama.UpdateAngleRangePerLayer();

                if (panorama._hasInitializedComponet)
                {
                    int layerLevel = panorama.GetLayerLevelByFieldOfView();
                    if (panorama.CanChangeLayer(layerLevel))
                    {
                        panorama.UpdateLayer(layerLevel);
                    }
                }
            }
        }

        private static void OnSpereModelTriangulationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;

            int oldValue = (int)e.OldValue;
            int newValue = (int)e.NewValue;

            if (oldValue != newValue && panorama._hasInitializedComponet)
            {
                int layerLevel = panorama.GetLayerLevelByFieldOfView();
                panorama.UpdateLayer(layerLevel);
            }
        }

        private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;

            int oldValue = (int)e.OldValue;
            int newValue = (int)e.NewValue;

            if (oldValue != newValue && panorama._hasInitializedComponet)
            {
                int layerLevel = panorama.GetLayerLevelByFieldOfView();
                panorama.UpdateLayer(layerLevel);
            }
        }

        private static void OnResourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;
            string newResourcePath = e.NewValue.ToString();
            if (string.IsNullOrEmpty(newResourcePath))
            {
                panorama.ResetPanoramaResource();
                return;
            }

            panorama._fullResourceDirectoryPath = panorama.GetRelativeOrAbsoluteFullPathOfDirectory(newResourcePath);

            string configPath = string.Format(@"{0}\{1}", panorama._fullResourceDirectoryPath, "config.json");
            if (!File.Exists(configPath))
            {
                throw new Exception("Panorama configuration file does not exist.");
            }

            string configString = File.ReadAllText(configPath);
            PanoramaResourceConfig resourceConfig = JsonSerializer.Deserialize<PanoramaResourceConfig>(configString);
            if (resourceConfig == null || resourceConfig.Layers == null || resourceConfig.Layers.Count == 0)
            {
                throw new Exception("There is something wrong with the panorama resource config.");
            }

            panorama._resourceConfig = resourceConfig;
            panorama._layerCount = panorama._resourceConfig.Layers.Count;
            panorama.UpdateAngleRangePerLayer();
        }

        #endregion

        #region Mouse Event Handlers

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
                Visual3DHitTestHelper visual3DHitTestHelper = new Visual3DHitTestHelper();
                visual3DHitTestHelper.HitTest(Viewport3D, _mousePosition, out Visual3D hitVisual3D, out Point3D HitPoint3D);

                Point mousePotion = e.GetPosition(null);
                visual3DHitTestHelper.HitTest(Viewport3D, mousePotion, out Visual3D hitVisual3D1, out Point3D HitPoint3D1);

                Vector3D vector1 = new Vector3D(HitPoint3D.X, HitPoint3D.Y, HitPoint3D.Z);
                Vector3D vector2 = new Vector3D(HitPoint3D1.X, HitPoint3D1.Y, HitPoint3D1.Z);

                Vector3D rotateAxis = Vector3D.CrossProduct(vector1, vector2);
                double horizontalRotateAngle = -Vector3D.AngleBetween(vector1, vector2);

                Camera?.RotateAroundAxis(rotateAxis, horizontalRotateAngle);
                _mousePosition = e.GetPosition(null);

                _isCameraRotated = true;

                RoutedEventArgs args = new RoutedEventArgs(CameraStatusChangingEvent, this);
                RaiseEvent(args);
            }
        }

        private void Sphere_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseLeftButtonDown = false;
            if (_root != null)
            {
                _root.ReleaseMouseCapture();
            }

            if (_isCameraRotated)
            {
                RoutedEventArgs args = new RoutedEventArgs(CameraStatusChangedEvent, this);
                RaiseEvent(args);

                _isCameraRotated = false;
            }
        }

        private void Sphere_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoomFactor = e.Delta > 0 ? 1 : -1;
            Camera?.ZoomIn(-zoomFactor, MinFieldOfView, MaxFieldOfView);

            int layerLevel = GetLayerLevelByFieldOfView();
            if (CanChangeLayer(layerLevel))
            {
                UpdateLayer(layerLevel);
            }

            RoutedEventArgs args = new RoutedEventArgs(CameraStatusChangingEvent, this);
            RaiseEvent(args);
        }

        #endregion

        private List<GeometryModel3D> CreateAllMapTileGeometryModel3Ds(int mapTileRowCount, int mapTileColumnCount)
        {
            List<GeometryModel3D> geometryModel3DList = new List<GeometryModel3D>();
            for (int tileRow = 0; tileRow < mapTileRowCount; tileRow++)
            {
                for (int tileColumn = 0; tileColumn < mapTileColumnCount; tileColumn++)
                {
                    GeometryModel3D geometryModel3D = CreateMapTileGeometryModel3D(tileRow, tileColumn, mapTileRowCount, mapTileColumnCount);
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
        private GeometryModel3D CreateMapTileGeometryModel3D(int tileRow, int tileColumn, int rowCount, int columnCount)
        {
            GeometryModel3D geometryModel3D = new GeometryModel3D();
            geometryModel3D.Material = new DiffuseMaterial(Brushes.Blue);
            geometryModel3D.BackMaterial = new DiffuseMaterial(Brushes.Red);

            MeshGeometry3D mesh = new MeshGeometry3D();
            geometryModel3D.Geometry = mesh;

            int _stackCountPerTile = StackCount / rowCount;
            int _sliceCountPerTile = SliceCount / columnCount;

            int beginStack = tileRow * _stackCountPerTile;
            int endStack = (tileRow + 1) * _stackCountPerTile;

            int beginSlice = tileColumn * _sliceCountPerTile;
            int endSlice = (tileColumn + 1) * _sliceCountPerTile;

            for (int stack = beginStack; stack <= endStack; stack++)
            {
                double phi = Maths.HALF_PI - Maths.PI * stack / StackCount;
                double y = Radius * Math.Sin(phi);

                double scale = Radius * Math.Cos(phi);
                for (int slice = beginSlice; slice <= endSlice; slice++)
                {
                    double theta = Maths.DOUBLE_PI * slice / SliceCount;
                    double x = scale * Math.Sin(theta);
                    double z = scale * Math.Cos(theta);

                    Point3D position = new Point3D(-x, y, z);
                    mesh.Positions.Add(position);

                    Vector3D normal = new Vector3D(x, y, z);
                    mesh.Normals.Add(normal);

                    mesh.TextureCoordinates.Add(new Point(slice / (double)SliceCount, stack / (double)StackCount));
                }
            }

            for (int stack = 0; stack < _stackCountPerTile; stack++)
            {
                int topPointStartIndex = stack * (_sliceCountPerTile + 1);
                int bottomPointStartIndex = (stack + 1) * (_sliceCountPerTile + 1);

                for (int slice = 0; slice < _sliceCountPerTile; slice++)
                {
                    mesh.TriangleIndices.Add(topPointStartIndex + slice);
                    mesh.TriangleIndices.Add(bottomPointStartIndex + slice);
                    mesh.TriangleIndices.Add(topPointStartIndex + slice + 1);

                    if (stack < _sliceCountPerTile)
                    {
                        mesh.TriangleIndices.Add(topPointStartIndex + slice + 1);
                        mesh.TriangleIndices.Add(bottomPointStartIndex + slice);
                        mesh.TriangleIndices.Add(bottomPointStartIndex + slice + 1);
                    }
                }
            }
            mesh.Freeze();
            return geometryModel3D;
        }

        /// <summary>
        /// 根据当前摄像机的市场角获取全景图层层级
        /// </summary>
        /// <returns></returns>
        private int GetLayerLevelByFieldOfView()
        {
            if (Camera.FieldOfView >= MaxFieldOfView)
            {
                return 1;
            }

            int layerLevel = _layerCount - (int)((Camera.FieldOfView - MinFieldOfView) / _angleRangePerLayer);
            return layerLevel;
        }

        /// <summary>
        /// 更新每一图层的视场角范围大小
        /// </summary>
        private void UpdateAngleRangePerLayer()
        {
            _angleRangePerLayer = (MaxFieldOfView - MinFieldOfView) / _layerCount;
        }

        /// <summary>
        /// 判断是否需要切换全景图层
        /// </summary>
        /// <param name="layerLevel"></param>
        /// <returns></returns>
        private bool CanChangeLayer(int layerLevel)
        {
            return layerLevel > 0
                && layerLevel <= _layerCount
                && layerLevel != CurrentLayerLevel;
        }

        /// <summary>
        /// 切换全景图层
        /// </summary>
        /// <param name="layerLevel"></param>
        private void UpdateLayer(int layerLevel)
        {
            if (_content == null || string.IsNullOrEmpty(_fullResourceDirectoryPath))
            {
                return;
            }

            CurrentLayerLevel = layerLevel;

            PanoramaLayer layer = _resourceConfig.Layers[layerLevel - 1];

            Model3DGroup model3DGroup = new Model3DGroup();
            _content.Content = model3DGroup;

            List<GeometryModel3D> geometries = CreateAllMapTileGeometryModel3Ds(layer.RowCount, layer.ColumnCount);

            string imageResourcePath = string.Format(@"{0}\{1}", _fullResourceDirectoryPath, layer.ImageResourcePath);
            for (int i = 0; i < geometries.Count; i++)
            {
                GeometryModel3D geometry = geometries[i];

                //图片资源的起始索引是1
                string sourceUrl = string.Format(@"{0}\{1}.png", imageResourcePath, i + 1);
                geometry.Material = new DiffuseMaterial(new ImageBrush(new BitmapImage(new Uri(sourceUrl))));
                model3DGroup.Children.Add(geometry);
            }
        }

        private string GetRelativeOrAbsoluteFullPathOfDirectory(string path)
        {
            string relativeFullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            if (Directory.Exists(relativeFullPath))
            {
                return relativeFullPath;
            }

            if (Directory.Exists(path))
            {
                return path;
            }
            else
            {
                throw new Exception("The directory of resource does not exist.");
            }
        }

        /// <summary>
        /// 重置全景资源
        /// </summary>
        /// <param name="panorama"></param>
        private void ResetPanoramaResource()
        {
            _fullResourceDirectoryPath = string.Empty;
            _resourceConfig = null;
            _layerCount = 0;
            _angleRangePerLayer = 0;
            CurrentLayerLevel = 0;
            if (_content != null)
            {
                _content.Content = null;
            }
        }

        #endregion

        #region Public Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _root = GetTemplateChild("PART_Root") as Grid;
            Viewport3D = GetTemplateChild("PART_Viewport3D") as Viewport3D;
            Camera = GetTemplateChild("PART_Camera") as PerspectiveCamera;
            _content = GetTemplateChild("PART_Content") as ModelVisual3D;

            _hasInitializedComponet = true;

            MouseWheel += Sphere_MouseWheel;
            MouseLeftButtonDown += Sphere_MouseLeftButtonDown;
            MouseMove += Sphere_MouseMove;
            MouseLeftButtonUp += Sphere_MouseLeftButtonUp;

            //初始化全景图层
            int layerLevel = GetLayerLevelByFieldOfView();
            UpdateLayer(layerLevel);
        }

        #endregion

    }
}
