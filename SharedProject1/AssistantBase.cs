﻿using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Recoding.ClippyVSPackage
{
    public class AssistantBase
    {
        /// <summary>
        /// The sprite with all the animation stages for Clippy
        /// </summary>
        protected BitmapSource Sprite;

        /// <summary>
        /// The actual Clippy container that works as a clipping mask
        /// </summary>
        public Canvas ClippyCanvas { get; private set; }

        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        protected Image clippedImage;

        /// <summary>
        /// Seconds between a random idle animation and another
        /// </summary>
        protected const int IdleAnimationTimeout = 45;

        /// <summary>
        /// When is true it means an animation is actually running
        /// </summary>
        public bool IsAnimating { get; set; }


        /// <summary>
        /// Reads the content of a stream into a string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string StreamToString(Stream stream)
        {
            string streamString = string.Empty;
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                streamString = reader.ReadToEnd();
            }
            return streamString;
        }
    }
}