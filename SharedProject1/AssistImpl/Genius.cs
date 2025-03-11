using Microsoft.VisualStudio.Shell;
using SharedProject1.Configurations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SharedProject1.AssistImpl
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Genius : RockyGeniusBase
    { 
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
            AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/Genius.json";
            SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/Genius/genius_map.png";

            if (canvas == null) return;

            InitAssistant(canvas, SpriteResourceUri);
            // Might not be required XXX
            AssistantFramesImage.Visibility = Visibility.Visible;

            ClippedImage1 = new Image
            {
                Source = Sprite,
                Stretch = Stretch.None,
                Visibility = Visibility.Collapsed
            };
    
            //canvas.Children.Clear();
            //canvas.Children.Add(ClippedImage);

            canvas1.Children.Clear();
            canvas1.Children.Add(ClippedImage1);

            if (Animations == null)
                RegisterAnimations();

            AllAnimations = new List<GeniusAnimations>();
            var values = Enum.GetValues(typeof(GeniusAnimations));
            AllAnimations.AddRange(values.Cast<GeniusAnimations>());
            RegisterIdleRandomAnimations();
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
                    var animation = Animations[animationType.ToString()];
                    if (animation == null) return;

                    Debug.WriteLine("Triggering Genius " + animationType);
                    Debug.WriteLine(animation.Layer0.Item1.ToString() + animation.Layer0.Item2);

                    var animLayers = animation.MaxLayers;
                    IsAnimating = true;

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    // well have to skip this (leave collapsed) if only one layer
                    if (animLayers > 1) {
                        ClippedImage1.Visibility = Visibility.Visible;
                        ((Canvas) ClippedImage1.Parent).Visibility = Visibility.Visible;
                    }
                    AssistantFramesImage.BeginAnimation(Canvas.LeftProperty, animation.Layer0.Item1);
                    AssistantFramesImage.BeginAnimation(Canvas.TopProperty, animation.Layer0.Item2);

                    ClippedImage1.BeginAnimation(Canvas.LeftProperty, animation.Layer1.Item1);
                    ClippedImage1.BeginAnimation(Canvas.TopProperty, animation.Layer1.Item2);
                    ClippedImage1.BeginAnimation(UIElement.OpacityProperty, animation.Visibility1);
                }
                else
                {
                    Debug.WriteLine("Genius: Animation skipped, IsAnimating is true");
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("StartAnimAsyncException Genius " + animationType);
            }
        }
    }
}
