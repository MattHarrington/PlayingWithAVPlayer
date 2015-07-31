using UIKit;

namespace PlayingWithAVPlayer
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            Xamarin.Insights.Initialize("6177fa9abb40234e5363a7143c5ee7b9cf7a70a9");
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}
