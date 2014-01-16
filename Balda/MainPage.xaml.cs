using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Balda.NET.Logic;
using Balda.Logic;
using Balda.Misc;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using Balda.NET;

namespace Balda
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static Field field;
        public UserSettings uss;
        public Popup myPopup;
        static bool initialized = false;
        // Конструктор
        public MainPage()
        {
            InitializeComponent();
            /*uss = new UserSettings();
            relayout();*/
            field = lgameField.Field;
            lgameField.onNewLetterRequired += new EventHandler(gameField_onNewLetterRequired);
            lgameField.RestartGame += new Action(gameField_RestartGame);
            fieldPreparer.DoWork += prepareField;
            myPopup = new Popup() { IsOpen = true, Child = new Splash()};
        }

        BackgroundWorker fieldPreparer = new BackgroundWorker();

        private void ApplicationBarMenuItem_Click_2(object sender, EventArgs e)
        {
            if (fieldPreparer.IsBusy) return;
            bool restart = true;
            if (restart && (field.players[0].score > 0 || field.players[1].score > 0))
            {
                var res = MessageBox.Show(AppResources.cfDemoText, AppResources.cfDemoTitle, MessageBoxButton.OKCancel);
                if (res != MessageBoxResult.OK)
                {
                    return;
                }
            }
            //ApplicationBar.IsVisible = false;
            fieldPreparer.RunWorkerAsync(new prepareArg { restart = restart, demo = true });
        }

        public void startGame(bool restart)
        {
            if (fieldPreparer.IsBusy) return;
            if (restart && (field.players[0].score > 0 || field.players[1].score > 0) && !field.demo)
            {
                var res = MessageBox.Show(AppResources.cfRestartText, AppResources.cfRestartTitle, MessageBoxButton.OKCancel);
                if (res != MessageBoxResult.OK)
                {
                    return;
                }
            }
            ApplicationBar.IsVisible = false;
            fieldPreparer.RunWorkerAsync(new prepareArg { restart = restart, demo = false });
        }
        class prepareArg
        {
            public bool restart;
            public bool demo;
        }

        void prepareField(object sender, DoWorkEventArgs args/*bool restart,bool demo*/)
        {
            WordList.ReadFileContents();
            prepareArg arg = (prepareArg) args.Argument;
            Dispatcher.BeginInvoke(() =>
            {
                UserSettings us = new UserSettings();
                uss = us;
                Field.FieldSize = us.FieldSize;
                if (field != null)
                {
                    Scores.addScore(field,SettingsWrapper.ApplicationSettings);
                    SettingsWrapper.Save();
                }
                if (!arg.restart && SettingsWrapper.ApplicationSettings.ContainsKey("lastState"))
                {
                    try
                    {
                        field = new Field();
                        field.loadState(SettingsWrapper.ApplicationSettings);
                    } catch(Exception ex)
                    {
                        field = new Field(WordList.getWordOfLength(Field.FieldSize));
                    }
                }
                else
                {
                    field = new Field(WordList.getWordOfLength(Field.FieldSize));
                }
                lgameField.Field = field;
                gameView.DataContext = field;
                if (arg.demo) field.setupPlayersDemo();
                field.start();
                relayout();
                ApplicationBar.IsVisible = true;
                myPopup.IsOpen = false;
                initialized = true;
            });
        }

        void gameField_RestartGame()
        {
            startGame(true);
        }

        GameLanguage formedLang = null;

        void setUniformGrid(Grid g, int lodim, bool horizontal)
        {
            g.ColumnDefinitions.Clear();
            g.RowDefinitions.Clear();
            if (formedLang != uss.Language)
            {
                g.Children.Clear();
                formedLang = uss.Language;
                for (int t = 0; t < formedLang.alphabet.Length; ++t)
                {
                    Button b = new Button();
                    b.Click += bb_Click;
                    b.Content = char.ToUpper(formedLang.alphabet[t]);
                    b.Background = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
                    b.Style = App.Current.Resources["ButtonStyle"] as Style;
                    b.Padding = new Thickness(5);
                    g.Children.Add(b);
                }
            }
            int i, j;
            j = lodim;
            int div = formedLang.alphabet.Length / j;
            int mod = formedLang.alphabet.Length % j;
            if (mod > 0) ++div;
            i = div;
            if (!horizontal)
            {
                j = div;
                i = lodim;
            }

            for (int x = 0; x < i; ++x)
            {
                g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }
            for (int x = 0; x < j; ++x)
            {
                g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }
            var enumer = g.Children.GetEnumerator();
            bool finished = false;
            for (int y = 0; y < j; ++y)
            {
                for (int x = 0; x < i; ++x)
                {
                    finished = !enumer.MoveNext();
                    if (!finished)
                    {
                        Grid.SetColumn((FrameworkElement)enumer.Current, x);
                        Grid.SetRow((FrameworkElement)enumer.Current, y);
                    }
                }
            }
        }

        void moveFieldTo(Grid sp)
        {
                Grid container = ((Grid)lgameField.Parent);
                if (container != sp)
                {
                    if (container!=null)
                        container.Children.Remove(lgameField);
                    sp.Children.Add(lgameField);
                }
        }


        void relayout()
        {
            if (uss == null) uss = new UserSettings();
            if ((Orientation & PageOrientation.Landscape) != 0)
            {
                setUniformGrid(letterz, 4, true);
                PortraitView.Visibility = System.Windows.Visibility.Collapsed;
                //gameField.IsEnabled = false;
                //lgameField.IsEnabled = true;
                moveFieldTo(LandscapeContainer);
                LandscapeView.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                setUniformGrid(letterz, 4, false);
                PortraitView.Visibility = System.Windows.Visibility.Visible;
                //gameField.IsEnabled = true;
                //lgameField.IsEnabled = false;
                moveFieldTo(PortraitContainer);
                LandscapeView.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        void gameField_onNewLetterRequired(object sender, EventArgs e)
        {
            gameView.Visibility = System.Windows.Visibility.Collapsed;
            letterz.Visibility = System.Windows.Visibility.Visible;
        }

        void bb_Click(object sender, RoutedEventArgs e)
        {
            field.putKey(((Button)sender).Content.ToString());
            gameView.Visibility = System.Windows.Visibility.Visible;
            letterz.Visibility = System.Windows.Visibility.Collapsed;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (letterz.Visibility == System.Windows.Visibility.Visible)
                {
                    gameView.Visibility = System.Windows.Visibility.Visible;
                    letterz.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (initialized)
                    e.Cancel = field.cancel();
                else
                    e.Cancel = false;
            }
            catch (Exception ex)
            {
            }
            base.OnBackKeyPress(e);
        }
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            relayout();
            base.OnOrientationChanged(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (initialized)
            {
                field.SaveState(SettingsWrapper.ApplicationSettings);
                SettingsWrapper.Save();
            }
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (initialized)
            {
                if (field.players[0].score == 0 && field.players[1].score == 0)
                {
                    startGame(true);
                }
                else
                {
                    field.finishDelaySwitch();
                }
            } else
                startGame(false);
            buildAppBar();
            base.OnNavigatedTo(e);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }
        private void ApplicationBarMenuItemScoreboard_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Scoreboard.xaml", UriKind.Relative));
        }
        private void ApplicationBarMenuItem_Click_1(object sender, EventArgs e)
        {
            //animGrid();
            startGame(true);

        }

        private ApplicationBarIconButton mkItem(string text, EventHandler click,string icon)
        {
            var r = new ApplicationBarIconButton(new Uri("/icons/" + icon, UriKind.Relative));
            r.Text = text;
            if (click != null)
            {
                r.Click += click;
            }
            return r;
        }
        private ApplicationBarMenuItem mkItem(string text, EventHandler click)
        {
            var r = new ApplicationBarMenuItem(text);
            if (click != null)
            {
                r.Click += click;
            }
            return r;
        }
        private void buildAppBar()
        {
            ApplicationBar.MenuItems.Clear();
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(mkItem(AppResources.mmRestart, ApplicationBarMenuItem_Click_1,"appbar.refresh.png"));
            ApplicationBar.Buttons.Add(mkItem(AppResources.mmSettings, ApplicationBarMenuItem_Click, "appbar.settings.png"));
            ApplicationBar.Buttons.Add(mkItem(AppResources.dictionary, ApplicationBarMenuItemDict_Click, "appbar.book.help.png"));
            ApplicationBar.MenuItems.Add(mkItem(AppResources.mmScore, ApplicationBarMenuItemScore_Click));
            ApplicationBar.MenuItems.Add(mkItem(AppResources.mmDemo, ApplicationBarMenuItem_Click_2));
            ApplicationBar.Buttons.Add(mkItem(AppResources.mmHelp, ApplicationBarMenuItemHelp_Click, "appbar.question.png"));
            //ApplicationBar.MenuItems.Add(mkItem(AppResources.mmAbout, null));
        }
        private void ApplicationBarMenuItemScore_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Scoreboard.xaml", UriKind.Relative));
        }
        private void ApplicationBarMenuItemHelp_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/HelpPage.xaml", UriKind.Relative));
        }
        private void ApplicationBarMenuItemDict_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Dictionary.xaml", UriKind.Relative));
        }
        private void BannerAd_AdReceived(object sender, RoutedEventArgs e)
        {

        }

        private void BannerAd_AdLoading(object sender, RoutedEventArgs e)
        {

        }

        private void BannerAd_AdFailed(object sender, Google.AdMob.Ads.WindowsPhone7.AdException exception)
        {

        }

        private void BannerAd_AdFailed_1(object sender, Google.AdMob.Ads.WindowsPhone7.AdException exception)
        {

        }

        /*private void animGrid()
        {
            // locate the pivot control  header
            //var header = this.lgameField.Descendants().OfType<Button>().Single(d => d.Name == "HeadersListElement");

            // create the list of items to peel
            var peelList = this.lgameField.Descendants().OfType<Button>().Select(b=>b as FrameworkElement);

            peelList.Peel(() =>
            {
            });
        }*/
    }
}