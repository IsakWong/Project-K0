﻿using UnityEngine;

namespace Obi.Samples
{

	[RequireComponent(typeof(ObiEmitter))]
	public class ViscositySurfTensionToUserData : MonoBehaviour
	{
		void Awake()
        {
            GetComponent<ObiEmitter>().OnEmitParticle += Emitter_OnEmitParticle;
		}

		void Emitter_OnEmitParticle (ObiEmitter emitter, int particleIndex)
		{
			if (emitter.solver != null)
            {
                int k = emitter.solverIndices[particleIndex];
				
				Vector4 userData = emitter.solver.userData[k];
				userData[0] = emitter.solver.fluidMaterials[k].z; //viscosity
				userData[1] = emitter.solver.fluidMaterials[k].y; //surf tension
				emitter.solver.userData[k] = userData;
			}		
		}
	
	}
}

