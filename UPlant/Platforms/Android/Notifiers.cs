using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Firebase.Messaging;
using Newtonsoft.Json;

namespace UPlant;

[Service(Name = "uplant.MyFirebaseMessagingService")]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class MyFirebaseMessagingService : FirebaseMessagingService
{
    public override async void OnMessageReceived(RemoteMessage message)
    {
        var a = await FirebaseApi.ReadDatabaseAsync("received");
        await FirebaseApi.WriteDatabaseAsync("received", a == "null" ? 1 : (int.Parse(a) + 1));

        base.OnMessageReceived(message);
        var notification = message.GetNotification();
        var data = message.Data;

        if (notification != null)
        {
            ShowNotification(notification.Title, notification.Body, data.ContainsKey("buttons") ? data["buttons"] : null);
        }
    }

    private void ShowNotification(string title, string body, string buttonsJson)
    {
        var notificationManager = NotificationManagerCompat.From(this);
        var id = (int)DateTime.UtcNow.Ticks;
        var builder = new NotificationCompat.Builder(this, "default_channel")
            .SetContentTitle(title)
            .SetContentText(body)
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
            .SetStyle(new NotificationCompat.DecoratedCustomViewStyle())
            .SetAutoCancel(true)
            .SetPriority((int)NotificationPriority.High);

        if (!string.IsNullOrEmpty(buttonsJson))
        {
            foreach (var button in JsonConvert.DeserializeObject<List<ButtonAction>>(buttonsJson))
            {
                var intent = new Intent(this, typeof(NotificationActionReceiver));
                intent.SetAction(button.Action);
                intent.PutExtra("action", button.Action);
                intent.PutExtra("notificationId", id);

                var pendingIntent = PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.UpdateCurrent);
                builder.AddAction(new NotificationCompat.Action.Builder(0, button.Title, pendingIntent).Build());
            }
        }
        notificationManager.Notify(id, builder.Build());
    }
}

[BroadcastReceiver(Name = "uplant.NotificationActionReceiver")]
public class NotificationActionReceiver : BroadcastReceiver
{
    public override async void OnReceive(Context context, Intent intent)
    {
        var action = intent.GetStringExtra("action");
        var notificationId = intent.GetIntExtra("notificationId", -1);
        if (notificationId != -1)
        {
            var notificationManager = NotificationManagerCompat.From(context);
            notificationManager.Cancel(notificationId);
        }

        if (action == "watered")
        {
            var a = await FirebaseApi.ReadDatabaseAsync("watered");
            await FirebaseApi.WriteDatabaseAsync("watered", a == "null" ? 1 : (int.Parse(a) + 1));
        }
    }
}

public class ButtonAction
{
    public string Title { get; set; }
    public string Action { get; set; }
}