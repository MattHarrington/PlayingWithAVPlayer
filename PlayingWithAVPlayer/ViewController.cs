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

            var audioSession = AVAudioSession.SharedInstance();
            audioSession.SetCategory(AVAudioSessionCategory.Playback);
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
//            base.RemoteControlReceived(theEvent);
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
                        RemoveObservers(player);
                        player.Pause();
                        player.Dispose();
                        playPauseButton.SetTitle("Play", UIControlState.Normal);
                    }
                    else
                    {
                        player = new AVPlayer(source);
                        AddObservers(player);
                        player.Play();
                        playPauseButton.SetTitle("Stop", UIControlState.Normal);
                    }
                    break;

                case UIEventSubtype.RemoteControlPause:
                case UIEventSubtype.RemoteControlStop:
                    RemoveObservers(player); // For some reason, observers must be removed before player is paused.
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
                        AddObservers(player);
                        player.Play();
                        playPauseButton.SetTitle("Stop", UIControlState.Normal);
                    }
                    break;
            }
        }

        void AddObservers(AVPlayer observedPlayer)
        {
//            return;
            observedPlayer.AddObserver(observer: this, keyPath: new NSString("error"),
                options: NSKeyValueObservingOptions.OldNew, context: IntPtr.Zero);
            observedPlayer.AddObserver(observer: this, keyPath: new NSString("status"),
                options: NSKeyValueObservingOptions.OldNew, context: IntPtr.Zero);
            observedPlayer.AddObserver(observer: this, keyPath: new NSString("rate"),
                options: NSKeyValueObservingOptions.OldNew, context: IntPtr.Zero);
        }

        void RemoveObservers(AVPlayer observedPlayer)
        {
//            return;
            observedPlayer.RemoveObserver(observer: this, keyPath: new NSString("error"),
                context: IntPtr.Zero);
            observedPlayer.RemoveObserver(observer: this, keyPath: new NSString("status"),
                context: IntPtr.Zero);
            observedPlayer.RemoveObserver(observer: this, keyPath: new NSString("rate"),
                context: IntPtr.Zero);
        }

        void OnButtonClick(object sender, EventArgs e)
        {
            if (player?.Rate > 0 && player.Error == null)
            {
                // Player is playing.  Let's stop it.
                Debug.WriteLine("OnButtonClick() - stopping player");
                player.Pause();
                RemoveObservers(player);
                player.Dispose();
                playPauseButton.SetTitle("Play", UIControlState.Normal);
            }
            else
            {
                Debug.WriteLine("OnButtonClick() starting player");
                player = new AVPlayer(source);
                AddObservers(player);
                player.Play();
                playPauseButton.SetTitle("Stop", UIControlState.Normal);
            }
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            Console.WriteLine("Old value of '{0}' = {1}; New value of '{0}' = {2}", 
                keyPath, change["old"], change["new"]);

            if (keyPath == "rate" && !(player.Rate > 0))
            {
                // User has stopped audio outside the app, 
                // perhaps by using iTunes to play something else.
                RemoveObservers(player);
                player.Dispose();
                playPauseButton.SetTitle("Play", UIControlState.Normal);
            }
        }
            
    }
}
