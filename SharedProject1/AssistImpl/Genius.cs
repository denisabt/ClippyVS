using Microsoft.VisualStudio.Shell;
using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Linq;
using Recoding.ClippyVSPackage;
using System.Diagnostics;
using System.Windows.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Recoding.ClippyVSPackage.Configurations.Legacy;
using Frame = Recoding.ClippyVSPackage.Configurations.Legacy.Frame;
using SharedProject1.Configurations;

namespace SharedProject1.AssistImpl
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Genius : RockyGeniusBase
    {
        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        //private static string spriteResourceUri = "pack://application:,,,/ClippyVSPackage;component/clippy.png";
        private static readonly string SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/genius_map.png";

        /// <summary>
        /// The URI for the animations json definition
        /// </summary>
        //private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/animations.json";
        private static readonly string AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/Genius.json";

        /// <summary>
        /// The height of the frame
        /// </summary>
        public static int ClipHeight { get; } = 93;

        /// <summary>
        /// The with of the frame
        /// </summary>
        public static int ClipWidth { get; } = 124;

        /// <summary>
        /// The list of all the available animations
        /// </summary>
        public List<GeniusAnimations> AllAnimations { get; } = new List<GeniusAnimations>();

        /// <summary>
        /// The list of couples of Columns/Rows double animations , supports no overlays
        /// </summary>
        private static LayeredAnimations _animations;

        /// <summary>
        /// All the animations that represents an Idle state
        /// </summary>
        private static readonly List<GeniusAnimations> IdleAnimations = new List<GeniusAnimations>() {
GeniusAnimations.Idle0,
GeniusAnimations.Idle1,
GeniusAnimations.Idle2,
GeniusAnimations.Idle3,
GeniusAnimations.Idle4,
GeniusAnimations.Idle5,
GeniusAnimations.Idle6,
GeniusAnimations.Idle7,
GeniusAnimations.Idle8,
GeniusAnimations.Idle9};


        /// <summary>
        /// Default ctor
        /// </summary>
        public Genius(Panel canvas, Panel canvas1)
        {
            if (canvas == null) return;

            InitAssistant(canvas, SpriteResourceUri);
            // Might not be required XXX
            AssistantFramesImage.Visibility = Visibility.Visible;

            _clippedImage1 = new Image
            {
                Source = Sprite,
                Stretch = Stretch.None,
                Visibility = Visibility.Collapsed
            };
    
            //canvas.Children.Clear();
            //canvas.Children.Add(ClippedImage);

            canvas1.Children.Clear();
            canvas1.Children.Add(_clippedImage1);

            if (_animations == null)
                RegisterAnimations();

            AllAnimations = new List<GeniusAnimations>();
            var values = Enum.GetValues(typeof(GeniusAnimations));
            AllAnimations.AddRange(values.Cast<GeniusAnimations>());
            RegisterIdleRandomAnimations();
        }


        public static int RegisterFrame(Frame frame, int animationMaxLayers, DoubleAnimationUsingKeyFrames xDoubleAnimation,
            DoubleAnimationUsingKeyFrames yDoubleAnimation, DoubleAnimationUsingKeyFrames xDoubleAnimation1,
            DoubleAnimationUsingKeyFrames yDoubleAnimation1, DoubleAnimationUsingKeyFrames xDoubleAnimation2,
            DoubleAnimationUsingKeyFrames yDoubleAnimation2, ObjectAnimationUsingKeyFrames visibility0, ObjectAnimationUsingKeyFrames visibility1, ObjectAnimationUsingKeyFrames visibility2, ref double timeOffset, ref int frameIndex)
        {
            if (frame.ImagesOffsets != null)
            {
                if (frame.ImagesOffsets.Count > animationMaxLayers)
                {
                    animationMaxLayers = frame.ImagesOffsets.Count;
                }

                if (frame.branching != null && frame.branching.branches != null)
                {
                    Debug.WriteLine("Has Branching Info");
                }

                for (var layerNum = 0; layerNum < frame.ImagesOffsets.Count; layerNum++)
                {
                    Debug.WriteLine("Processing Overlay " + layerNum);

                    // Prepare Key frame for all potential layers (max 3)
                    xDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    yDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    visibility0.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
                    xDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    yDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    visibility1.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
                    xDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    yDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    visibility2.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));

                    //Overlay is actually - layers - displayed at the same time...
                    var lastCol = frame.ImagesOffsets[layerNum][0];
                    var lastRow = frame.ImagesOffsets[layerNum][1];

                    // X and Y
                    var frameKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset));
                    var xKeyFrame = new DiscreteDoubleKeyFrame(lastCol * -1,
                        frameKeyTime);
                    var yKeyFrame = new DiscreteDoubleKeyFrame(lastRow * -1,
                        frameKeyTime);

                    switch (layerNum)
                    {
                        case 0:
                            xDoubleAnimation.KeyFrames.Insert(frameIndex, xKeyFrame);
                            yDoubleAnimation.KeyFrames.Insert(frameIndex, yKeyFrame);
                            visibility0.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
                            break;
                        case 1:
                            xDoubleAnimation1.KeyFrames.Insert(frameIndex, xKeyFrame);
                            yDoubleAnimation1.KeyFrames.Insert(frameIndex, yKeyFrame);
                            visibility1.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
                            break;
                        case 2:
                            xDoubleAnimation2.KeyFrames.Insert(frameIndex, xKeyFrame);
                            yDoubleAnimation2.KeyFrames.Insert(frameIndex, yKeyFrame);
                            visibility2.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
                            break;
                    }
                }

                //timeOffset += ((double)frame.Duration / 1000 * 4);
                timeOffset += ((double) frame.Duration / 1000);
                frameIndex++;
            }
            else
            {
                Debug.WriteLine("ImageOffsets was null");
            }

            return animationMaxLayers;
        }


        /// <summary>
        /// Registers a function to perform a subset of animations randomly (the idle ones)
        /// </summary>
        private void RegisterIdleRandomAnimations()
        {
            WpfAnimationsDispatcher = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(IdleAnimationTimeout)
            };
            WpfAnimationsDispatcher.Tick += WPFAnimationsDispatcher_Tick;

            WpfAnimationsDispatcher.Start();
        }

        private void WPFAnimationsDispatcher_Tick(object sender, EventArgs e)
        {
            var rmd = new Random();
            var randomInt = rmd.Next(0, IdleAnimations.Count);

            StartAnimation(IdleAnimations[randomInt]);
        }

        public void StartAnimation(GeniusAnimations animations, bool byPassCurrentAnimation = false)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                async delegate
                {
                    await StartAnimationAsync(animations, byPassCurrentAnimation);
                });
        }

        /// <summary>
        /// Start a specific animation
        /// </summary>
        /// <param name="animationType"></param>
        /// <param name="byPassCurrentAnimation">   </param>
        public async System.Threading.Tasks.Task StartAnimationAsync(GeniusAnimations animationType, bool byPassCurrentAnimation = false)
        {
            try
            {
                if (!IsAnimating || byPassCurrentAnimation)
                {
                    var animation = _animations[animationType.ToString()];
                    if (animation == null) return;

                    Debug.WriteLine("Triggering Genius " + animationType);
                    Debug.WriteLine(animation.Layer0.Item1.ToString() + animation.Layer0.Item2);

                    var animLayers = animation.MaxLayers;
                    IsAnimating = true;

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    // well have to skip this (leave collapsed) if only one layer
                    if (animLayers > 1) {
                        _clippedImage1.Visibility = Visibility.Visible;
                        ((Canvas) _clippedImage1.Parent).Visibility = Visibility.Visible;
                    }
                    AssistantFramesImage.BeginAnimation(Canvas.LeftProperty, animation.Layer0.Item1);
                    AssistantFramesImage.BeginAnimation(Canvas.TopProperty, animation.Layer0.Item2);

                    _clippedImage1.BeginAnimation(Canvas.LeftProperty, animation.Layer1.Item1);
                    _clippedImage1.BeginAnimation(Canvas.TopProperty, animation.Layer1.Item2);
                    _clippedImage1.BeginAnimation(UIElement.OpacityProperty, animation.Visibility1);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("StartAnimAsyncException Genius " + animationType);
            }
        }
    }
}
