using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AloysAdjustments.Common.UI.Controls
{
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Spinner : UserControl
    {
        public static readonly DependencyProperty SpinTimeProperty =
            DependencyProperty.Register("SpinTime", typeof(double), typeof(Spinner), new UIPropertyMetadata(1.0));

        private readonly DispatcherTimer _timer;
        
        public double SpinTime
        {
            get => (double)GetValue(SpinTimeProperty);
            set => SetValue(SpinTimeProperty, value);
        }

        public Spinner()
        {
            InitializeComponent();

            IsVisibleChanged += OnVisibleChanged;

            _timer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher)
            {
                Interval = TimeSpan.FromSeconds(SpinTime / 9)
            };
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                _timer.Tick += OnTimerTick;
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                _timer.Tick -= OnTimerTick;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            circleRotate.Angle = (circleRotate.Angle + 36) % 360;
        }

        private void OnCanvasLoaded(object sender, RoutedEventArgs e)
        {
            var pi = Math.PI;
            var step = Math.PI * 2 / 10.0;

            void moveCircle(DependencyObject circle, int pos)
            {
                circle.SetValue(Canvas.LeftProperty, 50 + (Math.Sin(pi + (pos * step)) * 50));
                circle.SetValue(Canvas.TopProperty, 50 + (Math.Cos(pi + (pos * step)) * 50));
            }
            

            moveCircle(circle0, 0);
            moveCircle(circle1, 1);
            moveCircle(circle2, 2);
            moveCircle(circle3, 3);
            moveCircle(circle4, 4);
            moveCircle(circle5, 5);
            moveCircle(circle6, 6);
            moveCircle(circle7, 7);
            moveCircle(circle8, 8);
        }

        private void OnCanvasUnloaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
        }
    }
}
