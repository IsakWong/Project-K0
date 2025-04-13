using System;
using K1.Gameplay;
using Mopsicus.InfiniteScroll;
using UnityEngine;

namespace K1.UI
{
    public class UITalentPanel : UIPanel
    {
        protected CharacterUnit mTalentTree;
        public InfiniteScroll mScroll;

        public void SetTalentTree(CharacterUnit talentTree)
        {
            mTalentTree = talentTree;
            if (mTalentTree)
                mScroll.InitData(mTalentTree.mTalentNodes.Count);
            else
                mScroll.InitData(0);
        }

        public void Start()
        {
            mScroll.OnFill += (i, o) => { o.GetComponent<UITalentIcon>().AttachTalent = mTalentTree.mTalentNodes[i]; };
            mScroll.OnHeight += (index) => { return 150; };
        }
    }
}