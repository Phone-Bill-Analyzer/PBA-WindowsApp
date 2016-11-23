using PBA_Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Contacts;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Phone_Bill_Analyzer.ViewModels
{
    public class ContactsPageViewModel : ViewModelBase
    {
        public ContactsPageViewModel()
        {}

        private Visibility _visibility_mode = Visibility.Visible;
        public Visibility VisibilityMode
        {
            get { return _visibility_mode; }
            set { Set(ref _visibility_mode, value); }
        }

        private List<ContactData> _contactList = new List<ContactData>();
        public List<ContactData> ContactList
        {
            get { return _contactList; }
            set { Set(ref _contactList, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("ContactsPage");
            //FileName = (suspensionState.ContainsKey(nameof(FileName))) ? suspensionState[nameof(FileName)]?.ToString() : parameter?.ToString();

            ContactList = PBAApplication.getInstance().GetAppContactList().ToList();

            if (PBAApplication.getInstance().IsPremiumUser())
            {
                VisibilityMode = Visibility.Collapsed;
            }
            else
            {
                VisibilityMode = Visibility.Visible;
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                //suspensionState[nameof(FileName)] = FileName;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public async void SaveContactData(object sender, RoutedEventArgs e)
        {
            Views.Busy.SetBusy(true, "Saving Data.. Please Wait...");
            PBAApplication.getInstance().SaveContactData(ContactList);
            Views.Busy.SetBusy(false);

            MessageDialog messageDialog = new MessageDialog("Super.. Contact data saved.");
            await messageDialog.ShowAsync();
        }

        public async void SyncContactsFromDevice(object sender, RoutedEventArgs e)
        {
            Views.Busy.SetBusy(true, "Please Wait...We are mapping phone numbers to your contacts");

            bool success = await PBAApplication.getInstance().SyncContactsFromDevice();
            ContactList.Clear();
            ContactList = PBAApplication.getInstance().GetAppContactList().ToList();

            Views.Busy.SetBusy(false);

            MessageDialog messageDialog;

            if (success)
            {
                messageDialog = new MessageDialog("Super.. Your contacts are mapped.");
            }
            else
            {
                messageDialog = new MessageDialog("Oops !! Something went wrong, we could not read map contacts.");
            }
            await messageDialog.ShowAsync();
        }

    }
}

