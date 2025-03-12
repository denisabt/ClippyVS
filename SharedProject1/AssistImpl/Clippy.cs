using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Linq;
using System.Diagnostics;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// The core object that represents Clippy and its animationses
    /// </summary>
    public class Clippy : AssistantBase
    {
        /// <summary>
        /// The height of the frame
        /// </summary>
        public static int ClipHeight => 93;

        /// <summary>
        /// The with of the frame
        /// </summary>
        public static int ClipWidth => 124;

        /// <summary>
        /// The list of all the available animationses
        /// </summary>
        public List<ClippyAnimations> AllAnimations { get; } = new List<ClippyAnimations>();

        /// <summary>
        /// The list of couples of Columns/Rows double animationses
        /// </summary>
        private static Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> _animations;

        /// <summary>
        /// All the animationses that represents an Idle state
        /// </summary>
        private static readonly List<ClippyAnimations> IdleAnimations = new List<ClippyAnimations>() {
            ClippyAnimations.Idle11,
            ClippyAnimations.IdleRopePile,
            ClippyAnimations.IdleAtom,
            ClippyAnimations.IdleEyeBrowRaise,
            ClippyAnimations.IdleFingerTap,
            ClippyAnimations.IdleHeadScratch,
            ClippyAnimations.IdleSideToSide,
            ClippyAnimations.IdleSnooze };

        /// <summary>
        /// Default ctor
        /// </summary>
        public Clippy(Canvas canvas)
        {
            SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/clippy.png";
            AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/animations.json"; 
            InitAssistant(canvas, SpriteResourceUri, "Clippy", "clippy_map.png");

            if (_animations == null)
                RegisterAnimations();

            AllAnimations = new List<ClippyAnimations>();
            var values = Enum.GetValues(typeof(ClippyAnimations));
            AllAnimations.AddRange(values.Cast<ClippyAnimations>());
            RegisterIdleRandomAnimations();
        }

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        private void RegisterAnimations()
        {
            _animations = RegisterAnimationsImpl(AnimationsResourceUri, XDoubleAnimation_Completed, ClipWidth, ClipHeight, "Clippy", "clippy.json");
        }

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void XDoubleAnimation_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
        }

        /// <summary>
        /// Registers a function to perform a subset of animationses randomly (the idle ones)
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

        public void StartAnimation(ClippyAnimations animationses, bool byPassCurrentAnimation = false)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(
                async delegate
                {
                    await StartAnimationAsync(animationses, byPassCurrentAnimation);
                });
        }

        /// <summary>
        /// Start a specific animation
        /// </summary>
        /// <param name="animationsType"></param>
        /// <param name="byPassCurrentAnimation"></param>
        public async Task StartAnimationAsync(ClippyAnimations animationsType, bool byPassCurrentAnimation = false)
        {
            if (!IsAnimating || byPassCurrentAnimation)
            {
                IsAnimating = true;
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (_animations.ContainsKey(animationsType.ToString()))
                {
                    AssistantFramesImage.BeginAnimation(Canvas.LeftProperty, _animations[animationsType.ToString()].Item1);
                    AssistantFramesImage.BeginAnimation(Canvas.TopProperty, _animations[animationsType.ToString()].Item2);
                } else
                {
                    Debug.WriteLine("Animation {0} not found!", animationsType.ToString());
                }
            }

        }
    }
}
