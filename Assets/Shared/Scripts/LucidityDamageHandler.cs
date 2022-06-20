using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.RpgGame.World;
using CommonCore.State;
using CommonCore.World;
using CommonCore;
using CommonCore.Messaging;

namespace Lucidity
{

    /// <summary>
    /// Handles taking damage and self-healing
    /// </summary>
    public class LucidityDamageHandler : MonoBehaviour
    {
        [SerializeField]
        private PlayerController PlayerController;

        [SerializeField]
        private float DamageMultiplier = 1.0f; //if we need to adjust things

        [SerializeField]
        private float HealAfterDamageDelay = 5f;
        [SerializeField]
        private float HealRate = 10f;

        [SerializeField]
        private float PainThreshold = 0.25f;

        [SerializeField]
        private AudioSource HitSound = null;
        [SerializeField]
        private AudioSource PainSound = null;
        [SerializeField]
        private AudioSource Hit2Sound = null;

        private float TimeSinceLastHit = 0;

        private void Start()
        {
            if (PlayerController == null)
                PlayerController = GetComponent<PlayerController>();

            PlayerController.DamageHandler = HandleDamageTaken;
        }

        private void Update()
        {
            if(TimeSinceLastHit >= HealAfterDamageDelay)
            {
                float healed = HealRate * Time.deltaTime;
                GameState.Instance.PlayerRpgState.Health = Mathf.Min(GameState.Instance.PlayerRpgState.Health + healed, GameState.Instance.PlayerRpgState.DerivedStats.MaxHealth);
            }
            else
            {
                TimeSinceLastHit += Time.deltaTime;

                if(TimeSinceLastHit >= HealAfterDamageDelay)
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("LucidityPlayerStartedHealing"));
            }
        }

        private void HandleDamageTaken(ActorHitInfo hitInfo)
        {
            //Debug.Log($"Player took {hitInfo.Damage} damage!");
            float totalDamage = hitInfo.Damage * DamageMultiplier + hitInfo.DamagePierce;
            GameState.Instance.PlayerRpgState.Health -= totalDamage;

            if(GameState.Instance.PlayerRpgState.HealthFraction < PainThreshold)
            {
                HitSound.Ref()?.Play();
                PainSound.Ref()?.Play();
                Hit2Sound.Ref()?.Play();
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("LucidityPlayerHitPenetrated"));
            }
            else
            {
                HitSound.Ref()?.Play();
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("LucidityPlayerHit"));
            }

            TimeSinceLastHit = 0;
        }
    }

}