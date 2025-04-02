using Recoding.ClippyVSPackage.Configurations;
using SharedProject1.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        private static DispatcherTimer _wpfAnimationsDispatcher;

        /// <summary>
        /// The sprite with all the animation stages for Clippy
        /// </summary>
        protected BitmapSource Sprite;

        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        protected System.Windows.Controls.Image Layer0;

        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        protected static string SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/clippy.png";

        /// <summary>
        /// The URI for the animationses json definition
        /// </summary>
        protected string AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/animations.json";

        /// <summary>
        /// Seconds between a random idle animation and another
        /// </summary>
        private const int IdleAnimationTimeout = 45;

        /// <summary>
        /// When is true it means an animation is actually running
        /// </summary>
        protected bool IsAnimating { get; set; }
        protected string AssistantName { get; set; }
        protected string AssistantMapFilename { get; set; }

        public void PopulateContextMenu(ContextMenu assistantContextMenu, bool showClippy, bool showMerlin, bool showGenius, bool showRocky, RoutedEventHandler menuItemOnClick)
        {
#if DEBUG
            var values = Enum.GetValues(typeof(ClippyAnimations));
            if (showClippy)
            {
                values = Enum.GetValues(typeof(ClippyAnimations));
            }
            if (showMerlin)
            {
                values = Enum.GetValues(typeof(MerlinAnimations));
            }
            if (showGenius)
            {
                values = Enum.GetValues(typeof(GeniusAnimations));
            }
            if (showRocky)
            {
                values = Enum.GetValues(typeof(RockyAnimations));
            }

            //// TEMP: create a voice for each animation in the context menu
            assistantContextMenu.Items.Clear();

            foreach (var val in values)
            {
                var menuItem = new MenuItem()
                {
                    Header = val.ToString(),
                    Name = "cmd" + val
                };
                menuItem.Click += menuItemOnClick;
                assistantContextMenu.Items.Add(menuItem);
            }
#endif
        }
        /// <summary>
        /// Reads the content of a stream into a string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected static string StreamToString(Stream stream)
        {
            string streamString;
            stream.Position = 0;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                streamString = reader.ReadToEnd();
            }
            return streamString;
        }

        public void Dispose()
        {
            _wpfAnimationsDispatcher?.Stop();
        }


        /// <summary>
        /// Watch out, only supports single layer/image for now 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="spriteResourceUri"></param>
        protected void InitAssistant(Panel canvas)
        {
            if (canvas == null) return; 
            
            Sprite = GetResourceBitmapFromSharedProject(AssistantName, AssistantMapFilename);

            Layer0 = new Image
            {
                Source = Sprite,
                Stretch = Stretch.None
            };

            canvas.Children.Clear();
            canvas.Children.Add(Layer0);
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

            if (animationsString2 == null)
                return animations;

            // Can go to Constructor/Init
            List<ClippySingleAnimation> storedAnimations = null;
            try
            {
                var animationsString = animationsString2;
                storedAnimations =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClippySingleAnimation>>(animationsString);
            }
            catch (Exception exjson)
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

                            timeOffset += ((double)frame.Duration / 1000);
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
            if (string.IsNullOrEmpty(assistant) || string.IsNullOrEmpty(filename))
                return new BitmapImage();

            //string[] resourceNames = executingAssembly.GetManifestResourceNames();

            var executingAssembly = Assembly.GetExecutingAssembly();
            var rocky_map_bmp = executingAssembly.GetManifestResourceStream(
                executingAssembly.GetName().Name + ".Resources." + assistant + "." + filename
                );

            if (rocky_map_bmp == null)
                return new BitmapImage();

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

        private string GetStringFromSharedProject(string assistant, string filename)
        {
            if (string.IsNullOrEmpty(assistant) || string.IsNullOrEmpty(filename))
                return string.Empty;

            var executingAssembly = Assembly.GetExecutingAssembly();
            var assemblyName = executingAssembly.GetName().Name;
            var manifestResourceName = assemblyName + ".Resources." + assistant + "." + filename;
            string textfileContents;

            using (var textfileStream = executingAssembly.GetManifestResourceStream(manifestResourceName))
            {
                textfileContents = StreamToString(textfileStream);
            }

            return textfileContents;
        }

        /// <summary>
        /// Registers a function to perform a subset of animationses randomly (the idle ones)
        /// </summary>
        protected void RegisterIdleRandomAnimations(EventHandler WPFAnimationsDispatcher_Tick)
        {
            _wpfAnimationsDispatcher = new DispatcherTimer();

            _wpfAnimationsDispatcher.Interval = TimeSpan.FromSeconds(IdleAnimationTimeout);
            _wpfAnimationsDispatcher.Tick += WPFAnimationsDispatcher_Tick;

            _wpfAnimationsDispatcher.Start();
        }
    }
}