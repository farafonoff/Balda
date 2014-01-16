using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Balda.NET.Logic;
using Balda.Logic;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using Balda.PlatformDependent;
using Google.AdMob.Ads.WindowsPhone7.WPF;

namespace Balda.NET
{
    /// <summary>
    /// Interaction logic for FieldGrid.xaml
    /// </summary>
    public partial class FieldGrid : UserControl
    {
        public FieldGrid()
        {
            InitializeComponent();
            _fld = new Field(null);
            initButtons();
            MainGrid.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(FieldGrid_ManipulationDelta);
            MainGrid.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(FieldGrid_ManipulationStarted);
            MainGrid.ManipulationCompleted+=new EventHandler<ManipulationCompletedEventArgs>(MainGrid_ManipulationCompleted);
            MainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMove);
            DataContext = _fld;
        }

        void MainGrid_MouseMove(object sender, MouseEventArgs e)
        {
            processSwipe(e.GetPosition((UIElement)sender).X, e.GetPosition((UIElement)sender).Y);
        }

        void FieldGrid_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
        }

        void FieldGrid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            finishDelay();
            e.Handled = true;
        }

        private void MainGrid_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_fld != null)
            {
                _fld.skip();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (RestartGame != null&&IsEnabled)
            {
                RestartGame();
            }
        }
        private void TextBlock_Tap(object sender, GestureEventArgs e)
        {
            finishDelay();
        }

    }
}
