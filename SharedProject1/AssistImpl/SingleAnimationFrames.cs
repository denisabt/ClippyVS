using System;
using System.Text;
using System.Windows.Media.Animation;

namespace SharedProject1.AssistImpl
{
    internal class SingleAnimationFrames
    {
        public DoubleAnimationUsingKeyFrames xDoubleAnimation = new DoubleAnimationUsingKeyFrames
        {
            FillBehavior = FillBehavior.HoldEnd
        };

        public DoubleAnimationUsingKeyFrames yDoubleAnimation = new DoubleAnimationUsingKeyFrames
        {
            FillBehavior = FillBehavior.HoldEnd
        };
        public ObjectAnimationUsingKeyFrames visibility0 = new ObjectAnimationUsingKeyFrames();

        public DoubleAnimationUsingKeyFrames xDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
        {
            FillBehavior = FillBehavior.HoldEnd
        };
        public DoubleAnimationUsingKeyFrames yDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
        {
            FillBehavior = FillBehavior.HoldEnd
        };
        public ObjectAnimationUsingKeyFrames visibility1 = new ObjectAnimationUsingKeyFrames();

        public DoubleAnimationUsingKeyFrames xDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
        {
            FillBehavior = FillBehavior.HoldEnd
        };
        public DoubleAnimationUsingKeyFrames yDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
        {
            FillBehavior = FillBehavior.HoldEnd
        };
        public ObjectAnimationUsingKeyFrames visibility2 = new ObjectAnimationUsingKeyFrames();

        public SingleAnimationFrames(EventHandler XDoubleAnimation_Completed)
        {
            xDoubleAnimation.Completed += XDoubleAnimation_Completed;
        }
    }
}
