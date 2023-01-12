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
using Frame = Recoding.ClippyVSPackage.Configurations.Legacy.Frame;

namespace SharedProject1.AssistImpl
{
    public class RockyGeniusBase : AssistantBase
    {
        /// <summary>
        /// The list of couples of Columns/Rows double animations , supports no overlays
        /// </summary>
        protected static LayeredAnimations Animations;

        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        protected Image ClippedImage1;

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        protected bool RegisterAnimations()
        {
            var storedAnimations = ParseAnimDescriptions();
            if (storedAnimations == null) return true;

            Animations = new LayeredAnimations(storedAnimations.Count);

            bool errorOccured = false;
            foreach (var animation in storedAnimations)
            {

                try
                {
                    RegisterAnimation(animation);

                }
                catch (Exception)
                {
                    errorOccured = true;

                }
            }

            return !errorOccured;
        }

        private static List<WeblikeSingleAnimation> ParseAnimDescriptions()
        {
            var spResUri = AnimationsResourceUri;

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
            if (animation.Frames == null) 
                return;

            var xDoubleAnimation = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            xDoubleAnimation.Completed += XDoubleAnimation_Completed;

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
                animationMaxLayers = 
                    RegisterFrame(frame, animationMaxLayers, 
                        xDoubleAnimation, yDoubleAnimation,
                        xDoubleAnimation1, yDoubleAnimation1, 
                        xDoubleAnimation2, yDoubleAnimation2, visibility0, visibility1, visibility2, ref timeOffset, ref frameIndex);
            
            Animations.Add(animation.Name,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(
                    xDoubleAnimation, yDoubleAnimation),
                visibility0,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(
                    xDoubleAnimation1, yDoubleAnimation1),
                visibility1,
                animationMaxLayers);

            Debug.WriteLine("Added Genius Anim {0}" + animation.Name);
            Debug.WriteLine("...  Frame Count: " + xDoubleAnimation.KeyFrames.Count + " - " +
                            yDoubleAnimation.KeyFrames.Count);
            Debug.WriteLine($"Animation {animation.Name} has {animationMaxLayers} layers");
        }

        protected static List<WeblikeSingleAnimation> DeserializeAnimations(StreamResourceInfo animJStream, List<string> errors)
        {
            var storedAnimations = new List<WeblikeSingleAnimation>();
            var jsonString = StreamToString(animJStream.Stream);
            var jsonDeserSettings = new JsonSerializerSettings
            {
                Error = delegate (object sender, ErrorEventArgs args)
                {
                    errors.Add(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                },
                MissingMemberHandling = MissingMemberHandling.Error,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                NullValueHandling = NullValueHandling.Include,
            };
            
            try
            {
                storedAnimations =
                    JsonConvert.DeserializeObject<List<WeblikeSingleAnimation>>(
                        jsonString,
                        jsonDeserSettings
                        );
            }
            catch (JsonSerializationException e)
            {
                Console.WriteLine($@"Deserialization Error: {e.Message}");
            }

            if (errors.Count > 0)
            {
                MessageBox.Show(errors.ToString());
            }

            return storedAnimations;
        }

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void XDoubleAnimation_Completed(object sender, EventArgs e)
        {
            Debug.WriteLine("Stopping animation");
            IsAnimating = false;
            AssistantFramesImage.Visibility = Visibility.Visible;
            if (AssistantFramesImage.Parent is Canvas canvas)
                canvas.Visibility = Visibility.Visible;

            if (ClippedImage1 != null) 
                ClippedImage1.Visibility = Visibility.Hidden;
            if (ClippedImage1?.Parent is Canvas canvas1)
                canvas1.Visibility = Visibility.Hidden;
        }

        private int RegisterFrame(Frame frame, int animationMaxLayers, DoubleAnimationUsingKeyFrames xDoubleAnimation,
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

                if (frame.branching?.branches != null)
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
                timeOffset += ((double)frame.Duration / 1000);
                frameIndex++;
            }
            else
            {
                Debug.WriteLine("ImageOffsets was null");
            }

            return animationMaxLayers;
        }
    }
}