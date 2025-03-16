using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Animation;

namespace SharedProject1.AssistImpl
{
    public class LayeredAnimations
    {
        private readonly List<LayeredAnimation> _animations;

        public LayeredAnimations(int capacity)
        {
            _animations = new List<LayeredAnimation>(capacity);
        }

        public void Add(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer0, ObjectAnimationUsingKeyFrames visibility0,
            Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer1, ObjectAnimationUsingKeyFrames visibility1,
            int animMaxLayers)
        {
            _animations.Add(new LayeredAnimation(animName, layer0, layer1, visibility1, animMaxLayers));
            Debug.WriteLine("Added animation");
        }

        public LayeredAnimation this[string animName]
        {
            get
            {
                LayeredAnimation result = _animations.First(animation => animation.Name.Equals(animName));
                Debug.WriteLine("Returning animation frames for {0} with {1}", animName, _animations.Count.ToString());
                return result;
            }
        }
    }
}