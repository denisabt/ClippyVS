using System;
using System.Windows.Media.Animation;
using Microsoft.VisualStudio.OLE.Interop;

namespace SharedProject1.AssistImpl
{
    public class LayeredAnimation
    {
        public readonly string Name;
        public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> Layer0;
        public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> Layer1;
        public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> Layer2;

        public readonly ObjectAnimationUsingKeyFrames Visibility0;
        public readonly ObjectAnimationUsingKeyFrames Visibility1;
        public readonly ObjectAnimationUsingKeyFrames Visibility2;
        public readonly int MaxLayers;

        public LayeredAnimation(
            string name, 
            Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer0,
            Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer1,
            Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer2,
            ObjectAnimationUsingKeyFrames visibility0,
            ObjectAnimationUsingKeyFrames visibility1,
            ObjectAnimationUsingKeyFrames visibility2,
            int animMaxLayers,
            EventHandler XDoubleAnimation_Completed) : this(name)
        {
            Layer0 = layer0;
            Layer1 = layer1;
            Layer2 = layer2;
            Visibility0 = visibility0;
            Visibility1 = visibility1;
            Visibility2 = visibility2;
            MaxLayers = animMaxLayers;

            Layer0.Item1.Completed += XDoubleAnimation_Completed;
        }

        public LayeredAnimation(string name)
        {
            Name = name;
            Layer0 = new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(new DoubleAnimationUsingKeyFrames(), new DoubleAnimationUsingKeyFrames());
            Layer1 = new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(new DoubleAnimationUsingKeyFrames(), new DoubleAnimationUsingKeyFrames());
            Layer2 = new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(new DoubleAnimationUsingKeyFrames(), new DoubleAnimationUsingKeyFrames());
            Visibility0 = new ObjectAnimationUsingKeyFrames();
            Visibility1 = new ObjectAnimationUsingKeyFrames();
            Visibility2 = new ObjectAnimationUsingKeyFrames();
        }

    }
}