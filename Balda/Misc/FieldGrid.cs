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
#endif

namespace Balda.NET
{
    public partial class FieldGrid:UserControl
    {
        Button[,] buttons;
        int fSize;
        public void initButtons()
        {
            MainGrid.Children.Clear();
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();
            buttons = new Button[Field.FieldSize, Field.FieldSize];
            for (int i = 0; i < Field.FieldSize; ++i)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(cd);
                MainGrid.RowDefinitions.Add(rd);
            }
                for (int i = 0; i < Field.FieldSize; ++i)
                {
                    for (int j = 0; j < Field.FieldSize; ++j)
                    {
                        Button bb = new Button();
                        buttons[i, j] = bb;
                        bb.Click += new RoutedEventHandler(bb_Click);
                        bb.Style = (Style)Application.Current.Resources["ButtonStyle"];
                        bb.Tag = new LPosition(i, j);
                        bb.Padding = new Thickness(3);
                        bb.Background = new SolidColorBrush(getColorByState(LetterState.Empty));
                        bb.Content = " ";
                        MainGrid.Children.Insert(0, bb);
                        Grid.SetColumn(bb, i);
                        Grid.SetRow(bb, j);
                    }
                }
                debugCanvas = new Canvas();
                MainGrid.Children.Add(debugCanvas);
                Grid.SetRowSpan(debugCanvas, Field.FieldSize);
                Grid.SetColumnSpan(debugCanvas, Field.FieldSize);
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
        Canvas debugCanvas;
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
                var res = MessageBox.Show(gex.text, gex.title, MessageBoxButton.OK);
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

        void bb_Click(object sender, RoutedEventArgs e)
        {
            if (_fld.state == FieldState.OtherTurn)
            {
                _fld.finishDelaySwitch();
                return;
            }
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
                        CompleteResult result = _fld.complete();
                        switch (result)
                        {
                            case CompleteResult.OK: _fld.switchTurn(); break;
                            case CompleteResult.UNKNOWN:
                                {
                                    var mr = MessageBox.Show(AppResources.fgcfAddText, AppResources.fgcfAddTitle, MessageBoxButton.OKCancel);
                                    if (mr == MessageBoxResult.OK)
                                    {
                                        WordList.add(_fld.lastWord);
                                        _fld.removeSkips();
                                        _fld.complete();
                                        _fld.switchTurn();
                                    }
                                    else
                                        _fld.cancel();
                                    break;
                                }
                            case CompleteResult.USED:
                                {
                                    MessageBox.Show(AppResources.fgcfUsedText, AppResources.fgcfUsedTitle, MessageBoxButton.OK);
                                    _fld.cancel();
                                    break;
                                }
                        }
                    }
            }
            catch (GameException gex)
            {
                var res = MessageBox.Show(gex.text, gex.title, MessageBoxButton.OK);
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
                if (fSize != Field.FieldSize)
                {
                    fSize = Field.FieldSize;
                    initButtons();
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
                buttons[args.root.x, args.root.y].Background = new SolidColorBrush(getColorByState(args.state));
            });
        }

        void _fld_updated(Field sender, FieldUpdatedEventArgs args)
        {
            int i = args.lp.x;
            int j = args.lp.y;
            UiThreadRunner.runOnUiThread(() => { updateButton(i, j); });
        }

        public Color getColorByState(LetterState ls)
        {
            Color cl = Colors.Black;
            switch (ls)
            {
                case (LetterState.Empty): cl = Colors.LightGray; break;
                case (LetterState.New): cl = Color.FromArgb(0xFF, 0x1b, 0xa1, 0xe2); break;
                case (LetterState.Old): cl = Color.FromArgb(0xFF, 0x00, 0xAB, 0xA9); break;
                case (LetterState.Selected): cl = Color.FromArgb(0xFF, 0xA2, 0x00, 0xFF); break;
                case (LetterState.SelectedNew): cl = Color.FromArgb(0xFF, 0xE5, 0x14, 0x00); break;
            }
            return cl;

        }

        private void updateButton(int i, int j)
        {
            buttons[i, j].Content = _fld[i, j].letter;
            Color cl = Colors.Black;
            buttons[i, j].Background = new SolidColorBrush(getColorByState(_fld[i, j].state));
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
