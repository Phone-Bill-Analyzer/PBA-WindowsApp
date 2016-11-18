using PBA_Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Phone_Bill_Analyzer.ViewModels
{
    public class DetailPageViewModel : ViewModelBase
    {
        public DetailPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                BillNo = "Designtime value";
            }
        }

        private string _pi1 = "ms-appx-web:///Charts/all_contacts_table.html";
        public string PI1 { get { return _pi1; } set { Set(ref _pi1, value); } }

        private string _pi2 = "ms-appx-web:///Charts/itemized_bill_details.html";
        public string PI2 { get { return _pi2; } set { Set(ref _pi2, value); } }

        private string _pi3 = "ms-appx-web:///Charts/top_5_pie_chart.html";
        public string PI3 { get { return _pi3; } set { Set(ref _pi3, value); } }

        private string _pi4 = "ms-appx-web:///Charts/contact_group_summary.html";
        public string PI4 { get { return _pi4; } set { Set(ref _pi4, value); } }

        private string _billNo = "Default";
        public string BillNo { get { return _billNo; } set { Set(ref _billNo, value); } }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("DetailPage");
            BillNo = (suspensionState.ContainsKey(nameof(BillNo))) ? suspensionState[nameof(BillNo)]?.ToString() : parameter?.ToString();
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(BillNo)] = BillNo;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Views.Busy.SetBusy(true, "Wait Madi...");
            ObservablePhoneBill pb = PBAApplication.getInstance().GetObservablePhoneBill(BillNo);
            if (pb != null)
            {
                sender.AddWebAllowedObject("phone_bill", pb);
            }
        }

        public void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Views.Busy.SetBusy(false);
        }
    }
}

