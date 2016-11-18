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
using Windows.ApplicationModel.Store;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Phone_Bill_Analyzer.ViewModels
{
    public class UpgradePageViewModel : ViewModelBase
    {
        public UpgradePageViewModel()
        {}

        private Visibility _visibility_mode = Visibility.Collapsed;
        public Visibility VisibilityMode
        {
            get { return _visibility_mode; }
            set { Set(ref _visibility_mode, value); }
        }

        private String _current_license;
        public String CurrentLicense
        {
            get { return _current_license; }
            set { Set(ref _current_license, value); }
        }

        private String _premium_price;
        public String PremiumPrice
        {
            get { return _premium_price; }
            set { Set(ref _premium_price, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("UpgradePage");
            await getProductDetails();

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

        private async Task getProductDetails()
        {
            if (PBAApplication.getInstance().IsPremiumUser())
            {
                VisibilityMode = Visibility.Collapsed;
                CurrentLicense = "You have already upgraded to premium service";
            }
            else
            {
                VisibilityMode = Visibility.Visible;

                CurrentLicense = "Currently you are using free version of the app";

                try
                {
                    //ListingInformation li = await CurrentAppSimulator.LoadListingInformationAsync();
                    ListingInformation li = await CurrentApp.LoadListingInformationAsync();

                    ProductListing pListing;
                    if (li.ProductListings.TryGetValue("premium_user", out pListing))
                    {
                        PremiumPrice = pListing.FormattedPrice;
                    }
                    else
                    {
                        PremiumPrice = "1.00 USD";
                    }                    
                }
                catch (Exception ex)
                {
                    PremiumPrice = "1.00 USD";
                }
                
            }
        }

        public async Task UpgradeToPremium()
        {
            MessageDialog messageDialog;

            try
            {
                //PurchaseResults results = await CurrentAppSimulator.RequestProductPurchaseAsync("premium_user");
                PurchaseResults results = await CurrentApp.RequestProductPurchaseAsync("premium_user");

                //Check the license state to determine if the in-app purchase was successful.
                if (PBAApplication.getInstance().IsPremiumUser())
                {
                    messageDialog = new MessageDialog("Thank you! Please restart the app for changes to take efffect.");
                    VisibilityMode = Visibility.Collapsed;
                    CurrentLicense = "You have already upgraded to premium service";
                }
                else
                {
                    messageDialog = new MessageDialog("Some error occured during purchase. Please try again.");
                }
            }
            catch (Exception ex)
            {
                // The in-app purchase was not completed because an error occurred.
                messageDialog = new MessageDialog("Some exception occured during purchase. Please try again.");
            }

            await messageDialog.ShowAsync();
        }
    }
}

