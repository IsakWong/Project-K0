using System;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;
using UnityEngine.Serialization;

namespace K1.Gameplay
{
    [DisallowMultipleComponent]
    public class GameUnit : UnitBase
    {

        public Vector3 DirectionToTarget(GameUnit to)
        {
            return (to.transform.position - transform.position).normalized;
        }


    }
}