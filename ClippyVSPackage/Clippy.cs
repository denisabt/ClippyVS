﻿
using Microsoft.VisualStudio.Shell;
using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;
using System.Linq;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Clippy
    {
        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        private static string spriteResourceUri = "pack://application:,,,/ClippyVSPackage;component/resources/clippy.png";

        /// <summary>
        /// The URI for the animations json definition
        /// </summary>
        private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/resources/animations.json";

        /// <summary>
        /// The sprite with all the animation stages for Clippy
        /// </summary>
        private BitmapSource Sprite;

        /// <summary>
        /// The actual Clippy container that works as a clipping mask
        /// </summary>
        public Canvas ClippyCanvas { get; private set; }

        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        private Image clippedImage;

        /// <summary>
        /// The with of the frame
        /// </summary>
        private static int clipWidth = 124;

        /// <summary>
        /// The height of the frame
        /// </summary>
        private static int clipHeight = 93;

        /// <summary>
        /// Seconds between a random idle animation and another
        /// </summary>
        private const int IdleAnimationTimeout = 45;

        /// <summary>
        /// When is true it means an animation is actually running
        /// </summary>
        public bool IsAnimating { get; set; }
        public static int ClipHeight { get => ClipHeight; set => ClipHeight = value; }
        public static int ClipWidth { get => ClipWidth; set => ClipWidth = value; }
        public List<ClippyAnimations> AllAnimations { get => allAnimations; }

        /// <summary>
        /// The list of couples of Columns/Rows double animations
        /// </summary>
        private static Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> Animations;

        /// <summary>
        /// All the animations that represents an Idle state
        /// </summary>
        public static List<ClippyAnimations> IdleAnimations = new List<ClippyAnimations>() {
            ClippyAnimations.Idle1_1,
            ClippyAnimations.IdleRopePile,
            ClippyAnimations.IdleAtom,
            ClippyAnimations.IdleEyeBrowRaise,
            ClippyAnimations.IdleFingerTap,
            ClippyAnimations.IdleHeadScratch,
            ClippyAnimations.IdleSideToSide,
            ClippyAnimations.IdleSnooze };

        /// <summary>
        /// The list of all the available animations
        /// </summary>
        private List<ClippyAnimations> allAnimations = new List<ClippyAnimations>();

        /// <summary>
        /// The time dispatcher to perform the animations in a random way
        /// </summary>
        private DispatcherTimer WPFAnimationsDispatcher;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Clippy(Canvas canvas)
        {
            this.Sprite = new BitmapImage(new Uri(spriteResourceUri, UriKind.RelativeOrAbsolute));

            clippedImage = new Image
            {
                Source = Sprite,
                Stretch = Stretch.None
            };

            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Children.Add(clippedImage);

            if (Animations == null)
                RegisterAnimations();


            //XX Requires testing..
            allAnimations = new List<ClippyAnimations>();
            var values = Enum.GetValues(typeof(ClippyAnimations));
            allAnimations.AddRange(values.Cast<ClippyAnimations>());
            RegisterIdleRandomAnimations();
        }

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        private void RegisterAnimations()
        {
            Uri uri = new Uri(animationsResourceUri, UriKind.RelativeOrAbsolute);
            StreamResourceInfo info = Application.GetResourceStream(uri);

            List<ClippyAnimation> storedAnimations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClippyAnimation>>(StreamToString(info.Stream));

            Animations = new Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>>();

            foreach (ClippyAnimation animation in storedAnimations)
            {
                DoubleAnimationUsingKeyFrames xDoubleAnimation = new DoubleAnimationUsingKeyFrames();
                xDoubleAnimation.FillBehavior = FillBehavior.HoldEnd;

                DoubleAnimationUsingKeyFrames yDoubleAnimation = new DoubleAnimationUsingKeyFrames();
                yDoubleAnimation.FillBehavior = FillBehavior.HoldEnd;

                int lastCol = 0;
                int lastRow = 0;
                double timeOffset = 0;

                foreach (Recoding.ClippyVSPackage.Configurations.Frame frame in animation.Frames)
                {
                    if (frame.ImagesOffsets != null)
                    {
                        lastCol = frame.ImagesOffsets.Column;
                        lastRow = frame.ImagesOffsets.Row;
                    }

                    // X
                    DiscreteDoubleKeyFrame xKeyFrame = new DiscreteDoubleKeyFrame(ClipWidth * -lastCol, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                    // Y
                    DiscreteDoubleKeyFrame yKeyFrame = new DiscreteDoubleKeyFrame(ClipHeight * -lastRow, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                    timeOffset += ((double)frame.Duration / 1000);

                    xDoubleAnimation.KeyFrames.Add(xKeyFrame);
                    yDoubleAnimation.KeyFrames.Add(yKeyFrame);
                }

                Animations.Add(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation));

                xDoubleAnimation.Completed += xDoubleAnimation_Completed;
            }
        }

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void xDoubleAnimation_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
        }

        /// <summary>
        /// Registers a function to perform a subset of animations randomly (the idle ones)
        /// </summary>
        private void RegisterIdleRandomAnimations()
        {
            WPFAnimationsDispatcher = new DispatcherTimer();
            WPFAnimationsDispatcher.Interval = TimeSpan.FromSeconds(IdleAnimationTimeout);
            WPFAnimationsDispatcher.Tick += WPFAnimationsDispatcher_Tick;

            WPFAnimationsDispatcher.Start();
        }

        void WPFAnimationsDispatcher_Tick(object sender, EventArgs e)
        {
            Random rmd = new Random();
            int random_int = rmd.Next(0, IdleAnimations.Count);

            StartAnimation(IdleAnimations[random_int]);
        }

        public void StartAnimation(ClippyAnimations animations, bool byPassCurrentAnimation = false)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                async delegate {
                    await StartAnimationAsync(ClippyAnimations.Idle1_1,byPassCurrentAnimation);
                });
        }

        /// <summary>
        /// Start a specific animation
        /// </summary>
        /// <param name="animationType"></param>
        public async System.Threading.Tasks.Task StartAnimationAsync(ClippyAnimations animationType, bool byPassCurrentAnimation = false)
        {
            if (!IsAnimating || byPassCurrentAnimation)
            {
                IsAnimating = true;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                clippedImage.BeginAnimation(Canvas.LeftProperty, Animations[animationType.ToString()].Item1);
                clippedImage.BeginAnimation(Canvas.TopProperty, Animations[animationType.ToString()].Item2);
            }
            
        }

        

        /// <summary>
        /// Reads the content of a stream into a string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
