using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Balda.NET.Logic;
using Balda.Logic;
using System.Windows.Input;
using Balda.PlatformDependent;
#if SILVERLIGHT
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;
using Windows.Devices.Input;
using Windows.UI.Xaml.Input;
using Balda02.PlatformDependent;
using Windows.UI.Popups;
#endif

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
            _fld = new Logic.Field(new string(' ', Field.FieldSize));
            DataContext = _fld;
            initButtons();
            /*MainGrid.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(FieldGrid_ManipulationDelta);
            MainGrid.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(FieldGrid_ManipulationStarted);
            MainGrid.ManipulationCompleted+=new EventHandler<ManipulationCompletedEventArgs>(MainGrid_ManipulationCompleted);
            MainGrid.MouseMove += new MouseEventHandler(MainGrid_MouseMove);*/
            MainGrid.PointerMoved += MainGrid_PointerMoved;
            MainGrid.PointerPressed += MainGrid_PointerPressed;
            MainGrid.PointerReleased += MainGrid_PointerReleased;
            MainGrid.AddHandler(PointerPressedEvent, new PointerEventHandler(MainGrid_PointerPressed), true);
            MainGrid.AddHandler(PointerReleasedEvent, new PointerEventHandler(MainGrid_PointerReleased), true);
            removeGoldenHighlight.Tick += removeGoldenHighlight_Tick;
            removeGoldenHighlight.Interval = TimeSpan.FromMilliseconds(300);
        }

        void removeGoldenHighlight_Tick(object sender, object e)
        {
            removeGoldenHighlight.Stop();
            goldenHighlight = false;
            updateAll();
            try
            {
                CompleteResult result = _fld.complete();
                switch (result)
                {
                    case CompleteResult.OK: _fld.switchTurn(); break;
                    case CompleteResult.UNKNOWN:
                        {
                            //var mr = MessageBox.Show(ResLoader.getRes("fgcfAddText"), ResLoader.getRes("fgcfAddTitle"), MessageBoxButton.OKCancel);
                            MessageDialog md = new MessageDialog(ResLoader.getRes("fgcfAddText"), ResLoader.getRes("fgcfAddTitle"));
                            md.Commands.Add(new UICommand(
                                ResLoader.getRes("msYes"), (target) =>
                                {
                                    WordList.add(_fld.lastWord);
                                    _fld.removeSkips();
                                    _fld.complete();
                                    _fld.switchTurn();
                                }));
                            md.Commands.Add(new UICommand(
                                ResLoader.getRes("msNo"), (target) =>
                                {
                                    _fld.cancel();
                                }));
                            md.ShowAsync();
                            break;
                        }
                    case CompleteResult.USED:
                        {
                            MessageDialog md = new MessageDialog(ResLoader.getRes("fgcfUsedText"), ResLoader.getRes("fgcfUsedTitle"));
                            md.ShowAsync();
                            _fld.cancel();
                            break;
                        }
                }
            }
            catch (GameException gex)
            {
                MessageDialog md = new MessageDialog(gex.text, gex.title);
                md.ShowAsync();
                _fld.cancel();
            }
        }
        bool pointerPressed = false;

        void MainGrid_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            pointerPressed = false;
        }

        void MainGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            pointerPressed = true;
        }

        void MainGrid_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!pointerPressed) return;
            double x = e.GetCurrentPoint((UIElement)sender).Position.X;
            double y = e.GetCurrentPoint((UIElement)sender).Position.Y;
            Canvas.SetLeft(dPointer, x);
            Canvas.SetTop(dPointer, y);
            processSwipe(x, y);
        }
        /*
        void MainGrid_MouseMove(object sender, MouseEventArgs e)
        {

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
        }*/

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

        private void StackPanel_Tapped_1(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            finishDelay();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Field.cancel();
        }
        /*
        private void TextBlock_Tap(object sender, GestureEventArgs e)
        {
            finishDelay();
        }*/
    }
}
