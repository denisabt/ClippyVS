using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Recoding.ClippyVSPackage;
using Recoding.ClippyVSPackage.Configurations.Legacy;

namespace SharedProject1.AssistImpl
{
    public class RockyGeniusBase : AssistantBase
    {
        /// <summary>
        /// The URI for the animations json definition
        /// </summary>
        //private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/animations.json";
        protected static string AnimationsResourceUri = "";

        /// <summary>
        /// The list of couples of Columns/Rows double animations , supports no overlays
        /// </summary>
        protected static LayeredAnimations _animations;

        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        protected Image _clippedImage1;

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        protected void RegisterAnimations()
        {
            var storedAnimations = Genius.ParseAnimDescriptions();
            if (storedAnimations == null) return;

            _animations = new LayeredAnimations(storedAnimations.Count);

            foreach (var animation in storedAnimations)
            {
                RegisterAnimation(animation);
            }
        }

        private static List<WeblikeSingleAnimation> ParseAnimDescriptions()
        {
            var spResUri = Genius.AnimationsResourceUri;

#if Dev19
            spResUri = spResUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
            var uri = new Uri(spResUri, UriKind.RelativeOrAbsolute);

            var animJStream = Application.GetResourceStream(uri);

            if (animJStream == null)
                return null;

            // Can go to Constructor/Init
            var errors = new List<string>();

            var storedAnimations = DeserializeAnimations(animJStream, errors);
            return storedAnimations;
        }

        private void RegisterAnimation(WeblikeSingleAnimation animation)
        {
            var xDoubleAnimation = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var yDoubleAnimation = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var visibility0 = new ObjectAnimationUsingKeyFrames();

            var xDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var yDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var visibility1 = new ObjectAnimationUsingKeyFrames();

            var xDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var yDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var visibility2 = new ObjectAnimationUsingKeyFrames();

            double timeOffset = 0;
            var frameIndex = 0;
            var animationMaxLayers = 0;

            foreach (var frame in animation.Frames)
            {
                animationMaxLayers = Genius.RegisterFrame(frame, animationMaxLayers, xDoubleAnimation, yDoubleAnimation, xDoubleAnimation1, yDoubleAnimation1, xDoubleAnimation2, yDoubleAnimation2, visibility0, visibility1, visibility2, ref timeOffset, ref frameIndex);
            }

            _animations.Add(animation.Name,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation),
                visibility0,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation1, yDoubleAnimation1),
                visibility1,
                animationMaxLayers);

            Debug.WriteLine("Added Genius Anim {0}" + animation.Name);
            Debug.WriteLine("...  Frame Count: " + xDoubleAnimation.KeyFrames.Count + " - " +
                            yDoubleAnimation.KeyFrames.Count);
            Debug.WriteLine($"Animation {animation.Name} has {animationMaxLayers} layers");

            xDoubleAnimation.Completed += XDoubleAnimation_Completed;
        }

        protected static List<WeblikeSingleAnimation> DeserializeAnimations(StreamResourceInfo animJStream, List<string> errors)
        {
            var storedAnimations =
                JsonConvert.DeserializeObject<List<WeblikeSingleAnimation>>(StreamToString(animJStream.Stream),
                    new JsonSerializerSettings
                    {
                        Error = delegate (object sender, ErrorEventArgs args)
                        {
                            errors.Add(args.ErrorContext.Error.Message);
                            args.ErrorContext.Handled = true;
                        },
                        MissingMemberHandling = MissingMemberHandling.Error,
                        NullValueHandling = NullValueHandling.Include
                    });
            return storedAnimations;
        }

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void XDoubleAnimation_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
            AssistantFramesImage.Visibility = Visibility.Visible;
            if (AssistantFramesImage.Parent is Canvas canvas)
                canvas.Visibility = Visibility.Visible;

            _clippedImage1.Visibility = Visibility.Hidden;
            if (_clippedImage1.Parent is Canvas canvas1)
                canvas1.Visibility=Visibility.Hidden;
        }
    }
}