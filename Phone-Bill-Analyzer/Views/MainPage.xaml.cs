using System;
using Phone_Bill_Analyzer.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using PBA_Application;
using Template10.Services.NavigationService;

namespace Phone_Bill_Analyzer.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        private void OnPhoneBillSelected(object sender, ItemClickEventArgs e)
        {
            //ObservablePhoneBill phoneBill = (ObservablePhoneBill)e.ClickedItem;
            //NavigationService.Navigate(typeof(Views.DetailPage), phoneBill.BillNo);
        }
    }
}
