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
        Thread _thread = null;

        public MainWindow()
        {
            InitializeComponent();

            _thread = new Thread(UpdateTagLocation);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void UpdateTagLocation()
        {
            while (true)
            {
                Dispatcher.Invoke(() =>
                {
                    if (panorama.Viewport3D != null)
                    {
                        Point point = D3Helper.Point3DToScreen2D(new Point3D(1, 0, 0), panorama.Viewport3D);

                        Canvas.SetLeft(tag, point.X);
                        Canvas.SetTop(tag, point.Y);
                    }
                });
                Thread.Sleep(1000);
            }
        }
    }
}
