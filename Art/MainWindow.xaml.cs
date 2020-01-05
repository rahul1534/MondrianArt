using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Environment;

namespace Art
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random rnd = new Random(Guid.NewGuid().ToString().GetHashCode());
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            MouseLeftButtonDown += MainWindow_MouseLeftButtonDown;
            MouseRightButtonDown += MainWindow_MouseRightButtonDown;
            MouseDoubleClick += MainWindow_MouseDoubleClick;
        }

        private void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Exit(0);
        }

        private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveImage();
        }

        private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            main.Children.Clear();
            Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private async void Start()
        {
            await DrawMondarian(Left, Top, ActualWidth, ActualHeight);
        }

        private async Task DrawMondarian(double x, double y, double w, double h)
        {
            if (h < 80 || w < 80)
                return;
            await FillRectangle(x, y, w, h, RandomColor());            
            switch ((h > 800 || w > 800) ? rnd.Next(1, 3) : rnd.Next(0, 3))
            {
                case 0:
                    await AddCircle(x, y, w, h, RandomColor());
                    break;
                case 1:
                    double midX = RandomReal(0, w);
                    await DrawBlackLine(x + midX, y, h, Orientation.Vertical);
                    await DrawMondarian(x, y, midX, h);
                    await DrawMondarian(x + midX, y, w - midX, h);
                    break;
                case 2:
                    double midY = RandomReal(0, h);
                    await DrawBlackLine(x, y + midY, w, Orientation.Horizontal);
                    await DrawMondarian(x, y, w, midY);
                    await DrawMondarian(x, y + midY, w, h - midY);
                    break;
            }
        }

        private async Task DrawBlackLine(double x, double y, double length, Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
                await AddChildren(new Line() { X1 = x, Y1 = y, X2 = x + length, Y2 = y, StrokeThickness = 1 });
            else
                await AddChildren(new Line() { X1 = x, Y1 = y, X2 = x, Y2 = y + length, StrokeThickness = 1 });
        }

        private double RandomReal(double low, double high)
        {
            return low + rnd.NextDouble() * (high - low);
        }

        private async Task FillRectangle(double left, double top, double width, double height, SolidColorBrush color)
        {
            await AddChildren(new Rectangle() { Height = height, Width = width, Fill = color, Margin = new Thickness() { Left = left, Top = top } });            
        }

        private async Task AddCircle(double left, double top, double width, double height, SolidColorBrush color)
        {
            if (height > width)
                top += (height - width) / 2;
            else
                left += (width - height) / 2;
            await AddChildren(new Ellipse()
            {
                Height = Math.Min(height, width),
                Width = Math.Min(height, width),
                Fill = color,
                Margin = new Thickness() { Left = left, Top = top }
            });
        }

        private async Task AddChildren(UIElement element)
        {
            await Task.Delay(100);
            Dispatcher.Invoke(() => { main.Children.Add(element); });
        }

        private SolidColorBrush RandomColor()
        {
            Color result = Colors.Transparent;
            Type colorsType = typeof(Colors);
            PropertyInfo[] properties = colorsType.GetProperties();
            int random = rnd.Next(properties.Length);
            result = (Color)properties[random].GetValue(null, null);
            return new SolidColorBrush(result);
        }

        private void SaveImage()
        {
            Rect rect = new Rect(main.RenderSize);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, 96d, 96d, PixelFormats.Default);
            rtb.Render(main);
            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            pngEncoder.Save(ms);
            ms.Close();
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(GetFolderPath(SpecialFolder.MyPictures), $"{Guid.NewGuid().ToString()}.png"), ms.ToArray());
        }
    }
}
