using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Firebase.CloudMessaging;
using System;
using System.Threading.Tasks;

namespace MeteoApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
         public static MainActivity Instance { get; private set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Instance = this;
            HandleIntent(Intent);
            CreateNotificationChannelIfNeeded();

            RequestStartupPermissions();

            // ATTIVARE PER AVERE TOKEN DISPOSITIVO PER RICEVERE NOTIFICHE
            //_ = FetchFcmToken();
        }

        private void RequestStartupPermissions()
        {
            var permissions = new System.Collections.Generic.List<string>();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
                    permissions.Add(Android.Manifest.Permission.PostNotifications);
            }

            if (CheckSelfPermission(Android.Manifest.Permission.AccessFineLocation) != Permission.Granted)
                permissions.Add(Android.Manifest.Permission.AccessFineLocation);

            if (permissions.Count > 0)
                RequestPermissions(permissions.ToArray(), 0);
        }

        public async Task FetchFcmToken()
        {
            try
            {
                await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();

                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

                System.Diagnostics.Debug.WriteLine($"\n\n---> FCM Device Token: {token} <--- \n\n");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    new Android.App.AlertDialog.Builder(this)
                        .SetTitle("IL TUO TOKEN FCM È:")
                        .SetMessage(token)
                        .SetPositiveButton("OK", (sender, args) => { })
                        .Show();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore recupero token: {ex.Message}");
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        private static void HandleIntent(Intent intent)
        {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }

        private void CreateNotificationChannelIfNeeded()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                CreateNotificationChannel();
            }
        }

        private void CreateNotificationChannel()
        {
            var channelId = $"{PackageName}.general";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);
            FirebaseCloudMessagingImplementation.ChannelId = channelId;
        }
    }
}