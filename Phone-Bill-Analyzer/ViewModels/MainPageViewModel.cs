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
                Value = "Designtime value";
            }
        }

        string _Value = "Gas";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }

        private List<ObservablePhoneBill> _PhoneBillList = new List<ObservablePhoneBill>();
        public List<ObservablePhoneBill> PhoneBillList
        {
            get {
                _PhoneBillList.Add(new ObservablePhoneBill("Test123"));
                _PhoneBillList.Add(new ObservablePhoneBill("TestABC"));
                _PhoneBillList.Add(new ObservablePhoneBill("TestXYZ"));
                _PhoneBillList.Add(new ObservablePhoneBill("Test456"));
                _PhoneBillList.Add(new ObservablePhoneBill("TestLMN"));
                _PhoneBillList.Add(new ObservablePhoneBill("Test789"));
                _PhoneBillList.Add(new ObservablePhoneBill("TestPQR"));
                _PhoneBillList.Add(new ObservablePhoneBill("Test000"));
                return _PhoneBillList;
            }
        }
        
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                Value = suspensionState[nameof(Value)]?.ToString();
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Value)] = Value;
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
            NavigationService.Navigate(typeof(Views.DetailPage), Value);

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);

    }
}

