// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace PlayingWithAVPlayer
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnError { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton btnStatus { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton playPauseButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (btnError != null) {
				btnError.Dispose ();
				btnError = null;
			}
			if (btnStatus != null) {
				btnStatus.Dispose ();
				btnStatus = null;
			}
			if (playPauseButton != null) {
				playPauseButton.Dispose ();
				playPauseButton = null;
			}
		}
	}
}
