using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Coding4Fun.Toolkit.Controls;
using ItsYourShout.Classes;
using Microsoft.Phone.UserData;

namespace ItsYourShout.Pages
{
    public partial class AddEditGroup
    {
        public AppSettings appSettings { get; set; }
        private ShoutGroup currentGroup { get; set; }

        public AddEditGroup()
        {
            InitializeComponent();
            appSettings = new AppSettings();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get a dictionary of query string keys and values.
            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;

            // Ensure that there is at least one key in the query string, and check whether the "token" key is present.
            if (queryStrings.ContainsKey("groupId"))
            {
                currentGroup = appSettings.AvailableGroups.SingleOrDefault(g => g.GroupId == queryStrings["groupId"]);
                PopulateForm();
            }
            else
            {
                this.GroupMembers.ItemsSource = new List<Shouter> {new Shouter {Name = ShoutGroupExtensions.DefaultShoutName, TimesShouted = 0}};
            }
        }

        private void PopulateForm()
        {
            if (currentGroup != null)
            {
                this.PageTitle.Text = "Edit Group";
                this.GroupName.Tag = currentGroup.GroupId;
                this.GroupName.Text = currentGroup.Name;
                this.GroupMembers.ItemsSource = currentGroup.Shouters;

                SetEnabledButton(currentGroup.GroupIcon);
            }
        }

        private bool SaveOrUpdateGroup(bool redirectHome)
        {
            var isExisting = currentGroup != null;
            var group = currentGroup ?? new ShoutGroup();

            group.Name = GroupName.Text;
            group.GroupIcon = GetEnabledButton();

            if (isExisting)
            {
                if (group.UpdateGroup() && redirectHome)
                {
                    appSettings.CurrentGroup = group;
                    NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
                }
            }
            else
            {
                if (group.AddGroup() && redirectHome)
                {
                    appSettings.CurrentGroup = group;
                    NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
                }
            }

            currentGroup = group;
            return true;
        }

        private void SaveGroup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GroupName.Text))
            {
                MessageBox.Show("Please give your Group a name then try again.");
            }
            else
            {
                SaveOrUpdateGroup(true);
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            var goodToGo = currentGroup == null || appSettings.AvailableGroups.Contains(currentGroup);

            if (goodToGo || MessageBox.Show("You haven't saved your group yet. Do you wish to continue and lose your group?", "Unsaved Group", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
            }
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var name = currentGroup.Name;
            if (MessageBox.Show(string.Format("Are you sure you wish to delete {0}?", name), string.Format("Delete {0}?", name), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                currentGroup.DeleteGroup();
                NavigationService.Navigate(new Uri("/Pages/MainPage.xaml", UriKind.Relative));
            }
        }

        protected void ToggleTypeButtons(Button enableButton)
        {
            var buttons = TypeSelect.Children.OfType<Button>();
            foreach (var button in buttons)
            {
                button.BorderThickness = new Thickness(0);
            }
            enableButton.BorderThickness = new Thickness(2);
        }

        protected string GetEnabledButton()
        {
            var buttons = TypeSelect.Children.OfType<Button>();
            foreach (var button in buttons)
            {
                if (button.BorderThickness != new Thickness(0))
                {
                    return string.Format("/Assets/Icons/{0}.png", button.Tag);
                }
            }

            return string.Empty;
        }

        protected void SetEnabledButton(string buttonUri)
        {
            if (string.IsNullOrEmpty(buttonUri)) return;

            var buttons = TypeSelect.Children.OfType<Button>();
            foreach (var button in buttons)
            {
                if (buttonUri.Contains(button.Tag.ToString()))
                {
                    button.BorderThickness = new Thickness(2);
                }
                else
                {
                    button.BorderThickness = new Thickness(0);
                }
            }
        }

        private void Type_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            ToggleTypeButtons(button);
        }

        private void DeleteMember_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var panel = (StackPanel)sender;

            var name = panel.Tag.ToString();

            if (name == ShoutGroupExtensions.DefaultShoutName)
            {
                MessageBox.Show("Sorry, you can't delete yourself.");
            }
            else if (MessageBox.Show(string.Format("Are you sure you wish to delete {0} from this group?", name), string.Format("Delete {0}?", name), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (currentGroup.DeleteShouter(name))
                {
                    NavigationService.Navigate(new Uri(string.Format("/Pages/AddEditGroup.xaml?groupId={0}&x={1}", currentGroup.GroupId, Utility.CacheBreaker()), UriKind.Relative));
                }
            }
        }

        private void ButtonContacts_Click(object sender, RoutedEventArgs e)
        {
            var cons = new Contacts();

            AddContacts.Visibility = Visibility.Visible;
            ContactResultsLabel.Visibility = Visibility.Visible;
            ApplicationBar.IsVisible = false;

            //Identify the method that runs after the asynchronous search completes.
            cons.SearchCompleted += Contacts_SearchCompleted;

            //Start the asynchronous search.
            cons.SearchAsync(String.Empty, FilterKind.None, "IYS Contact Search");
        }

        private void Contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            try
            {
                //Bind the results to the user interface.
                //ContactResultsData.DataContext = e.Results;
                ContactResultsData.ItemsSource = AlphaKeyGroup<Contact>.CreateGroups(e.Results,
                System.Threading.Thread.CurrentThread.CurrentUICulture,
                    (Contact s) => { return s.DisplayName; }, true);

            }
            catch (System.Exception ex)
            {
                ContactResultsLabel.Text = "Unable to read contacts";
                ApplicationBar.IsVisible = true;
            }

            ContactResultsLabel.Text = e.Results.Any() ? "Select Members..." : "No contacts found.";
        }

        private void ContactName_Tap(object sender, GestureEventArgs e)
        {
            if (currentGroup == null)
            {
                SaveOrUpdateGroup(false);
            }

            var group = currentGroup;

            var textBox = sender as TextBlock;

            var newShouter = new Shouter();
            var memberName = textBox.Text.Split(' ');
            var abbrName = new StringBuilder();

            // Format the name so we get their full first name and the first letter of any following names. e.g. MichaelDS
            foreach(var name in memberName)
            {
                abbrName.Append(abbrName.Length < 1 ? name : name.Substring(0, 1));
            }
            newShouter.Name = abbrName.ToString();
            group.AddShouter(newShouter);

            ApplicationBar.IsVisible = true;
            AddContacts.Visibility = Visibility.Collapsed;
            NavigationService.Navigate(new Uri(string.Format("/Pages/AddEditGroup.xaml?groupId={0}&x={1}", group.GroupId, Utility.CacheBreaker()), UriKind.Relative));
        }

        //private void SaveContacts_Click(object sender, RoutedEventArgs e)
        //{
        //    if (currentGroup == null)
        //    {
        //        SaveOrUpdateGroup(false);
        //    }

        //    var group = currentGroup;
        //    var selectedMembers = ContactResultsData.SelectedItems;
        //    foreach (var newMember in selectedMembers)
        //    {
        //        var newShouter = new Shouter();
        //        var member = (Contact)newMember;
        //        newShouter.Name = string.Format("{0}{1}", member.CompleteName.FirstName, member.CompleteName.LastName.Substring(0, 1));
        //        group.AddShouter(newShouter);
        //    }

        //    ApplicationBar.IsVisible = true;
        //    AddContacts.Visibility = Visibility.Collapsed;
        //    NavigationService.Navigate(new Uri(string.Format("/Pages/AddEditGroup.xaml?groupId={0}&x={1}", group.GroupId, Utility.CacheBreaker()), UriKind.Relative));
        //}

        private void CancelContacts_Click(object sender, RoutedEventArgs e)
        {
            AddContacts.Visibility = Visibility.Collapsed;
            ApplicationBar.IsVisible = true;
        }

        private void AddMember_Click(object sender, RoutedEventArgs e)
        {
            if (currentGroup == null)
            {
                SaveOrUpdateGroup(false);
            }

            var scope = new InputScope();
            scope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.PersonalGivenName });

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
                var group = currentGroup;

                if (group.Shouters != null && group.Shouters.Any(s => s.Name == newName))
                {
                    MessageBox.Show("Sorry, there's already someone in the group with that name", "So Unoriginal!", MessageBoxButton.OK);
                }
                else
                {
                    var newShouter = new Shouter();
                    newShouter.Name = newName;

                    if (group.AddShouter(newShouter))
                    {
                        NavigationService.Navigate(new Uri(string.Format("/Pages/AddEditGroup.xaml?groupId={0}&x={1}", currentGroup.GroupId, Utility.CacheBreaker()), UriKind.Relative));
                    }
                }
            }
        }

    }
}