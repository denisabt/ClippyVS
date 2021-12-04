using Microsoft.VisualStudio.Shell;
using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;
using System.Linq;
using Recoding.ClippyVSPackage;

namespace SharedProject1.AssistImpl
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Merlin : AssistantBase
    {
        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        //private static string spriteResourceUri = "pack://application:,,,/ClippyVSPackage;component/clippy.png";
        private static string spriteResourceUri = "pack://application:,,,/ClippyVs2022;component/merlin_map.png";

        /// <summary>
        /// The URI for the animations json definition
        /// </summary>
        //private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/animations.json";
        private static string animationsResourceUri = "pack://application:,,,/ClippyVs2022;component/merlin_agent.js";

        /// <summary>
        /// The with of the frame
        /// </summary>
        private static int clipWidth = 128;

        /// <summary>
        /// The height of the frame
        /// </summary>
        private static int clipHeight = 128;

        public static int ClipHeight { get => clipHeight; set => clipHeight = value; }
        public static int ClipWidth { get => clipWidth; set => clipWidth = value; }
        public List<MerlinAnimations> AllAnimations { get => allAnimations; }

        /// <summary>
        /// The list of couples of Columns/Rows double animations
        /// </summary>
        private static Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> Animations;

        /// <summary>
        /// All the animations that represents an Idle state
        /// </summary>
        public static List<MerlinAnimations> IdleAnimations = new List<MerlinAnimations>() {
            MerlinAnimations.MoveLeft,
MerlinAnimations.Congratulate,
MerlinAnimations.Hide,
MerlinAnimations.Pleased,
MerlinAnimations.Acknowledge,
MerlinAnimations.Thinking,
MerlinAnimations.Suggest,
MerlinAnimations.Explain,
MerlinAnimations.Decline,
MerlinAnimations.DontRecognize,
MerlinAnimations.Writing,
MerlinAnimations.Write,
MerlinAnimations.Idle3_2,
MerlinAnimations.Idle3_1,
MerlinAnimations.Congratulate_2,
MerlinAnimations.StartListening,
MerlinAnimations.Idle2_2,
MerlinAnimations.Announce,
MerlinAnimations.GetAttention,
MerlinAnimations.Idle2_1,
MerlinAnimations.GestureLeft,
MerlinAnimations.Surprised,
MerlinAnimations.GestureRight,
MerlinAnimations.Idle1_4,
MerlinAnimations.LookLeftReturn,
MerlinAnimations.GestureUp,
MerlinAnimations.Idle1_1,
MerlinAnimations.Idle1_3,
MerlinAnimations.Idle1_2,
MerlinAnimations.Read,
MerlinAnimations.Processing,
MerlinAnimations.Wave,
MerlinAnimations.DoMagic1,
MerlinAnimations.DoMagic2,
MerlinAnimations.LookRight,
MerlinAnimations.Alert,
MerlinAnimations.MoveRight,
MerlinAnimations.Reading,
MerlinAnimations.GetAttentionContinued,
MerlinAnimations.WriteContinued,
MerlinAnimations.Confused,
MerlinAnimations.LookRightBlink,
MerlinAnimations.Search,
MerlinAnimations.Uncertain,
MerlinAnimations.LookLeft,
MerlinAnimations.LookDownReturn,
MerlinAnimations.Hearing_4,
MerlinAnimations.LookUpReturn,
MerlinAnimations.Hearing_1,
MerlinAnimations.Greet,
MerlinAnimations.Hearing_3,
MerlinAnimations.WriteReturn,
MerlinAnimations.Hearing_2,
MerlinAnimations.GetAttentionReturn,
MerlinAnimations.RestPose,
MerlinAnimations.LookDownBlink,
MerlinAnimations.LookUpBlink,
MerlinAnimations.Think,
MerlinAnimations.Blink,
MerlinAnimations.Show,
MerlinAnimations.LookRightReturn,
MerlinAnimations.StopListening,
MerlinAnimations.MoveDown,
MerlinAnimations.ReadContinued,
MerlinAnimations.LookDown,
MerlinAnimations.Sad,
MerlinAnimations.Process,
MerlinAnimations.LookUp,
MerlinAnimations.GestureDown,
MerlinAnimations.ReadReturn,
MerlinAnimations.Searching,
MerlinAnimations.MoveUp,
MerlinAnimations.LookLeftBlink };


        /// <summary>
        /// The list of all the available animations
        /// </summary>
        private List<MerlinAnimations> allAnimations = new List<MerlinAnimations>();

        /// <summary>
        /// The time dispatcher to perform the animations in a random way
        /// </summary>
        private DispatcherTimer WPFAnimationsDispatcher;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Merlin(Canvas canvas)
        {
            var spResUri = spriteResourceUri;
#if Dev19
            spResUri = spriteResourceUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
#if Dev22
#endif
            this.Sprite = new BitmapImage(new Uri(spResUri, UriKind.RelativeOrAbsolute));

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
            allAnimations = new List<MerlinAnimations>();
            var values = Enum.GetValues(typeof(MerlinAnimations));
            allAnimations.AddRange(values.Cast<MerlinAnimations>());
            RegisterIdleRandomAnimations();
        }

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        private void RegisterAnimations()
        {
            var spResUri = animationsResourceUri;

#if Dev19
            spResUri = spResUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
            Uri uri = new Uri(spResUri, UriKind.RelativeOrAbsolute);

            StreamResourceInfo info = Application.GetResourceStream(uri);

            List<ClippySingleAnimation> storedAnimations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClippySingleAnimation>>(StreamToString(info.Stream));

            Animations = new Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>>();

            foreach (ClippySingleAnimation animation in storedAnimations)
            {
                DoubleAnimationUsingKeyFrames xDoubleAnimation = new DoubleAnimationUsingKeyFrames
                {
                    FillBehavior = FillBehavior.HoldEnd
                };

                DoubleAnimationUsingKeyFrames yDoubleAnimation = new DoubleAnimationUsingKeyFrames
                {
                    FillBehavior = FillBehavior.HoldEnd
                };

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

        public void StartAnimation(MerlinAnimations animations, bool byPassCurrentAnimation = false)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                async delegate {
                    await StartAnimationAsync(animations, byPassCurrentAnimation);
                });
        }

        /// <summary>
        /// Start a specific animation
        /// </summary>
        /// <param name="animationType"></param>
        public async System.Threading.Tasks.Task StartAnimationAsync(MerlinAnimations animationType, bool byPassCurrentAnimation = false)
        {
            if (!IsAnimating || byPassCurrentAnimation)
            {
                IsAnimating = true;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                clippedImage.BeginAnimation(Canvas.LeftProperty, Animations[animationType.ToString()].Item1);
                clippedImage.BeginAnimation(Canvas.TopProperty, Animations[animationType.ToString()].Item2);
            }

        }
    }
}
