using Balda;
using Balda.Logic;
using Balda.NET.Logic;
using Balda02.PlatformDependent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Balda02
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        UserSettings uss;
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
            this.NavigationCacheMode =
                Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            uss = new UserSettings();
            relayout();
            gameField.RestartGame += gameField_RestartGame;
            gameField.onNewLetterRequired += gameField_onNewLetterRequired;
            startGame(false);
            sbkbout.Completed += (ss, ev) => { Keyboard.Visibility = Visibility.Collapsed; };
        }

        private void gameField_onNewLetterRequired(object sender, EventArgs e)
        {
            Keyboard.Visibility = Visibility.Visible;
            sbkbin.Begin();
        }

        private void gameField_RestartGame()
        {
            startGame(true);
        }

        GameLanguage formedLang = null;
        void setAlphabetGrid(Grid g)
        {
            g.ColumnDefinitions.Clear();
            g.RowDefinitions.Clear();
            if (formedLang != uss.Language)
            {
                g.Children.Clear();
                StackPanel wrapper = new StackPanel();
                wrapper.Orientation = Orientation.Vertical;
                g.Background = new SolidColorBrush(Colors.Black);
                wrapper.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                wrapper.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                wrapper.Margin = new Thickness(50);
                g.Children.Add(wrapper);
                formedLang = uss.Language;
                string[] rows = formedLang.keyboard.Split(';');
                /*&for (int x = 0; x < rows.Length; ++x)
                {
                    g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1,GridUnitType.Auto) });
                }*/
                int ri = 0;
                foreach (string row in rows)
                {
                    StackPanel sp = new StackPanel();
                    sp.Orientation=Orientation.Horizontal;
                    wrapper.Children.Add(sp);
                    sp.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                    //g.Children.Add(sp);
                    //Grid.SetRow(sp, ri);
                    ++ri;
                    for (int t = 0; t < row.Length; ++t)
                    {
                        Button b = new Button();
                        b.Click += bb_Click;
                        b.Content = char.ToLower(row[t]);
                        b.Background = new SolidColorBrush(Colors.DarkGray);
                        //b.Style = App.Current.Resources["ButtonStyle"] as Style;
                        b.FontSize = 50;
                        b.Padding = new Thickness(5);
                        b.MinWidth = 100;
                        sp.Children.Add(b);
                        //g.Children.Add(b);
                    }
                    if (ri == rows.Length)
                    {
                        Button back = new Button();
                        back.Click+=back_Click;
                        back.FontSize = 30;
                        back.Content = ResLoader.getRes("mpCancel");
                        sp.Children.Add(back);
                    }
                }
            }
                        
        }

        void back_Click(object sender, RoutedEventArgs e)
        {
            field.cancel();
            sbkbout.Begin();
        }
        void bb_Click(object sender, RoutedEventArgs e)
        {
            field.putKey(((Button)sender).Content.ToString().ToUpper());
            //gameView.Visibility = System.Windows.Visibility.Visible;
            //Keyboard.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            sbkbout.Begin();
        }

        void relayout()
        {
            setAlphabetGrid(Keyboard);
        }

        public static Field field;


        public async void startGame(bool restart)
        {
            if (restart && (field.players[0].score > 0 || field.players[1].score > 0) && !field.demo)
            {
                MessageDialog md = new MessageDialog(ResLoader.getRes("cfRestartText"),ResLoader.getRes("cfRestartTitle"));
                IUICommand cmdOk = new UICommand(ResLoader.getRes("msYes"));
                md.Commands.Add(cmdOk);
                md.Commands.Add(new UICommand(ResLoader.getRes("msNo")));
                cmdOk.Invoked = async (target) =>
                {
                    await reallyStartGame(restart, false);
                    return;
                };
                await md.ShowAsync();
            }
            else
            {
                await reallyStartGame(restart, false);
            }
                        

        }

        async Task reallyStartGame(bool restart,bool demo)
        {
            await WordList.ReadFileContents();
            await UserSettings.getDUsername();
            relayout();
            if (!restart && SettingsWrapper.ApplicationSettings.ContainsKey("lastState"))
            {
                field = new Field();
                field.loadState(SettingsWrapper.ApplicationSettings);
            }
            else
            {
                field = new Field(WordList.getWordOfLength(Field.FieldSize));
            }
            gameField.Field = field;
            if (demo)
            {
                field.setupPlayersDemo();
            }
            DataContext = field;
            field.start();
            return;
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            startGame(true);
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            bool restart = true;
            if (restart && (field.players[0].score > 0 || field.players[1].score > 0))
            {
                MessageDialog md = new MessageDialog(ResLoader.getRes("cfDemoText"),ResLoader.getRes("cfDemoTitle"));
                IUICommand cmdOk = new UICommand(ResLoader.getRes("msYes"));
                md.Commands.Add(cmdOk);
                md.Commands.Add(new UICommand(ResLoader.getRes("msNo")));
                cmdOk.Invoked = async (target) =>
                {
                    await reallyStartGame(restart, true);
                    return;
                };
                await md.ShowAsync();
            }
            else
            {
                await reallyStartGame(restart, true);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(SettingsPage));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            field.SaveState(SettingsWrapper.ApplicationSettings);
            SettingsWrapper.Save();
            base.OnNavigatedFrom(e);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(HelpPage));
        }
    }
}
