using Template10.Mvvm;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using PBA_Application;

namespace Phone_Bill_Analyzer.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                _infoText = "Designtime value";
            }
        }

        string _infoText = "";
        public string InfoText { get { return _infoText; } set { Set(ref _infoText, value); } }

        private List<ObservablePhoneBill> _PhoneBillList = PBAApplication.getInstance().PhoneBillList.ToList();
        public List<ObservablePhoneBill> PhoneBillList
        {
            get { return _PhoneBillList; }
            set { Set(ref _PhoneBillList, value); }
        }
        
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("MainPage");

            if (suspensionState.Any())
            {
                _infoText = suspensionState[nameof(_infoText)]?.ToString();
            }

            PhoneBillList = PBAApplication.getInstance().PhoneBillList.ToList();

            if (PhoneBillList.Count <= 0)
            {
                InfoText = "Upload a Bill to start analyzing";
            }
            else
            {
                InfoText = "Select a bill to analyze OR upload a new one";
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(_infoText)] = _infoText;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void GotoUploadBillPage() =>
            NavigationService.Navigate(typeof(Views.UploadBill), "");

        public void GotoDetailsPage() =>
            NavigationService.Navigate(typeof(Views.DetailPage), "");

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);

    }
}

