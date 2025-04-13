using System.Collections.Generic;

namespace K1.Gameplay
{
    public partial class CharacterUnit : GameUnit
    {
        #region Talent天赋树相关

        public TalentTree mTalentTree = new();
        public List<TalentNode> mTalentNodes = new();
        public int mTalentPoint = 5;

        public void AddTalent(TalentNode node)
        {
            node.mOwner = this;
            node.OnTalentEnable();
            mTalentNodes.Add(node);
        }

        public bool EnableTalentNode(TalentNode node, int pointCost = 1)
        {
            if (node.mTalentEnable)
                return true;
            if (mTalentPoint >= pointCost)
                mTalentPoint -= pointCost;
            else
                return false;
            node.mTalentEnable = true;
            return true;
        }

        public TalentNode GetTalent<T>()
        {
            foreach (var node in mTalentNodes)
            {
                if (node is T)
                    return node;
            }

            return null;
        }

        #endregion
    }
}