using System;
using K1.Gameplay;
using Mopsicus.InfiniteScroll;
using UnityEngine;
using UnityEngine.Video;

namespace K1.UI
{
    public class UICharaterUltPanel : UIPanel
    {
        public VideoPlayer mVideo;

        public override void ShowPanel(float delta, float duration)
        {
            base.ShowPanel(delta, duration);
            mVideo.Play();
        }
    }
}