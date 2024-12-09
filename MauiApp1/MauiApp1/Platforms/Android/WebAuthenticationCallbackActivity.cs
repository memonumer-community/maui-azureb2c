using Android.App;
using Microsoft.Identity.Client;

namespace MauiApp1
{
    [Activity(NoHistory = true, Exported = true)]
    [IntentFilter(new[] { Android.Content.Intent.ActionView },
        Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
        DataHost = CALLBACK_HOST,
        DataScheme = CALLBACK_SCHEME)]
    public class WebAuthenticationCallbackActivity : BrowserTabActivity
    {
        const string CALLBACK_HOST= "callback";
        const string CALLBACK_SCHEME = "myapp";

    }
}