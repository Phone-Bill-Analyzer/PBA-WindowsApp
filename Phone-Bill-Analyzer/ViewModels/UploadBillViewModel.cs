using PBA_Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Phone_Bill_Analyzer.ViewModels
{
    public class UploadBillViewModel : ViewModelBase
    {
        public UploadBillViewModel()
        {
        }

        private ServiceProvider _serviceProvider;
        public List<ServiceProvider> ServiceProviderList
        {
            get { return PBAApplication.getInstance().ServiceProviderList.ToList(); }
        }

        private StorageFile _file;
        private string _fileName = "Select a file";
        public string FileName
        {
            get { return _fileName; }
            set { Set(ref _fileName, value); }
        }

        public string FilePassword { get; set; }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //FileName = (suspensionState.ContainsKey(nameof(FileName))) ? suspensionState[nameof(FileName)]?.ToString() : parameter?.ToString();
            FilePassword = "";
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

        public async void BrowseFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;

            picker.FileTypeFilter.Add(".pdf");

            Views.Busy.SetBusy(true, "Wait Madi...");
            _file = await picker.PickSingleFileAsync();
            Views.Busy.SetBusy(false);

            if (_file != null)
            {
                // Application now has read/write access to the picked file
                FileName = _file.Name;
            }
            else
            {
                // Operation cancelled.
            }
        }

        public void ServiceProviderSelected(object sender, SelectionChangedEventArgs e)
        {
            _serviceProvider = (ServiceProvider)((ComboBox)sender).SelectedItem;
        }

        public async void ParseFile(object sender, RoutedEventArgs e)
        {
            MessageDialog messageDialog;

            if (_serviceProvider == null)
            {
                messageDialog = new MessageDialog("Please select a service provider from the list.");
                await messageDialog.ShowAsync();
                return;
            }

            PhoneBill pb = new PhoneBill(_file);
            pb.BillType = _serviceProvider.Code;
            pb.FilePassword = FilePassword;

            Views.Busy.SetBusy(true, "Please Wait...We are reading your bill information");
            bool success = await pb.ReadPDFFile();
            Views.Busy.SetBusy(false);

            if (success)
            {
                messageDialog = new MessageDialog("Super.. We have read your bill. You can analyze it now.");

                // Add to main List
                PBAApplication.getInstance().AddBillToList(pb);
            }
            else
            {
                messageDialog = new MessageDialog("Oops !! Something went wrong, we could not understand your bill. Please try again.");
            }

            await messageDialog.ShowAsync();
        }

    }
}

