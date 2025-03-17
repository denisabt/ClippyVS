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
//    public class WebLikeAssistant : AssistantBase
//    {
//        protected  string SpriteResourceUri;
//        protected string AnimationsResourceUri;

//        /// <summary>
//        /// The list of couples of Columns/Rows double animations , supports no overlays
//        /// </summary>
//        protected static LayeredAnimations _animations;

//        /// <summary>
//        /// The image that holds the sprite
//        /// </summary>
//        protected Image _lyr2AssistantFramesImage;

//        protected void RegisterAnimation(WeblikeSingleAnimation animation)
//        {
//            var xDoubleAnimation = new DoubleAnimationUsingKeyFrames
//            {
//                FillBehavior = FillBehavior.HoldEnd
//            };
//            var yDoubleAnimation = new DoubleAnimationUsingKeyFrames
//            {
//                FillBehavior = FillBehavior.HoldEnd
//            };
//            var visibility0 = new ObjectAnimationUsingKeyFrames();

//            var xDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
//            {
//                FillBehavior = FillBehavior.HoldEnd
//            };
//            var yDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
//            {
//                FillBehavior = FillBehavior.HoldEnd
//            };
//            var visibility1 = new ObjectAnimationUsingKeyFrames();

//            var xDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
//            {
//                FillBehavior = FillBehavior.HoldEnd
//            };
//            var yDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
//            {
//                FillBehavior = FillBehavior.HoldEnd
//            };
//            var visibility2 = new ObjectAnimationUsingKeyFrames();

//            double timeOffset = 0;
//            var frameIndex = 0;
//            var animationMaxLayers = 0;

//            foreach (var frame in animation.Frames)
//            {
//                animationMaxLayers = RegisterFrame(frame, animationMaxLayers, xDoubleAnimation, yDoubleAnimation, xDoubleAnimation1, yDoubleAnimation1, xDoubleAnimation2, yDoubleAnimation2, visibility0, visibility1, visibility2, ref timeOffset, ref frameIndex);
//            }

//            _animations.Add(animation.Name,
//                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation),
//                visibility0,
//                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation1, yDoubleAnimation1),
//                visibility1,
//                animationMaxLayers);

//            Debug.WriteLine("Added Genius Anim {0}", animation.Name);
//            Debug.WriteLine("...  Frame Count: " + xDoubleAnimation.KeyFrames.Count + " - " +
//                            yDoubleAnimation.KeyFrames.Count);
//            Debug.WriteLine($"Animation {animation.Name} has {animationMaxLayers} layers");

//            xDoubleAnimation.Completed += XDoubleAnimation_Completed;
//        }

//        /// <summary>
//        /// Callback to execute at the end of an animation
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void XDoubleAnimation_Completed(object sender, EventArgs e)
//        {
//            IsAnimating = false;
//            AssistantFramesImage.Visibility = Visibility.Visible;
//            if (AssistantFramesImage.Parent is Canvas canvas)
//                canvas.Visibility = Visibility.Visible;

//            _lyr2AssistantFramesImage.Visibility = Visibility.Hidden;
//            if (_lyr2AssistantFramesImage.Parent is Canvas canvas1)
//                canvas1.Visibility = Visibility.Hidden;
//        }

//        protected int RegisterFrame(Frame frame, int currentLayerCount, DoubleAnimationUsingKeyFrames xDoubleAnimation,
//            DoubleAnimationUsingKeyFrames yDoubleAnimation, DoubleAnimationUsingKeyFrames xDoubleAnimation1,
//            DoubleAnimationUsingKeyFrames yDoubleAnimation1, DoubleAnimationUsingKeyFrames xDoubleAnimation2,
//            DoubleAnimationUsingKeyFrames yDoubleAnimation2, ObjectAnimationUsingKeyFrames visibility0, ObjectAnimationUsingKeyFrames visibility1, ObjectAnimationUsingKeyFrames visibility2, ref double timeOffset, ref int frameIndex)
//        {
//            if (frame.ImagesOffsets != null)
//            {
//                if (currentLayerCount < frame.ImagesOffsets.Count)
//                {
//                    currentLayerCount = frame.ImagesOffsets.Count;
//                }

//                if (frame.branching != null && frame.branching.branches != null)
//                {
//                    Debug.WriteLine("Has Branching Info");
//                }

//                for (var layerNum = 0; layerNum < frame.ImagesOffsets.Count; layerNum++)
//                {
//                    Debug.WriteLine("Processing Overlay " + layerNum);

//                    // Prepare Key frame for all potential layers (max 3)
//                    xDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
//                    yDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
//                    visibility0.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
//                    xDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
//                    yDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
//                    visibility1.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
//                    xDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
//                    yDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
//                    visibility2.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));

//                    //Overlay is actually - layers - displayed at the same time...
//                    var lastCol = frame.ImagesOffsets[layerNum][0];
//                    var lastRow = frame.ImagesOffsets[layerNum][1];

//                    // X and Y
//                    var frameKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset));
//                    var xKeyFrame = new DiscreteDoubleKeyFrame(lastCol * -1,
//                        frameKeyTime);
//                    var yKeyFrame = new DiscreteDoubleKeyFrame(lastRow * -1,
//                        frameKeyTime);

//                    AddFrameToAnimLayer(xDoubleAnimation, yDoubleAnimation, xDoubleAnimation1, yDoubleAnimation1, xDoubleAnimation2, yDoubleAnimation2, visibility0, visibility1, visibility2, frameIndex, layerNum, frameKeyTime, xKeyFrame, yKeyFrame);
//                }

//                //timeOffset += ((double)frame.Duration / 1000 * 4);
//                timeOffset += ((double)frame.Duration / 1000);
//                frameIndex++;
//            }
//            else
//            {
//                Debug.WriteLine("ImageOffsets was null");
//            }

//            return currentLayerCount;
//        }

//        protected void AddFrameToAnimLayer(DoubleAnimationUsingKeyFrames xDoubleAnimation, DoubleAnimationUsingKeyFrames yDoubleAnimation, DoubleAnimationUsingKeyFrames xDoubleAnimation1, DoubleAnimationUsingKeyFrames yDoubleAnimation1, DoubleAnimationUsingKeyFrames xDoubleAnimation2, DoubleAnimationUsingKeyFrames yDoubleAnimation2, ObjectAnimationUsingKeyFrames visibility0, ObjectAnimationUsingKeyFrames visibility1, ObjectAnimationUsingKeyFrames visibility2, int frameIndex, int layerNum, KeyTime frameKeyTime, DiscreteDoubleKeyFrame xKeyFrame, DiscreteDoubleKeyFrame yKeyFrame)
//        {
//            switch (layerNum)
//            {
//                case 0:
//                    xDoubleAnimation.KeyFrames.Insert(frameIndex, xKeyFrame);
//                    yDoubleAnimation.KeyFrames.Insert(frameIndex, yKeyFrame);
//                    visibility0.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
//                    break;
//                case 1:
//                    xDoubleAnimation1.KeyFrames.Insert(frameIndex, xKeyFrame);
//                    yDoubleAnimation1.KeyFrames.Insert(frameIndex, yKeyFrame);
//                    visibility1.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
//                    break;
//                case 2:
//                    xDoubleAnimation2.KeyFrames.Insert(frameIndex, xKeyFrame);
//                    yDoubleAnimation2.KeyFrames.Insert(frameIndex, yKeyFrame);
//                    visibility2.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
//                    break;
//            }
//        }

//        protected List<WeblikeSingleAnimation> ParseJsStyleAnimDefinition()
//        {
//            var spResUri = AnimationsResourceUri;
//#if Dev19
//            spResUri = spResUri.Replace("ClippyVs2022", "ClippyVSPackage");
//#endif
//            var animJStream = Application.GetResourceStream(new Uri(spResUri, UriKind.RelativeOrAbsolute));

//            if (animJStream == null)
//                return null;

//            // Can go to Constructor/Init
//            var errors = new List<string>();

//            var storedAnimations = DeserializeAnimations(animJStream, errors);
//            return storedAnimations;
//        }

//        private static List<WeblikeSingleAnimation> DeserializeAnimations(StreamResourceInfo animJStream, List<string> errors)
//        {
//            var storedAnimations =
//                JsonConvert.DeserializeObject<List<WeblikeSingleAnimation>>(StreamToString(animJStream.Stream),
//                    new JsonSerializerSettings
//                    {
//                        Error = delegate (object sender, ErrorEventArgs args)
//                        {
//                            errors.Add(args.ErrorContext.Error.Message);
//                            args.ErrorContext.Handled = true;
//                        },
//                        MissingMemberHandling = MissingMemberHandling.Error,
//                        NullValueHandling = NullValueHandling.Include
//                    });
//            return storedAnimations;
//        }

//        /// <summary>
//        /// Registers all the animation definitions into a static property
//        /// </summary>
//        protected void RegisterAnimations()
//        {
//            var storedAnimations = ParseJsStyleAnimDefinition();
//            if (storedAnimations == null) return;

//            _animations = new LayeredAnimations(storedAnimations.Count);

//            foreach (var animation in storedAnimations)
//            {
//                RegisterAnimation(animation);
//            }
//        }
//    }
}