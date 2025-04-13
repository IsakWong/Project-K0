using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayQueuePlayable : PlayableBehaviour
{
    public bool mLoop = false;
    private int m_CurrentClipIndex = -1;

    private float m_TimeToNextClip;

    private Playable mixer;

    public void Initialize(List<AnimationClip> clipsToPlay, Playable owner, PlayableGraph graph)

    {
        if (clipsToPlay == null)
            return;

        owner.SetInputCount(1);
        mixer = AnimationMixerPlayable.Create(graph, clipsToPlay.Count);

        graph.Connect(mixer, 0, owner, 0);

        owner.SetInputWeight(0, 1);

        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)
        {
            graph.Connect(AnimationClipPlayable.Create(graph, clipsToPlay[clipIndex]), 0, mixer, clipIndex);
            mixer.SetInputWeight(clipIndex, 1.0f);
        }
    }

    override public void PrepareFrame(Playable owner, FrameData info)

    {
        if (mixer.GetInputCount() == 0)

            return;

        // Advance to next clip if necessary

        m_TimeToNextClip -= (float)info.deltaTime;

        if (m_TimeToNextClip <= 0.0f)

        {
            m_CurrentClipIndex++;

            if (m_CurrentClipIndex >= mixer.GetInputCount())
            {
                if (mLoop)
                {
                    m_CurrentClipIndex = 0;
                }
                else
                {
                    return;
                }
            }


            var currentClip = (AnimationClipPlayable)mixer.GetInput(m_CurrentClipIndex);

            // Reset the time so that the next clip starts at the correct position

            currentClip.SetTime(0);

            m_TimeToNextClip = currentClip.GetAnimationClip().length;
        }

        // Adjust the weight of the inputs

        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)

        {
            if (clipIndex == m_CurrentClipIndex)

                mixer.SetInputWeight(clipIndex, 1.0f);

            else

                mixer.SetInputWeight(clipIndex, 0.0f);
        }
    }
}