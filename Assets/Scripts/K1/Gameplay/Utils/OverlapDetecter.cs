using System;
using System.Collections.Generic;
using K1.Gameplay;
using UnityEngine;

public class OverlapDetecter : MonoBehaviour
{
    public List<CharacterUnit> OverlapUnits = new List<CharacterUnit>();
    public CharacterUnit Owner;

    private void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponent<CharacterUnit>();
        if (target && target.IsEnemy(Owner))
        {
            OverlapUnits.Add(other.GetComponent<CharacterUnit>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var target = other.GetComponent<CharacterUnit>();
        if (OverlapUnits.Contains(target))
        {
            OverlapUnits.Remove(target);
        }
    }
}