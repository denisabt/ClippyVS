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
        protected Image Layer1;

        protected int MaxLayers = 0;

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
        protected List<WeblikeSingleAnimation> ParseAnimDescriptions()
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
            
            double timeOffset = 0;
            var frameIndex = 0;
            var singleAnimationFrames = ParseWeblikeAnimation(animation, timeOffset, frameIndex);

            Animations.Add(animation.Name,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(
                    singleAnimationFrames.Layer0.Item1, singleAnimationFrames.Layer0.Item2),
                singleAnimationFrames.Visibility1,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(
                    singleAnimationFrames.Layer1.Item1, singleAnimationFrames.Layer1.Item2),
                singleAnimationFrames.Visibility1,
                MaxLayers, XDoubleAnimation_Completed);

            Debug.WriteLine("Added RockyGenius Anim {0}", animation.Name);
            Debug.WriteLine("...  Frame Count: " + singleAnimationFrames.Layer0.Item1.KeyFrames.Count + " - " +
                            singleAnimationFrames.Layer0.Item2.KeyFrames.Count);
            Debug.WriteLine($"Animation {animation.Name} has {MaxLayers} layers");
        }

        protected LayeredAnimation ParseWeblikeAnimation(WeblikeSingleAnimation animation, 
            double timeOffset, int frameIndex)
        {
            // Get rid of this class and replace with LayeredAnimation TODO
            LayeredAnimation singleAnimationFrameses = new LayeredAnimation(animation.Name);
            
            foreach (var frame in animation.Frames) {
                RegisterFrame(frame, 
                    singleAnimationFrameses, ref timeOffset, ref frameIndex);
            }

            return singleAnimationFrameses;
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
        private void XDoubleAnimation_Completed(object sender, EventArgs e)
        {
            Debug.WriteLine("Stopping animation");
            IsAnimating = false;
            Layer0.Visibility = Visibility.Visible;
            if (Layer0.Parent is Canvas canvas)
                canvas.Visibility = Visibility.Visible;

            if (Layer1 != null) 
                Layer1.Visibility = Visibility.Hidden;
            if (Layer1?.Parent is Canvas canvas1)
                canvas1.Visibility = Visibility.Hidden;
        }

        private void RegisterFrame(Frame frame, LayeredAnimation layeredAnimation, ref double timeOffset, ref int frameIndex)
        {
            
            if (frame.ImagesOffsets != null)
            {
                if (frame.ImagesOffsets.Count > MaxLayers)
                {
                    MaxLayers = frame.ImagesOffsets.Count;
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
                    // Prepare
                    // Key frame for all potential layers (max 3)
                    //var layer0

                    //layeredAnimatsion.xDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    //layeredAnimatsion.yDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    //layeredAnimatsion.visibility0.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
                    //layeredAnimatsion.xDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    //layeredAnimatsion.yDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    //layeredAnimatsion.visibility1.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
                    //layeredAnimatsion.xDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    //layeredAnimatsion.yDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    //layeredAnimatsion.visibility2.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));

                    //Overlay is actually - layers - displayed at the same time...
                    var lastCol = frame.ImagesOffsets[layerNum][0];
                    var lastRow = frame.ImagesOffsets[layerNum][1];

                    // X and Y
                    var frameKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset));
                    var xKeyFrame = new DiscreteDoubleKeyFrame(lastCol * -1,
                        frameKeyTime);
                    var yKeyFrame = new DiscreteDoubleKeyFrame(lastRow * -1,
                        frameKeyTime);

                    int visibleLayers = frame.ImagesOffsets.Count;  

                    switch (layerNum)
                    {
                        case 0:
                            //var layer0Frame = new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xKeyFrame)
                            layeredAnimation.Layer0.Item1.KeyFrames.Add(xKeyFrame);
                            layeredAnimation.Layer0.Item2.KeyFrames.Add(yKeyFrame);
                            var visibility0Frame = new DiscreteObjectKeyFrame(0.0, frameKeyTime);
                            layeredAnimation.Visibility0.KeyFrames.Add(visibility0Frame);
                            break;
                        case 1:
                            layeredAnimation.Layer1.Item1.KeyFrames.Add(xKeyFrame);
                            layeredAnimation.Layer1.Item2.KeyFrames.Add(yKeyFrame);
                            // XXXXX DEBUG..  and unit test
                            var visibility1Frame = new DiscreteObjectKeyFrame((visibleLayers > 1 ? Visibility.Visible : Visibility.Hidden ), frameKeyTime);
                            layeredAnimation.Visibility1.KeyFrames.Add(visibility1Frame);
                            break;
                        case 2:
                            layeredAnimation.Layer2.Item1.KeyFrames.Add(xKeyFrame);
                            layeredAnimation.Layer2.Item2.KeyFrames.Add(yKeyFrame);
                            var visibility2Frame = new DiscreteObjectKeyFrame(0.0, frameKeyTime);
                            //layeredAnimation.Visibility2.KeyFrames.Add(visibility2Frame);
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


            return;
        }
    }
}