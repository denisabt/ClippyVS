﻿using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Recoding.ClippyVSPackage
{
    public class AssistantBase : IDisposable
    {
        /// <summary>
        /// The time dispatcher to perform the animations in a random way
        /// </summary>
        protected static DispatcherTimer WpfAnimationsDispatcher;

        /// <summary>
        /// The sprite with all the animation stages for Clippy
        /// </summary>
        protected BitmapSource Sprite;
        
        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        protected System.Windows.Controls.Image AssistantFramesImage;

        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        //private static string spriteResourceUri = "pack://application:,,,/ClippyVSPackage;component/clippy.png";
        protected static string SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/clippy.png";

        /// <summary>
        /// The URI for the animationses json definition
        /// </summary>
        //private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/animations.json";
        protected static string AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/animations.json";

        /// <summary>
        /// Seconds between a random idle animation and another
        /// </summary>
        protected const int IdleAnimationTimeout = 45;

        /// <summary>
        /// When is true it means an animation is actually running
        /// </summary>
        protected bool IsAnimating { get; set; }

        /// <summary>
        /// Reads the content of a stream into a string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected static string StreamToString(Stream stream)
        {
            string streamString;
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                streamString = reader.ReadToEnd();
            }
            return streamString;
        }

        public void Dispose()
        {
            WpfAnimationsDispatcher?.Stop();
        }


        /// <summary>
        /// Watch out, only supports single layer/image for now 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="spriteResourceUri"></param>
        protected void InitAssistant(Panel canvas, string spriteResourceUri, string assistantName, string assistantMapFilename)
        {
            // ReSharper disable once RedundantAssignment
            var spResUri = spriteResourceUri;
#if Dev19
            spResUri = spriteResourceUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
#if Dev22
#endif
            // pass BitmapImage
            var uri = new Uri(spResUri, UriKind.RelativeOrAbsolute);
            ResourceManager rm = Resources.ResourceManager;
            //var resourceSet = rm.GetResourceSet(CultureInfo.InvariantCulture, false, true);
            //var spr = this.GetResourceBitmapFromSharedProject("Merlin", "merlin_map.png");
            var spr = this.GetResourceBitmapFromSharedProject(assistantName, assistantMapFilename);

            //this.Sprite = new BitmapImage(uri);
            this.Sprite = spr;

            AssistantFramesImage = new System.Windows.Controls.Image
            {
                Source = Sprite,
                Stretch = Stretch.None
            };

            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Children.Add(AssistantFramesImage);
        }

        protected Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> RegisterAnimationsImpl(string animationsResourceUri,
           
            EventHandler xDoubleAnimationCompleted, int clipWidth, int clipHeight, string assistantName, string assistantAnimationsFilename)
        {
            var spResUri = animationsResourceUri;
            var animations = new Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>>();

#if Dev19
            spResUri = spResUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
            var uri = new Uri(spResUri, UriKind.RelativeOrAbsolute);

            var animationsString2 = GetStringFromSharedProject(assistantName, assistantAnimationsFilename);
            //var animationsStream = Application.GetResourceStream(uri);

            if (animationsString2 == null)
                return animations;

            // Can go to Constructor/Init
            List<ClippySingleAnimation> storedAnimations = null;
            try
            {
                var animationsString = animationsString2;
                storedAnimations =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClippySingleAnimation>>(animationsString);
            } catch (Exception exjson)
            {
                Console.Write(exjson.ToString());
            }

            if (storedAnimations == null) return animations;

            foreach (var animation in storedAnimations)
            {
                try
                {
                    var xDoubleAnimation = new DoubleAnimationUsingKeyFrames
                    {
                        FillBehavior = FillBehavior.HoldEnd
                    };

                    var yDoubleAnimation = new DoubleAnimationUsingKeyFrames
                    {
                        FillBehavior = FillBehavior.HoldEnd
                    };

                    double timeOffset = 0;

                    foreach (var frame in animation.Frames)
                    {
                        if (frame.ImagesOffsets != null)
                        {
                            var lastCol = frame.ImagesOffsets.Column;
                            var lastRow = frame.ImagesOffsets.Row;

                            // X
                            var xKeyFrame = new DiscreteDoubleKeyFrame(clipWidth * -lastCol,
                                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                            // Y
                            var yKeyFrame = new DiscreteDoubleKeyFrame(clipHeight * -lastRow,
                                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                            timeOffset += ((double) frame.Duration / 1000);
                            xDoubleAnimation.KeyFrames.Add(xKeyFrame);
                            yDoubleAnimation.KeyFrames.Add(yKeyFrame);
                        }
                    }

                    animations.Add(animation.Name,
                        new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation,
                            yDoubleAnimation));
                   // xDoubleAnimation.Completed += xDoubleAnimationCompleted;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error parsing animations");
                }
            }
            return animations;
        }

        /// <summary>
        /// Get a bitmap from the resources of the shared project
        /// </summary>
        /// <param name="assistant">Rocky, Genius or similar (folder in resource name)</param>
        /// <param name="filename">rocky_map.png for example</param>
        /// <returns></returns>
        protected BitmapImage GetResourceBitmapFromSharedProject(string assistant, string filename)
        {
            // XXXXXX
            var executingAssembly = Assembly.GetExecutingAssembly();
            //string[] resourceNames = executingAssembly.GetManifestResourceNames();

            //foreach (var resourceName in resourceNames)
            //{
            //    Debug.WriteLine(resourceName);
            //}


            //var rocky_map_test = executingAssembly.GetManifestResourceInfo(executingAssembly.GetName().Name + ".Resources.Rocky.rocky_map.png");
            var rocky_map_bmp = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + ".Resources."+ assistant+"." +filename);

            // global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Recoding.ClippyVSPackage.Resources", typeof(Resources).Assembly);
            //temp.GetResourceSet(CultureInfo.InvariantCulture, true, true);

            // XXXX
            var bitmap = new BitmapImage();
            using (rocky_map_bmp)
            {
                bitmap.BeginInit();
                bitmap.StreamSource = rocky_map_bmp;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            return bitmap;
        }

        protected string GetStringFromSharedProject(string assistant, string filename)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var textfileStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + ".Resources." + assistant + "." + filename);
            var textfileContents = StreamToString(textfileStream);
            return textfileContents;
        }
    }
}