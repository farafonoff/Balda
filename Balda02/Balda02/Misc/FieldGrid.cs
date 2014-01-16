using System;
using Balda.NET.Logic;
using System.Windows;
using Balda.Logic;
using System.Collections.Generic;
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
using Balda02.PlatformDependent;
using Balda02;
using Windows.UI.Popups;
#endif

namespace Balda.NET
{
    public partial class FieldGrid:UserControl
    {
        Button[,] buttons = new Button[Field.FieldSize, Field.FieldSize];
        public void initButtons()
        {
            for (int i = 0; i < Field.FieldSize; ++i)
            {
                for (int j = 0; j < Field.FieldSize; ++j)
                {
                    Button bb = new Button();
                    MainGrid.Children.Insert(0, bb);
                    Grid.SetColumn(bb, i);
                    Grid.SetRow(bb, j);
                    buttons[i, j] = bb;
                    bb.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                    bb.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch;
                    bb.Click += new RoutedEventHandler(bb_Click);
                    bb.FontSize = 40;
                    bb.Background = new SolidColorBrush(getColorByState(LetterState.Empty,Colors.LightGray));
                    //bb.Style = (Style)Application.Current.Resources["ButtonStyle"];
                    bb.Tag = new LPosition(i, j);
                    bb.Padding = new Thickness(3);
                }
            }
        }
        Polygon getArrow()
        {
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(-0.5, -0.5));
            myPointCollection.Add(new Point(-0.5, 0.5));
            myPointCollection.Add(new Point(0.5, 0));
            Polygon myPolygon = new Polygon();
            myPolygon.Points = myPointCollection;
            myPolygon.Fill = new SolidColorBrush(Color.FromArgb(0xA0, 255, 255, 255));
            myPolygon.MinWidth = 30;
            myPolygon.MinHeight = 20;
            myPolygon.Stretch = Stretch.Fill;
            return myPolygon;
        }

        void drawPath()
        {
            List<LPosition> path = new List<LPosition>();
            if (Field._builder != null)
            {
                path = Field._builder.path;
            }
            else
                if (Field.aiWordAdded && Field.aiWordPath != null)
                {
                    path = new List<LPosition>(Field.aiWordPath);
                    path.Reverse();
                }
                else
                {
                    debugCanvas.Children.Clear();
                    return;
                }
            LPosition prev = null;
            debugCanvas.Children.Clear();
            foreach (LPosition lp in path)
            {
                if (prev != null)
                {
                    var ar = getArrow();
                    debugCanvas.Children.Add(ar);

                    RotateTransform rt = new RotateTransform();
                    rt.CenterX = ar.ActualWidth / 2;
                    rt.CenterY = ar.ActualHeight / 2;
                    if (prev.x < lp.x)
                    {
                    }
                    else
                        if (prev.x > lp.x)
                        {
                            rt.Angle = 180;
                        }
                        else if (prev.y < lp.y)
                        {
                            rt.Angle = 90;
                        }
                        else if (prev.y > lp.y)
                        {
                            rt.Angle = 270;
                        }
                    ar.RenderTransform = rt;
                    double aw = ar.ActualWidth;
                    double ah = ar.ActualHeight;

                    Canvas.SetLeft(ar, ((prev.x + lp.x + 1) * buttons[0, 0].ActualWidth - aw) / 2);
                    Canvas.SetTop(ar, ((prev.y + lp.y + 1) * buttons[0, 0].ActualHeight - ah) / 2);

                }
                prev = lp;
            }
        }

        void processSwipe(double x, double y)
        {
            int i = (int)(x * Field.FieldSize / MainGrid.ActualWidth);
            int j = (int)(y * Field.FieldSize / MainGrid.ActualHeight);
            if (i < 0) i = 0;
            if (j < 0) j = 0;
            if (i >= Field.FieldSize) i = Field.FieldSize - 1;
            if (j >= Field.FieldSize) j = Field.FieldSize - 1;
            /*Button b = buttons[i, j];
            b.Background = new SolidColorBrush(Colors.Green);*/
            var pos = new LPosition(i, j);
            var letter = _fld[pos];
            try
            {
                if ((letter.state == LetterState.New || letter.state == LetterState.Old) && _fld.state == FieldState.ComposingWord)
                {
                    Field.select(new LPosition(i, j));
                    drawPath();
                }
                //e.Handled = true;
                //buttons[i, j].Background = new SolidColorBrush(Colors.White);
            }
            catch (GameException gex)
            {
                MessageDialog md = new MessageDialog(gex.text,gex.title);
                md.ShowAsync();
                _fld.cancel();
            }
        }


        void finishDelay()
        {
            if (_fld.state == FieldState.OtherTurn)
            {
                _fld.finishDelaySwitch();
                return;
            }
        }



        LPosition selPosition;
        bool goldenHighlight = false;
        DispatcherTimer removeGoldenHighlight = new DispatcherTimer();

        void bb_Click(object sender, RoutedEventArgs e)
        {
            if (_fld.state == FieldState.OtherTurn)
            {
                _fld.finishDelaySwitch();
                return;
            }
            if (goldenHighlight) return;
            try
            {
                Button ss = (Button)sender;
                LPosition pos = (LPosition)ss.Tag;
                var oldstate = _fld[pos].state;
                _fld.select(pos);
                var letter = _fld[pos];
                if (_fld.state == FieldState.AddingLetter && letter.state == LetterState.New && onNewLetterRequired != null)
                {
                    selPosition = pos;
                    onNewLetterRequired(this, EventArgs.Empty);
                }
                else
                    if (_fld.state == FieldState.ComposingWord && (oldstate == LetterState.SelectedNew || oldstate == LetterState.Selected))
                    {
                        goldenHighlight = true;
                        removeGoldenHighlight.Start();
                        updateAll();
                    }
            }
            catch (GameException gex)
            {
                MessageDialog md = new MessageDialog(gex.text, gex.title);
                md.ShowAsync();
                _fld.cancel();
            }
        }
        Field _fld;
        public Field Field
        {
            get
            {
                return _fld;
            }
            set
            {
                if (_fld != null)
                {
                    _fld.updated -= _fld_updated;
                    _fld.onHighlight -= _fld_onHighlight;
                    _fld.PropertyChanged -= _fld_PropertyChanged;
                }
                _fld = value;
                _fld.updated += _fld_updated;
                _fld.onHighlight += _fld_onHighlight;
                _fld.PropertyChanged += _fld_PropertyChanged;
                DataContext = _fld;
                updateAll();
            }
        }

        void _fld_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "aiWordAdded")
            {
                if (Field.aiWordAdded) drawPath();
            }
        }

        void _fld_onHighlight(Field sender, Field.HighLightEventArgs args)
        {
            UiThreadRunner.runOnUiThread(() =>
            {
                buttons[args.root.x, args.root.y].Background = new SolidColorBrush(getColorByState(args.state, ((SolidColorBrush) buttons[args.root.x, args.root.y].Background).Color));
            });
        }

        void _fld_updated(Field sender, FieldUpdatedEventArgs args)
        {
            int i = args.lp.x;
            int j = args.lp.y;
            UiThreadRunner.runOnUiThread(() => { updateButton(i, j); });
        }

        public Color getColorByState(LetterState ls, Color old)
        {
            Color cl = Colors.Black;
            if (!goldenHighlight)
            {
                switch (ls)
                {
                    case (LetterState.Empty): cl = Colors.LightGray; break;
                    case (LetterState.New): cl = Color.FromArgb(0xFF, 0x1b, 0xa1, 0xe2); break;
                    case (LetterState.Old): cl = Color.FromArgb(0xFF, 0x00, 0xAB, 0xA9); break;
                    case (LetterState.Selected): cl = Color.FromArgb(0xFF, 0xA2, 0x00, 0xFF); break;
                    case (LetterState.SelectedNew): cl = Color.FromArgb(0xFF, 0xE5, 0x14, 0x00); break;
                }
            }
            else
            {
                switch (ls)
                {
                    case (LetterState.Empty): cl = Colors.LightGray; break;
                    case (LetterState.Old): cl = old; break;
                    default:
                        cl = Color.FromArgb(0xFF, 0xF0, 0x96, 0x09);
                        break;
                }
            }
            return cl;

        }

        private void updateButton(int i, int j)
        {
            buttons[i, j].Content = _fld[i, j].letter;
            Color cl = Colors.Black;
            buttons[i, j].Background = new SolidColorBrush(getColorByState(_fld[i, j].state, (buttons[i, j].Background as SolidColorBrush).Color));
            drawPath();
        }

        private void updateAll()
        {
            if (_fld == null) return;
            for (int i = 0; i < Field.FieldSize; ++i)
            {
                for (int j = 0; j < Field.FieldSize; ++j)
                {
                    updateButton(i, j);
                }
            }
        }

        public event EventHandler onNewLetterRequired;
        public event Action RestartGame;


    }
}
