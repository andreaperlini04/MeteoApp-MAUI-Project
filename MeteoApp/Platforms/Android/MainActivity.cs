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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleIntent(Intent);
            CreateNotificationChannelIfNeeded();

            RequestNotificationPermission();
            
            // ATTIVARE PER AVERE TOKEN DISPOSITIVO PER RICEVERE NOTIFICHE
            _ = FetchFcmToken();
        }

        private void RequestNotificationPermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
                {
                    RequestPermissions(new[] { Android.Manifest.Permission.PostNotifications }, 0);
                }
            }
        }

        private async Task FetchFcmToken()
        {
            try
            {
                // 1. Controlla che i servizi Firebase/Google Play siano validi
                await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();

                // 2. Richiede il Token univoco del dispositivo
                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();

                // Lo stampa nella console di Debug di Visual Studio / VS Code
                System.Diagnostics.Debug.WriteLine($"\n\n---> FCM Device Token: {token} <--- \n\n");

                // (Opzionale ma comodissimo) Lo mostra direttamente sullo schermo del telefono
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