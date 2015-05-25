using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using CoreGraphics;
using AVFoundation;

[assembly: ExportRenderer(typeof(VideoDemo.StartPage), typeof(VideoDemo.iOS.Renderers.StartPageRenderer))]
namespace VideoDemo.iOS.Renderers
{
    public class StartPageRenderer : PageRenderer
    {

        PlayerViewController playerController;
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();            
            var view = NativeView;

            playerController = new PlayerViewController();
            view.AddSubview(playerController.View);

            view.ConstrainLayout(() => this.playerController.View.Top() == view.Top() &&
                                        this.playerController.View.Left() == view.Left() &&
                                        this.playerController.View.Right() == view.Right() &&
                                        this.playerController.View.Bottom() == view.Bottom());           
        }


        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            playerController.View.Frame = new CGRect(this.View.Frame.X, this.View.Frame.Y, this.View.Frame.Width, this.View.Frame.Height);

            //need wait for recalc autolayouts for player frame
            Device.StartTimer(TimeSpan.FromSeconds(0.3), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    playerController.UpdateVideoFrame();    
                });

                return false;
            });
                    
        }
       
    }
}