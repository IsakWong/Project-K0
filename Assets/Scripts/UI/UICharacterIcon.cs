using System.Collections;
using System.Collections.Generic;
using K1.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICharacterIcon : UIIcon
{
    public CharacterUnit _character;
    public Image mIcon;

    public CharacterUnit mCharacter
    {
        get { return _character; }
        set
        {
            _character = value;
            if (_character)
            {
                mIcon.sprite = _character.mCharacterConfig.Avatar;
            }
        }
    }

    protected override void OnShowTip()
    {
        mTipOrigin.SetContent(mCharacter.mCharacterConfig.Name, mCharacter.mCharacterConfig.Description);
        base.OnShowTip();
    }
}