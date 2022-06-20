using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.World;
using CommonCore.RpgGame;
using CommonCore.RpgGame.World;
using CommonCore.LockPause;
using CommonCore.Input;
using System;
using CommonCore.RpgGame.Rpg;
using UnityEngine.UI;
using CommonCore.State;
using CommonCore.Messaging;

namespace Lucidity
{

    /// <summary>
    /// Handles weapon animation and firing
    /// </summary>
    public class LucidityWeaponController : MonoBehaviour
    {
        [SerializeField, Header("Components")]
        private PlayerController PlayerController;
        [SerializeField]
        private Transform ShootPoint;
        [SerializeField]
        private Transform EffectSpawnPoint;
        [SerializeField]
        private Image WeaponImage;        

        [SerializeField, Header("Attack Parameters")]
        private float AttackInterval = 1f;
        [SerializeField]
        private float AttackDamage = 10f;
        [SerializeField]
        private float AttackReach = 1.0f;
        [SerializeField]
        private string AttackHitPuff = "TestHitPuff";

        [SerializeField, Header("Power Attack")]
        private float PowerAttackInterval = 5f;
        [SerializeField]
        private float PowerAttackContactDamage = 25f;
        [SerializeField]
        private float PowerAttackWaveDamage = 10f;
        [SerializeField]
        private float PowerAttackReach = 1.5f;
        [SerializeField]
        private float PowerAttackWaveRange = 5f;
        [SerializeField]
        private float PowerAttackWaveRadius = 1.0f;
        [SerializeField]
        private string PowerAttackHitPuff = "MagicHitPuff";
        [SerializeField]
        private string PowerAttackEffect = "PowerAttackEffect";

        [SerializeField, Header("Animations")]
        private WeaponFrame[] IdleAnimation = null;
        [SerializeField]
        private WeaponFrame[] AttackAnimation = null;
        [SerializeField]
        private WeaponFrame[] AttackAltAnimation = null;
        [SerializeField]
        private WeaponFrame[] PowerAttackAnimation = null;
        [SerializeField]
        private WeaponFrame[] BlockEnterAnimation = null;
        [SerializeField]
        private WeaponFrame[] BlockHoldAnimation = null;
        [SerializeField]
        private WeaponFrame[] BlockExitAnimation = null;

        [SerializeField, Header("Sounds")]
        private AudioSource AttackSound = null;
        [SerializeField]
        private AudioSource AltAttackSound = null;
        [SerializeField]
        private AudioSource PowerAttackSound = null;
               
        private float TimeToNext = 0;
        private float TimeToNextPowerAttack = 0;
        private bool LastAttackWasAlt = false;

        private WeaponFrame[] CurrentAnimation = null;
        private int CurrentFrameIndex;
        private float TimeInFrame = 0;

        private void Start()
        {
            if (PlayerController == null)
                PlayerController = GetComponentInParent<PlayerController>();

            if (ShootPoint == null)
                ShootPoint = transform;

            if (EffectSpawnPoint == null)
                EffectSpawnPoint = PlayerController.transform;

            if (WeaponImage == null)
                WeaponImage = GetComponentInChildren<Image>(); //probably unreliable

            CurrentAnimation = IdleAnimation;
        }

        private void Update()
        {
            if (Time.timeScale == 0 || LockPauseModule.IsPaused())
                return;

            if (PlayerController.PlayerInControl && !LockPauseModule.IsInputLocked())
            {
                HandleAttack();
                HandleAnimation();
                QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("LucidityAttackCharging", "charge", PowerAttackCharge)); //fuck it
            }
        }

        private void HandleAttack()
        {
            if (TimeToNext >= 0)
                TimeToNext -= Time.deltaTime;

            if (TimeToNextPowerAttack >= 0)
            {
                TimeToNextPowerAttack -= Time.deltaTime;
                if (MetaState.Instance.SessionFlags.Contains("BriellaIsABattleLesbian") || GameState.Instance.PlayerFlags.Contains("LucidityInstantCharge"))
                    TimeToNextPowerAttack = 0;
            }

            if(TimeToNext <= 0 && !GameState.Instance.PlayerFlags.Contains("LucidityNoWeapons")) //we have a separate flag because we use the other one to disable default weapons handling
            {
                if(MappedInput.GetButtonDown(CommonCore.Input.DefaultControls.AltFire) && TimeToNextPowerAttack <= 0)
                {
                    DoPowerAttack();
                    SpawnPowerAttackEffect();
                    TimeToNext = AttackInterval;
                    TimeToNextPowerAttack = PowerAttackInterval;
                    SetAnimationState(PowerAttackAnimation);
                    LastAttackWasAlt = false;
                    PowerAttackSound.Ref()?.Play();
                }
                else if (MappedInput.GetButtonDown(CommonCore.Input.DefaultControls.Fire))
                {
                    //attack!
                    DoMeleeAttack();
                    TimeToNext = AttackInterval;
                    SetAnimationState(LastAttackWasAlt ? AttackAnimation : AttackAltAnimation);
                    if (!LastAttackWasAlt && AltAttackSound != null)
                        AltAttackSound.Play();
                    else
                        AttackSound.Ref()?.Play();
                    LastAttackWasAlt = !LastAttackWasAlt;
                }
            }

        }

        

        private void DoMeleeAttack()
        {
            //TODO delay?
            var (otherController, hitPoint, hitLocation, hitMaterial) = WorldUtils.SpherecastAttackHit(ShootPoint.position, ShootPoint.forward, 0.4f, 3.0f, true, false, PlayerController);
            float distance = (hitPoint - ShootPoint.position).magnitude;

            if (distance <= AttackReach)
            {
                var hitInfo = new ActorHitInfo(AttackDamage, 0, 0, hitLocation, hitMaterial, PlayerController, AttackHitPuff, hitPoint);

                if (otherController is ITakeDamage itd)
                {
                    HitPuffScript.SpawnHitPuff(hitInfo);
                    itd.TakeDamage(hitInfo);
                }
            }

        }

        private void DoPowerAttack()
        {
            //power attack is hitscan, hopefully nobody will notice

            //do contact damage
            {
                var (otherController, hitPoint, hitLocation, hitMaterial) = WorldUtils.SpherecastAttackHit(ShootPoint.position, ShootPoint.forward, 0.4f, 3.0f, true, false, PlayerController);
                float distance = (hitPoint - ShootPoint.position).magnitude;

                if (distance <= PowerAttackReach)
                {
                    var hitInfo = new ActorHitInfo(PowerAttackContactDamage, 0, 0, hitLocation, hitMaterial, PlayerController, PowerAttackHitPuff, hitPoint);

                    if (otherController is ITakeDamage itd)
                    {
                        HitPuffScript.SpawnHitPuff(hitInfo);
                        itd.TakeDamage(hitInfo);
                    }
                }
            }

            //do wave damage
            {
                var hits = Physics.SphereCastAll(ShootPoint.position, PowerAttackWaveRadius, ShootPoint.forward, PowerAttackWaveRange, WorldUtils.GetAttackLayerMask(), QueryTriggerInteraction.Collide);

                foreach(var hit in hits)
                {
                    var hitbox = hit.collider.GetComponent<IHitboxComponent>();
                    if (hitbox != null && hitbox.ParentController != null && hitbox.ParentController != PlayerController && hitbox.ParentController is ITakeDamage hitboxITD)
                    {
                        var hitInfo = new ActorHitInfo(PowerAttackWaveDamage, 0, 0, hitbox.HitLocationOverride, hitbox.HitMaterial, PlayerController, PowerAttackHitPuff, hit.point);
                        HitPuffScript.SpawnHitPuff(hitInfo);
                        hitboxITD.TakeDamage(hitInfo);
                    }
                    else
                    {
                        var itd = hit.collider.GetComponent<ITakeDamage>();
                        if(itd != null && itd != PlayerController) //not a bug
                        {
                            var hitInfo = new ActorHitInfo(PowerAttackWaveDamage, 0, 0, 0, 0, PlayerController, PowerAttackHitPuff, hit.point);
                            HitPuffScript.SpawnHitPuff(hitInfo);
                            itd.TakeDamage(hitInfo);
                        }
                    }
                }
            }
        }

        private void SpawnPowerAttackEffect()
        {
            var go = WorldUtils.SpawnEffect(PowerAttackEffect, EffectSpawnPoint.position, transform.eulerAngles, CoreUtils.GetWorldRoot());
            if(go != null)
            {
                var effectScript = go.GetComponent<PowerAttackEffectScript>();
                if(effectScript != null)
                {
                    effectScript.MoveDistance = PowerAttackWaveRange;
                }
            }
        }

        private void HandleAnimation()
        {
            if (WeaponImage == null || CurrentAnimation == null || CurrentFrameIndex >= CurrentAnimation.Length)
            {
                //fail. Do nothing.
                return;
            }

            TimeInFrame += Time.deltaTime;// / Timescale;

            WeaponFrame currentFrame = CurrentAnimation[CurrentFrameIndex];

            if (TimeInFrame > currentFrame.Duration)
            {
                //advance to the next frame
                CurrentFrameIndex++;
                TimeInFrame = 0;
                if (CurrentFrameIndex == CurrentAnimation.Length)
                {
                    //we were on the last frame and are now past the end
                    if (CurrentAnimation == IdleAnimation) //loop
                    {
                        CurrentFrameIndex = 0;
                    }
                    else if (CurrentAnimation == AttackAnimation || CurrentAnimation == AttackAltAnimation || CurrentAnimation == PowerAttackAnimation) //autotransition
                    {
                        CurrentAnimation = IdleAnimation;
                        CurrentFrameIndex = 0;
                    }
                    else
                    {
                        return; //stop
                    }
                }

                if (CurrentAnimation != null && CurrentAnimation.Length > 0)
                {
                    WeaponImage.enabled = true;
                    WeaponImage.sprite = CurrentAnimation[CurrentFrameIndex].Sprite;
                }
                else
                {
                    Debug.LogError($"Can't set weapon frame for state because no frames exist");
                }
            }
        }

        private void SetAnimationState(WeaponFrame[] animation)
        {
            TimeInFrame = 0;
            CurrentFrameIndex = 0;

            CurrentAnimation = animation;

            if (CurrentAnimation != null && CurrentAnimation.Length > 0)
            {
                WeaponImage.enabled = true;
                WeaponImage.sprite = CurrentAnimation[CurrentFrameIndex].Sprite;
            }
            else
            {
                Debug.LogError($"Can't set weapon frame for state because no frames exist");
            }
        }

        public float PowerAttackCharge => (PowerAttackInterval - TimeToNextPowerAttack) / PowerAttackInterval;

        [Serializable]
        public struct WeaponFrame
        {
            public Sprite Sprite;
            public float Duration;
        }
    }
}