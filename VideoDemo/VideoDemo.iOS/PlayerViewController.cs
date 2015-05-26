
using System;
using System.Drawing;

using Foundation;
using UIKit;
using AVFoundation;
using CoreMedia;
using CoreFoundation;
using System.Threading.Tasks;
using CoreGraphics;

namespace VideoDemo.iOS
{
	public partial class PlayerViewController : UIViewController
	{
        const int NSEC_PER_SEC = 1000000000;

        Boolean playing, scrubInFlight, seekToZeroBeforePlaying;        
        float lastScrubSliderValue,playRateToRestore;        
        NSObject timeObserver;        
        public static NSString StatusObservationContext = new NSString("AVCustomEditPlayerViewControllerStatusObservationContext");
        public static NSString RateObservationContext = new NSString("AVCustomEditPlayerViewControllerRateObservationContext");

        double playerItemDuration
        {
            get
            {
                if (_player == null)
                    return Double.PositiveInfinity;

                CMTime itemDuration = CMTime.Invalid;
                AVPlayerItem playerItem = _player.CurrentItem;

                if (_player.Status == AVPlayerStatus.ReadyToPlay)
                {
                    itemDuration = playerItem.Duration;
                }

                return itemDuration.Seconds;
            }
        }

        AVPlayer _player;
        AVPlayerLayer _playerLayer;
        AVAsset _asset;
        AVPlayerItem _playerItem;

		public PlayerViewController () : base ("PlayerViewController", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            _asset = AVAsset.FromUrl(NSUrl.FromFilename("SampleVideo.mp4"));
            _playerItem = new AVPlayerItem(_asset);

            _playerItem.SeekingWaitsForVideoCompositionRendering = true;
            _playerItem.AddObserver(this, "status", NSKeyValueObservingOptions.New | NSKeyValueObservingOptions.Initial, StatusObservationContext.Handle);
            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, (notification) =>
            {
                Console.WriteLine("Seek Zero = true");
                seekToZeroBeforePlaying = true;
            }, _playerItem);                            

            _player = new AVPlayer(_playerItem);
            _playerLayer = AVPlayerLayer.FromPlayer(_player);
            _playerLayer.Frame = this.playerView.Frame;
            this.playerView.Layer.AddSublayer(_playerLayer);

            updateScrubber();
            updateTimeLabel();

            slider.EditingDidBegin += slider_EditingDidBegin;
            slider.EditingDidEnd += slider_EditingDidEnd;
            slider.ValueChanged += slider_ValueChanged;            
            playpauseButton.TouchUpInside += playpauseButton_TouchUpInside;

            playing = true;
            _player.Play();
		}
       
        public void UpdateVideoFrame()
        {
            _playerLayer.Frame = new CGRect(this.playerView.Frame.X, this.playerView.Frame.Y, this.playerView.Frame.Width, this.playerView.Frame.Height);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

           
            seekToZeroBeforePlaying = false;
                
            _player.AddObserver(this, (NSString)"rate", NSKeyValueObservingOptions.Old | NSKeyValueObservingOptions.New, RateObservationContext.Handle);

            addTimeObserverToPlayer();
        }

        void addTimeObserverToPlayer()
        {
            if (timeObserver != null)
            {
                return;
            }

            if (_player == null)
                return;

            if (_player.CurrentItem == null)
                return;

            if (_player.CurrentItem.Status != AVPlayerItemStatus.ReadyToPlay)
                return;

            double duration = playerItemDuration;

            if (!Double.IsInfinity(duration))
            {
                float width = (float)slider.Bounds.Width;
                double interval = 0.5 * duration / width;

                if (interval > 1.0)
                    interval = 1.0;
                timeObserver = _player.AddPeriodicTimeObserver(CMTime.FromSeconds(interval, NSEC_PER_SEC), DispatchQueue.MainQueue, delegate
                {
                    updateScrubber();
                    updateTimeLabel();
                });
            }

        }

        void removeTimeObserverFromPlayer()
        {
            if (timeObserver != null)
            {
                _player.RemoveTimeObserver(timeObserver);
                timeObserver = null;
            }
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            var ch = new NSObservedChange(change);
            if (context == RateObservationContext.Handle)
            {
                //TODO: need debug here.
                float newRate = ((NSNumber)ch.NewValue).FloatValue;
                NSNumber oldRateNum = (NSNumber)ch.OldValue;
                if (oldRateNum != null && newRate != oldRateNum.FloatValue)
                {
                    playing = (newRate != 0.0f || playRateToRestore != 0.0f);
                    updatePlayPauseButton();
                    updateScrubber();
                    updateTimeLabel();
                }
            }
            else if (context == StatusObservationContext.Handle)
            {
                AVPlayerItem playerItem = ofObject as AVPlayerItem;
                if (playerItem.Status == AVPlayerItemStatus.ReadyToPlay)
                {
                    /* Once the AVPlayerItem becomes ready to play, i.e.
                       [playerItem status] == AVPlayerItemStatusReadyToPlay,
                       its duration can be fetched from the item. */
                    addTimeObserverToPlayer();
                }
                else if (playerItem.Status == AVPlayerItemStatus.Failed)
                {
                    reportError(playerItem.Error);
                }
            }
            else
            {
                base.ObserveValue(keyPath, ofObject, change, context);
            }
        }

        private void updatePlayPauseButton()
        {
            if (playing)
                playImg.Image = UIImage.FromBundle("pause.png");
            else
                playImg.Image = UIImage.FromBundle("play.png");           
        }

        private void playpauseButton_TouchUpInside(object sender, EventArgs e)
        {            
            togglePlayPause();
            updatePlayPauseButton();
        }

        void updateTimeLabel()
        {
            if (_player == null)
                return;

            var seconds = _player.CurrentTime.Seconds;
            Console.WriteLine(seconds);
            if (double.IsInfinity(seconds))
                seconds = 0;

            int secondsInt = (int)Math.Round(seconds);
            int minutes = secondsInt / 60;
            secondsInt -= minutes * 60;

            int durSec = (int)Math.Round(playerItemDuration);
            int durMinutes = durSec / 60;
            durSec -= durMinutes * 60;

            lbl.TextColor = UIColor.White;
            lbl.TextAlignment = UITextAlignment.Center;
            lbl.Text = string.Format("{0}:{1}/{2}:{3}", minutes, secondsInt, durMinutes, durSec);

        }

        void updateScrubber()
        {
            double duration = playerItemDuration;
            if (!double.IsInfinity(duration))
            {
                double time = _player.CurrentTime.Seconds;
                slider.Value = (float)(time / duration);
            }
            else
            {
                slider.Value = 0.0f;
            }
        }

        void reportError(NSError error)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if (error == null)
                    return;

                new UIAlertView(error.LocalizedDescription, error.DebugDescription, null, "OK", null).Show();
            });
        }

        private void togglePlayPause()
        {
            playing = !playing;
            if (playing)
            {
                if (seekToZeroBeforePlaying)
                {
                    _player.Seek(CMTime.Zero);
                    seekToZeroBeforePlaying = false;                   
                }
                _player.Play();
            }
            else
            {
                _player.Pause();
            }
        }

        void slider_EditingDidBegin(object sender, EventArgs e)
        {
            seekToZeroBeforePlaying = false;
            playRateToRestore = _player.Rate;
            _player.Rate = 0f;

            removeTimeObserverFromPlayer();
        }

        private async void slider_ValueChanged(object sender, EventArgs e)
        {
            lastScrubSliderValue = slider.Value;

            if (!scrubInFlight)
                await scrubTo(lastScrubSliderValue);
        }

        private async void slider_EditingDidEnd(object sender, EventArgs e)
        {
            if (scrubInFlight)
                await scrubTo(lastScrubSliderValue);
            addTimeObserverToPlayer();

            _player.Rate = playRateToRestore;
            playRateToRestore = 0f;
        }

        private async Task scrubTo(float sliderValue)
        {
            var duration = playerItemDuration;

            if (Double.IsInfinity(duration))
                return;

            var width = slider.Bounds.Width;

            var time = duration * sliderValue;
            var tolerance = 1f * duration / width;

            scrubInFlight = true;

            await _player.SeekAsync(CMTime.FromSeconds(time, NSEC_PER_SEC), CMTime.FromSeconds(tolerance, NSEC_PER_SEC), CMTime.FromSeconds(tolerance, NSEC_PER_SEC));

            scrubInFlight = false;
            updateTimeLabel();
        }
	}
}

