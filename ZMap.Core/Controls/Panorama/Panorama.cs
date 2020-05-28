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

        private Grid _root = null;
        private ModelVisual3D _content = null;

        private PanoramaResourceConfig _resourceConfig = null;
        private int _layerCount = 0;
        private double _angleRangePerLayer = 0;

        private Point _mousePosition;
        private bool _isMouseLeftButtonDown = false;

        #endregion

        #region Properties

        public Viewport3D Viewport3D { get; private set; }
        public PerspectiveCamera Camera { get; private set; }
        public int CurrentLayerLevel { get; private set; }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty MaxFieldOfViewProperty =
            DependencyProperty.Register("MaxFieldOfView", typeof(double), typeof(Panorama), new PropertyMetadata(150.0, OnMaxFieldOfViewChanged));

        public static readonly DependencyProperty MinFieldOfViewProperty =
            DependencyProperty.Register("MinFieldOfView", typeof(double), typeof(Panorama), new PropertyMetadata(5.0, OnMinFieldOfViewChanged));

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(Panorama), new PropertyMetadata(1.0, OnRadiusChanged));

        public static readonly DependencyProperty StackCountProperty =
            DependencyProperty.Register("StackCount", typeof(int), typeof(Panorama), new PropertyMetadata(64, OnStackCountChanged));

        public static readonly DependencyProperty SliceCountProperty =
            DependencyProperty.Register("SliceCount", typeof(int), typeof(Panorama), new PropertyMetadata(64, OnSliceCountChanged));

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

        #region Constructors

        static Panorama()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Panorama), new FrameworkPropertyMetadata(typeof(Panorama)));
        }

        #endregion

        #region Private Methods

        #region Dependency Property Changed Callbacks

        private static void OnMaxFieldOfViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;

            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;

            if (oldValue != newValue)
            {
                panorama.UpdateAngleRangePerLayer();

                int layerLevel = panorama.GetLayerLevelByFieldOfView();
                if (panorama.CanChangeLayer(layerLevel))
                {
                    panorama.UpdateLayer(layerLevel);
                }
            }
        }

        private static void OnMinFieldOfViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;

            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;

            if (oldValue != newValue)
            {
                panorama.UpdateAngleRangePerLayer();

                int layerLevel = panorama.GetLayerLevelByFieldOfView();
                if (panorama.CanChangeLayer(layerLevel))
                {
                    panorama.UpdateLayer(layerLevel);
                }
            }
        }

        private static void OnStackCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;

            int oldValue = (int)e.OldValue;
            int newValue = (int)e.NewValue;

            if (oldValue != newValue)
            {
                int layerLevel = panorama.GetLayerLevelByFieldOfView();
                panorama.UpdateLayer(layerLevel);
            }
        }

        private static void OnSliceCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Panorama panorama = d as Panorama;

            int oldValue = (int)e.OldValue;
            int newValue = (int)e.NewValue;

            if (oldValue != newValue)
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

            if (oldValue != newValue)
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
                panorama.ResetPanoramaResource(panorama);
                return;
            }

            string configPath = string.Format(@"{0}\{1}", newResourcePath, "config.json");
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
                    Camera?.VerticalRotateInSitu(verticalMouseMove);
                }
                else
                {
                    Camera?.HorizontalRotateInSitu(horizontalMouseMove);
                }

                //Pan = Vector3D.AngleBetween(new Vector3D(_camera.LookDirection.X, 0, _camera.LookDirection.Z), new Vector3D(1, 0, 0)) * (_camera.LookDirection.Z >= 0 ? 1 : -1);
                //Tilt = 90 - Vector3D.AngleBetween(_camera.LookDirection, new Vector3D(0, 1, 0));
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
            Camera?.ZoomIn(-zoomFactor, MinFieldOfView, MaxFieldOfView);

            int layerLevel = GetLayerLevelByFieldOfView();
            if (CanChangeLayer(layerLevel))
            {
                UpdateLayer(layerLevel);
            }
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
            if (_content == null)
            {
                return;
            }

            CurrentLayerLevel = layerLevel;

            PanoramaLayer layer = _resourceConfig.Layers[layerLevel - 1];

            Model3DGroup model3DGroup = new Model3DGroup();
            _content.Content = model3DGroup;

            List<GeometryModel3D> geometries = CreateAllMapTileGeometryModel3Ds(layer.RowCount, layer.ColumnCount);

            string imageResourcePath = string.Format(@"{0}\{1}", Resource, layer.ImageResourcePath);
            for (int i = 0; i < geometries.Count; i++)
            {
                GeometryModel3D geometry = geometries[i];

                //图片资源的起始索引是1
                string sourceUrl = string.Format(@"{0}\{1}.png", imageResourcePath, i + 1);
                geometry.Material = new DiffuseMaterial(new ImageBrush(new BitmapImage(new Uri(sourceUrl))));
                model3DGroup.Children.Add(geometry);
            }
        }

        /// <summary>
        /// 重置全景资源
        /// </summary>
        /// <param name="panorama"></param>
        private void ResetPanoramaResource(Panorama panorama)
        {
            panorama._resourceConfig = null;
            panorama._layerCount = 0;
            panorama._angleRangePerLayer = 0;
            panorama.CurrentLayerLevel = 0;
            if (panorama._content != null)
            {
                panorama._content.Content = null;
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
