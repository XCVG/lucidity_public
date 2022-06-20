using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidity
{

    /// <summary>
    /// Script that animates and eventually destroys the power attack effect
    /// </summary>
    public class PowerAttackEffectScript : MonoBehaviour
    {
        [SerializeField]
        private float MoveSpeed = 5f;
        [SerializeField]
        public float MoveDistance = 10f;
        [SerializeField]
        public float HoldTime = 1f;

        [SerializeField]
        private ParticleSystem Particles = null;
        [SerializeField]
        private AudioSource Sound = null;

        private float TimeHeld = 0;
        private float DistanceMoved = 0;

        private void Update()
        {
            if(DistanceMoved < MoveDistance)
            {
                Vector3 moveVec = transform.forward * MoveSpeed * Time.deltaTime;
                transform.Translate(moveVec, Space.World);
                DistanceMoved += moveVec.magnitude;



                if(DistanceMoved >= MoveDistance)
                {
                    Particles.loop = false; //Particles.main.loop doesn't actually work, great job unity
                    if (Sound != null)
                        Sound.loop = false;
                }
            }
            else
            {
                Vector3 moveVec = transform.forward * MoveSpeed * Time.deltaTime;
                transform.Translate(moveVec, Space.World);

                TimeHeld += Time.deltaTime;
                if(TimeHeld > HoldTime)
                {
                    Destroy(gameObject);
                }
            }
        }

    }
}