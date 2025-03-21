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
        protected LayeredAnimations Animations;

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

        /// <summary>
        /// todo Migrate to shared file use
        /// </summary>
        /// <returns></returns>
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

            SingleAnimationFrame singleAnimationFrame = new SingleAnimationFrame(XDoubleAnimation_Completed);

            double timeOffset = 0;
            var frameIndex = 0;

            var animationMaxLayers = 0;
            foreach (var frame in animation.Frames) 
                animationMaxLayers = 
                    RegisterFrame(frame, animationMaxLayers, 
                        singleAnimationFrame, ref timeOffset, ref frameIndex);
            
            Animations.Add(animation.Name,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(
                    singleAnimationFrame.xDoubleAnimation, singleAnimationFrame.yDoubleAnimation),
                singleAnimationFrame.visibility0,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(
                    singleAnimationFrame.xDoubleAnimation1, singleAnimationFrame.yDoubleAnimation1),
                singleAnimationFrame.visibility1,
                animationMaxLayers);

            Debug.WriteLine("Added RockyGenius Anim {0}", animation.Name);
            Debug.WriteLine("...  Frame Count: " + singleAnimationFrame.xDoubleAnimation.KeyFrames.Count + " - " +
                            singleAnimationFrame.yDoubleAnimation.KeyFrames.Count);
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

        private int RegisterFrame(Frame frame, int animationMaxLayers, SingleAnimationFrame singleAnimationFrame, ref double timeOffset, ref int frameIndex)
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
                    Debug.WriteLine("Frame {frameIndex} has branching to " + frame.branching.branches.Count +
                                    " branches.");

                    int branchWeightTotal = 0;
                    foreach (Branch b in frame.branching.branches)
                    {
                        Debug.WriteLine("Branch-Option is " + b.frameIndex);
                        Debug.WriteLine("Branch-Weight is " + b.weight);
                        branchWeightTotal += b.weight;
                    }

                    Debug.WriteLine("Total Branches Weight: " + branchWeightTotal);
                    //https://github.com/pi0/clippyjs/blob/d88943d529410114c9cea7f01e05de40254cd914/lib/animator.js#L121
                }

                for (var layerNum = 0; layerNum < frame.ImagesOffsets.Count; layerNum++)
                {
                    //Debug.WriteLine("Processing Overlay " + layerNum);

                    // For Branching reasons, this can actually only be assembled on runtime.... :-/
                    // Prepare Key frame for all potential layers (max 3)
                    singleAnimationFrame.xDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    singleAnimationFrame.yDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    singleAnimationFrame.visibility0.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
                    singleAnimationFrame.xDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    singleAnimationFrame.yDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    singleAnimationFrame.visibility1.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
                    singleAnimationFrame.xDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    singleAnimationFrame.yDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    singleAnimationFrame.visibility2.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));

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
                            singleAnimationFrame.xDoubleAnimation.KeyFrames.Insert(frameIndex, xKeyFrame);
                            singleAnimationFrame.yDoubleAnimation.KeyFrames.Insert(frameIndex, yKeyFrame);
                            singleAnimationFrame.visibility0.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
                            break;
                        case 1:
                            singleAnimationFrame.xDoubleAnimation1.KeyFrames.Insert(frameIndex, xKeyFrame);
                            singleAnimationFrame.yDoubleAnimation1.KeyFrames.Insert(frameIndex, yKeyFrame);
                            singleAnimationFrame.visibility1.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
                            break;
                        case 2:
                            singleAnimationFrame.xDoubleAnimation2.KeyFrames.Insert(frameIndex, xKeyFrame);
                            singleAnimationFrame.yDoubleAnimation2.KeyFrames.Insert(frameIndex, yKeyFrame);
                            singleAnimationFrame.visibility2.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
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