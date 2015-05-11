using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
using ItsYourShout.Classes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace ItsYourShout.Pages
{
    public partial class MainPage
    {
        public AppSettings appSettings { get; set; }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            appSettings = new AppSettings();
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshGroups();
            ShowGroup();
        }

        private void RefreshGroups()
        {
            if (appSettings.AvailableGroups == null)
            {
                NoGroupsText.Visibility = Visibility.Visible;
                AddFirstGroup.Visibility = Visibility.Visible;
                AddFirstGroupShadow.Visibility = Visibility.Visible;
                ButtonShadow.Visibility = Visibility.Collapsed;
                CurrentShouter.Visibility = Visibility.Collapsed;
                SkipShout.Visibility = Visibility.Collapsed;
                SkipShadow.Visibility = Visibility.Collapsed;
                PreviousShouter.Visibility = Visibility.Collapsed;
                MainPanorama.DefaultItem = MainPanorama.Items[1];
            }
            else
            {
                NoGroupsText.Visibility = Visibility.Collapsed;
                AddFirstGroup.Visibility = Visibility.Collapsed;
                AddFirstGroupShadow.Visibility = Visibility.Collapsed;
                CurrentShouter.Visibility = Visibility.Visible;
                ButtonShadow.Visibility = Visibility.Visible;
                SkipShout.Visibility = Visibility.Visible;
                SkipShadow.Visibility = Visibility.Visible;
                PreviousShouter.Visibility = Visibility.Visible;
                AvailableGroups.ItemsSource = appSettings.AvailableGroups;
                GroupStats.ItemsSource = appSettings.AvailableGroups;
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/AddEditGroup.xaml", UriKind.Relative));
        }

        protected void ShowGroup()
        {
            if (appSettings.CurrentGroup != null)
            {
                CurrentGroup.Header = appSettings.CurrentGroup.Name;
                CurrentShouter.Content = appSettings.CurrentGroup.CurrentShouter;
                PreviousShouter.Text = appSettings.CurrentGroup.PreviousShouter;

                MainPanorama.DefaultItem = MainPanorama.Items[0];
            }
            else
            {
                MainPanorama.DefaultItem = MainPanorama.Items[1];
                ApplicationBar.IsVisible = false;
            }
        }

        private void CurrentShouter_Click(object sender, RoutedEventArgs e)
        {
            appSettings.CurrentGroup.Shout(true);
            ShowGroup();
        }

        private void SelectGroup_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var selectedItem = (StackPanel) sender;
            var groupGuidControl = (TextBlock)selectedItem.FindName("Guid");

            var groupId = groupGuidControl.Text;
            var group = groupId.FindGroupById();

            appSettings.CurrentGroup = group;
            ShowGroup();
        }

        private void AddShouter_Click(object sender, System.EventArgs e)
        {
            var scope = new InputScope();
            scope.Names.Add(new InputScopeName {NameValue = InputScopeNameValue.PersonalGivenName});

            var msgPrompt = new MessagePrompt
            {
                Title = "Enter Name of New Member",
                Body = new TextBox { Text = "", FontSize = 30.0, TextWrapping = TextWrapping.Wrap, InputScope = scope },
                IsCancelVisible = true
            };
            msgPrompt.Completed += ShouterNameEntered;
            msgPrompt.Show();
        }

        protected void ShouterNameEntered(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult == PopUpResult.Ok)
            {
                var prompt = (MessagePrompt)sender;
                var textBox = (TextBox)prompt.Body;

                var newName = textBox.Text;

                var group = appSettings.CurrentGroup;

                if (group.Shouters.Any(s => s.Name == newName))
                {
                    MessageBox.Show("Sorry, there's already someone in the group with that name", "So Unoriginal!", MessageBoxButton.OK);
                }
                else
                {
                    var newShouter = new Shouter();
                    newShouter.Name = newName;

                    if (group.AddShouter(newShouter))
                    {
                        RefreshGroups();
                    }
                }
            }
        }

        private void DeleteGroup_Click(object sender, System.EventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to delete this group?", "Delete Group", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                appSettings.CurrentGroup.DeleteGroup();
                NavigationService.Navigate(new Uri(string.Format("/Pages/MainPage.xaml?x={0}", Utility.CacheBreaker()), UriKind.Relative));
            }
        }
        
        private void MainPanorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationBar.IsVisible = e.AddedItems[0] == MainPanorama.Items[0];
        }

        private void EditGroup_Click(object sender, EventArgs e)
        {
            if (appSettings.CurrentGroup != null)
            {
                NavigationService.Navigate(
                    new Uri(string.Format("/Pages/AddEditGroup.xaml?groupId={0}", appSettings.CurrentGroup.GroupId),
                            UriKind.Relative));
            }else
            {
                MessageBox.Show("Please select a Group first");
                MainPanorama.DefaultItem = MainPanorama.Items[1];
            }
        }

        private void SkipShout_Tap(object sender, GestureEventArgs e)
        {
            appSettings.CurrentGroup.Shout(false);
            ShowGroup();
        }

        private void RateApp_Click(object sender, EventArgs e)
        {
            new MarketplaceReviewTask().Show();
        }
    }
}