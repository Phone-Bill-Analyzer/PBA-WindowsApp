using Windows.UI.Xaml;
using System.Threading.Tasks;
using Phone_Bill_Analyzer.Services.SettingsServices;
using Windows.ApplicationModel.Activation;
using Template10.Controls;
using System;
using Windows.UI.Xaml.Data;
using PBA_Application;
using Windows.ApplicationModel.Background;

namespace Phone_Bill_Analyzer
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : Template10.Common.BootStrapper
    {
        public App()
        {
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);

            #region App settings

            var _settings = SettingsService.Instance;
            RequestedTheme = _settings.AppTheme;
            CacheMaxDuration = _settings.CacheMaxDuration;
            ShowShellBackButton = _settings.UseShellBackButton;

            #endregion
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (Window.Current.Content as ModalDialog == null)
            {
                // create a new frame 
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);

                // create modal root
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(nav),
                    ModalContent = new Views.Busy(),
                };
            }
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // long-running startup tasks go here
            //await Task.Delay(2000);
            PBAApplication.getInstance().InitializeApplication();

            // Register backgroud task for Push Notifications
            await registerBackgroundTaskForPushNotification();

            var launchKind = DetermineStartCause(args);
            switch (launchKind)
            {
                case AdditionalKinds.SecondaryTile:
                    var tileargs = args as LaunchActivatedEventArgs;
                    NavigationService.Navigate(typeof(Views.MainPage), tileargs.Arguments);
                    break;

                case AdditionalKinds.Toast:
                    var toastargs = args as ToastNotificationActivatedEventArgs;
                    if (toastargs.Argument.Equals("ShowInfoMessage"))
                    {
                        //NavigationService.Navigate(typeof(Views.InfoDisplayPage));
                    }
                    else
                    {
                        NavigationService.Navigate(typeof(Views.MainPage));
                    }
                    break;

                case AdditionalKinds.JumpListItem:
                    //NavigationService.Navigate(typeof(Views.InfoDisplayPage));
                    break;

                case AdditionalKinds.Primary:

                case AdditionalKinds.Other:
                    NavigationService.Navigate(typeof(Views.MainPage));
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task<bool> registerBackgroundTaskForPushNotification()
        {
            var taskRegistered = false;
            var exampleTaskName = "PBA_NotificationBackgroundTask";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (taskRegistered)
            {
                //OutputText.Text = "Task already registered.";
                return true;
            }

            // Register background task
            BackgroundAccessStatus backgroundStatus = await BackgroundExecutionManager.RequestAccessAsync();

            if (backgroundStatus != BackgroundAccessStatus.Denied && backgroundStatus != BackgroundAccessStatus.Unspecified)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = exampleTaskName;
                builder.TaskEntryPoint = "PBA_BackgroundTasks.NotificationBackgroundTask";
                builder.SetTrigger(new PushNotificationTrigger());
                BackgroundTaskRegistration task = builder.Register();
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}