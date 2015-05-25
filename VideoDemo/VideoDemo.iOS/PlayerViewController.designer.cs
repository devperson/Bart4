// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace VideoDemo.iOS
{
	[Register ("PlayerViewController")]
	partial class PlayerViewController
	{
		[Outlet]
		UIKit.UILabel lbl { get; set; }

		[Outlet]
		UIKit.UIView playerView { get; set; }

		[Outlet]
		UIKit.UIImageView playImg { get; set; }

		[Outlet]
		UIKit.UIButton playpauseButton { get; set; }

		[Outlet]
		UIKit.UISlider slider { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lbl != null) {
				lbl.Dispose ();
				lbl = null;
			}

			if (playerView != null) {
				playerView.Dispose ();
				playerView = null;
			}

			if (playpauseButton != null) {
				playpauseButton.Dispose ();
				playpauseButton = null;
			}

			if (slider != null) {
				slider.Dispose ();
				slider = null;
			}

			if (playImg != null) {
				playImg.Dispose ();
				playImg = null;
			}
		}
	}
}
