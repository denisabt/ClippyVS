using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using SharedProject1.Configurations;

namespace SharedProject1.AssistImpl
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Rocky : RockyGeniusBase
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
        public List<RockyAnimations> AllAnimationNames { get; set; }

        /// <summary>
        /// All the animations that represents an Idle state
        /// </summary>
        private static readonly List<RockyAnimations> IdleAnimationNames = new List<RockyAnimations>
        {

        RockyAnimations.DeepIdle1,
        RockyAnimations.Congratulate,
        RockyAnimations.Idle_8,
        RockyAnimations.Hide,
        RockyAnimations.SendMail,
        RockyAnimations.Thinking,
        RockyAnimations.Idle_3,
        RockyAnimations.Explain,
        RockyAnimations.Idle_5,
RockyAnimations.Print,
        RockyAnimations.LookRight,
        RockyAnimations.GetAttention,
        RockyAnimations.Save,
        RockyAnimations.GetTechy,
        RockyAnimations.GestureUp,
        RockyAnimations.Idle1_1,
        RockyAnimations.Processing,
        RockyAnimations.Alert,
        RockyAnimations.LookUpRight,
        RockyAnimations.Idle_9,
        RockyAnimations.Idle_7,
        RockyAnimations.GestureDown,
        RockyAnimations.LookLeft,
        RockyAnimations.Idle_2,
        RockyAnimations.LookUpLeft,
        RockyAnimations.CheckingSomething,
        RockyAnimations.Hearing_1,
        RockyAnimations.GetWizardy,
        RockyAnimations.GestureLeft,
        RockyAnimations.Wave,
        RockyAnimations.Goodbye,
        RockyAnimations.GestureRight,
        RockyAnimations.Writing,
        RockyAnimations.LookDownRight,
        RockyAnimations.GetArtsy,
        RockyAnimations.Show,
        RockyAnimations.LookDown,
        RockyAnimations.Searching,
        RockyAnimations.Idle_4,
        RockyAnimations.EmptyTrash,
        RockyAnimations.Greeting,
        RockyAnimations.LookUp,
        RockyAnimations.Idle_6,
        RockyAnimations.RestPose,
        RockyAnimations.Idle_8,
        RockyAnimations.LookDownLeft
    };

        /// <summary>
        /// Default ctor
        /// </summary>
        public Rocky(Panel canvas)
        {
            SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/Rocky/rocky_map.png";
            AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/rocky.json";
            Animations = null;
            InitAssistant(canvas, SpriteResourceUri, "Rocky", "rocky_map.png");

            RegisterAnimationsImpl();
        }

        private void RegisterAnimationsImpl()
        {
            bool registerSuccess = false;
            if (Animations == null)
                registerSuccess = RegisterAnimations();

            if (registerSuccess)
            {
                AllAnimationNames = new List<RockyAnimations>();
                var values = Enum.GetValues(typeof(RockyAnimations));
                AllAnimationNames.AddRange(values.Cast<RockyAnimations>());
                RegisterIdleRandomAnimations();
            }
            else
            {
                MessageBox.Show(Application.Current.MainWindow, "Error when initializing Animations for \"Rocky\"" );
            }
        }

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        //private void RegisterAnimations()
        //{
        //    Animations = RegisterAnimationsImpl(AnimationsResourceUri, XDoubleAnimation_Completed, ClipWidth, ClipHeight);
        //}

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void XDoubleAnimation_Completed(object sender, EventArgs e)
        //{
        //    IsAnimating = false;
        //}

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
            var randomInt = rmd.Next(0, IdleAnimationNames.Count);

            StartAnimation(IdleAnimationNames[randomInt]);
        }

        public void StartAnimation(RockyAnimations animations, bool byPassCurrentAnimation = false)
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
        public async System.Threading.Tasks.Task StartAnimationAsync(RockyAnimations animationType, bool byPassCurrentAnimation = false)
        {
            try
            {
                if (!IsAnimating || byPassCurrentAnimation)
                {
                    var animation = Animations[animationType.ToString()];
                    if (animation == null) return;

                    Debug.WriteLine("Triggering Rocky " + animationType);
                    Debug.WriteLine("Rocky Layers: " + animation.Layer0.ToString() + animation.Layer1);
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    AssistantFramesImage.BeginAnimation(Canvas.LeftProperty, animation.Layer0.Item1);
                    AssistantFramesImage.BeginAnimation(Canvas.TopProperty, animation.Layer1.Item1);
                    IsAnimating = true;
                }
                else
                {
                    Debug.WriteLine("Rocky: Animation skipped, IsAnimating is true");
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("StartAnimAsyncException Rocky " + animationType);
            }

        }
    }
}
