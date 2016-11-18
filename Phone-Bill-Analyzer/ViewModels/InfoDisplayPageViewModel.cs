using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace Phone_Bill_Analyzer.ViewModels
{
    public class InfoDisplayPageViewModel : ViewModelBase
    {
        public InfoDisplayPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
            }
        }

        private string _infoTitle = "Initial Content";
        public string InfoTitle { get { return _infoTitle; } set { Set(ref _infoTitle, value); } }

        private string _infoContent = "Initial Content";
        public string InfoContent { get { return _infoContent; } set { Set(ref _infoContent, value); } }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("InfoDisplayPage");

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            InfoTitle = localSettings.Values["ToastMessageTitle"].ToString();
            InfoContent = localSettings.Values["ToastMessageContent"].ToString();

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void GotoHomePage()
        {
            NavigationService.Navigate(typeof(Views.MainPage));
            NavigationService.ClearHistory();
            NavigationService.ClearCache(true);

        }
    }
}