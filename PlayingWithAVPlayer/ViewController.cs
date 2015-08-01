using System;

using UIKit;
using AVFoundation;
using Foundation;
using MediaPlayer;
using System.Diagnostics;

namespace PlayingWithAVPlayer
{
    public partial class ViewController : UIViewController
    {
        AVPlayer player;
        readonly NSUrl source = NSUrl.FromString("http://live2.artoflogic.com:8190/kvmr");

        public ViewController(IntPtr handle)
            : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Must call certain methods in Reachability before ReachabilityChanged
            // will work. See https://forums.xamarin.com/discussion/15291/reachability-changed-event-never-fires
            NetworkStatus networkStatus = Reachability.RemoteHostStatus();
            Reachability.ReachabilityChanged += ReachabilityChanged;

            AVAudioSession audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Playback);
            audioSession.BeginInterruption += AudioSession_BeginInterruption;
            audioSession.EndInterruption += AudioSession_EndInterruption;
            NSNotificationCenter notificationCenter = NSNotificationCenter.DefaultCenter;
            notificationCenter.AddObserver(this, new ObjCRuntime.Selector("routeChanged:"), 
                AVAudioSession.RouteChangeNotification, null);
            audioSession.SetActive(true);

            playPauseButton.TouchUpInside += OnButtonClick;
            btnStatus.TouchUpInside += (sender, e) => 
                Debug.WriteLine("player.Status = {0} and player.Rate = {1}", player?.Status, player?.Rate);
            btnError.TouchUpInside += (sender, e) => Debug.WriteLine("player.Error = {0}", player?.Error);

            MPNowPlayingInfo nowPlayingInfo = new MPNowPlayingInfo();
            nowPlayingInfo.AlbumTitle = "California";
            nowPlayingInfo.Artist = "Nevada City";
            nowPlayingInfo.Title = "KVMR";
            MPNowPlayingInfoCenter.DefaultCenter.NowPlaying = nowPlayingInfo;

            // This is not working...
            MPRemoteCommandCenter rcc = MPRemoteCommandCenter.Shared;
            rcc.SeekBackwardCommand.Enabled = false;
            rcc.SeekForwardCommand.Enabled = false;
            rcc.NextTrackCommand.Enabled = false;
            rcc.PreviousTrackCommand.Enabled = false;
            rcc.SkipBackwardCommand.Enabled = false;
            rcc.SkipForwardCommand.Enabled = false;

            // You must enable a command so that others can be disabled?
            // See http://stackoverflow.com/a/28925369.
            rcc.PlayCommand.Enabled = true;
        }

        void ReachabilityChanged (object sender, EventArgs e)
        {
            Debug.WriteLine("Reachability changed");
            NetworkStatus ics = Reachability.InternetConnectionStatus();
            switch (ics)
            {
                case NetworkStatus.NotReachable:
                    Debug.WriteLine("Network changed - NotReachable");
                    break;
                case NetworkStatus.ReachableViaCarrierDataNetwork:
                case NetworkStatus.ReachableViaWiFiNetwork:
                    Debug.WriteLine("Network changed - Reachable");
                    break;
            }
        }

        void AudioSession_BeginInterruption(object sender, EventArgs e)
        {
            Console.WriteLine("Begin interruption");
            player.Dispose();
            playPauseButton.SetTitle("Play", UIControlState.Normal);
        }

        void AudioSession_EndInterruption(object sender, EventArgs e)
        {
            Console.WriteLine("End interruption");
        }

        [Export("routeChanged:")]
        public void RouteChanged(NSNotification notification)
        {
            var reason = notification.UserInfo.ValueForKey(new NSString("AVAudioSessionRouteChangeReasonKey"));
            if (reason.Description == "2") // Headphones were unplugged
            {
                if (player != null)  // player will be null if user has not hit play
                {
                    player.Dispose(); 
                    InvokeOnMainThread(() => playPauseButton.SetTitle("Play", UIControlState.Normal));
                }
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            UIApplication.SharedApplication.BeginReceivingRemoteControlEvents();
            this.BecomeFirstResponder();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            UIApplication.SharedApplication.EndReceivingRemoteControlEvents();
            this.ResignFirstResponder();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void RemoteControlReceived(UIEvent theEvent)
        {
            if (theEvent.Type != UIEventType.RemoteControl)
            {
                return;
            }

            Debug.WriteLine("Received remote control event: " + theEvent.Subtype);

            switch (theEvent.Subtype)
            {
                case UIEventSubtype.RemoteControlTogglePlayPause:
                    if (player?.Rate > 0 && player.Error == null) // player is playing
                    {
                        player.Pause();
                        player.Dispose();
                        playPauseButton.SetTitle("Play", UIControlState.Normal);
                    }
                    else
                    {
                        player = new AVPlayer(source);
                        player.Play();
                        playPauseButton.SetTitle("Stop", UIControlState.Normal);
                    }
                    break;

                case UIEventSubtype.RemoteControlPause:
                case UIEventSubtype.RemoteControlStop:
                    player.Pause();
                    player.Dispose();
                    playPauseButton.SetTitle("Play", UIControlState.Normal);
                    break;

                case UIEventSubtype.RemoteControlPlay:
                case UIEventSubtype.RemoteControlPreviousTrack:
                case UIEventSubtype.RemoteControlNextTrack:
                    // only handle these cases if player not already playing
                    if (!(player?.Rate > 0) && player.Error == null)
                    {
                        player = new AVPlayer(source);
                        player.Play();
                        playPauseButton.SetTitle("Stop", UIControlState.Normal);
                    }
                    break;
            }
        }

        void OnButtonClick(object sender, EventArgs e)
        {
            if (!Reachability.IsHostReachable("live2.artoflogic.com"))
            {
                Debug.WriteLine("Host unreachable");
                return;
            }
            if (player?.Rate > 0 && player.Error == null)
            {
                // Player is playing.  Let's stop it.
                Debug.WriteLine("OnButtonClick() - stopping player");
                player.Pause();
                player.Dispose();
                playPauseButton.SetTitle("Play", UIControlState.Normal);
            }
            else
            {
                Debug.WriteLine("OnButtonClick() - starting player");
                player = new AVPlayer(source);
                player.Play();
                playPauseButton.SetTitle("Stop", UIControlState.Normal);
            }
        }
            
    }
}
