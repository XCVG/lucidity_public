using CommonCore.LockPause;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.RpgGame.World;
using CommonCore.Input;
using CommonCore;
using CommonCore.World;
using CommonCore.Messaging;
using CommonCore.State;
using CommonCore.RpgGame.Rpg;

namespace Lucidity
{

    /// <summary>
    /// Handles flash step and power jump abilities
    /// </summary>
    public class LucidityMovementComponent : MonoBehaviour
    {
        private const float MoveDeadzone = 0.1f;

        [SerializeField, Header("Components")]
        private PlayerController PlayerController;
        [SerializeField]
        private PlayerMovementComponent MovementComponent;

        [SerializeField, Header("Options")]
        private float RechargeTime = 5f;

        [SerializeField, Header("Jump")]
        private Vector3 JumpInstantVelocity = Vector3.zero;
        [SerializeField]
        private Vector3 JumpAcceleration = new Vector3(0, 20f, 0.5f);
        [SerializeField]
        private float JumpTime = 0.5f;

        [SerializeField, Header("Dash")]
        private Vector3 DashInstantVelocity = Vector3.zero;
        [SerializeField]
        private Vector3 DashAcceleration = new Vector3(0, 0, 20f);
        [SerializeField]
        private float DashTime = 0.5f;
        [SerializeField]
        private float DashMaxVelocity = 50f;

        [SerializeField, Header("Dash Damage")]
        private float DashPushForce = 20f;
        [SerializeField]
        private float DashPushAngle = 45f;
        [SerializeField]
        private float DashPushDamage = 1f;
        [SerializeField]
        private float DashPushRadius = 2f;

        [SerializeField, Header("Effects")]
        private AudioSource JumpSound = null;
        [SerializeField]
        private string JumpEffect = null;
        [SerializeField]
        private AudioSource DashSound = null;
        [SerializeField]
        private string DashEffect = null;

        public PushState CurrentState { get; private set; }
        public float TimeToNext { get; private set; }
        private float TimeLeftInState;

        //TODO CONCEPTUAL should we use move vector or facing vector?

        private void Start()
        {
            if (PlayerController == null)
                PlayerController = GetComponent<PlayerController>();

            if (MovementComponent == null)
                MovementComponent = GetComponent<PlayerMovementComponent>();
        }

        private void Update()
        {
            if (Time.timeScale == 0 || LockPauseModule.IsPaused())
                return;

            if (PlayerController.PlayerInControl && !LockPauseModule.IsInputLocked())
            {
                HandleAbilities();                
            }

            HandlePush();

            QdmsMessageBus.Instance.PushBroadcast(new QdmsKeyValueMessage("LucidityDashCharging", "charge", DashCharge));
        }

        private void HandleAbilities()
        {
            if (CurrentState != PushState.Idle)
                return;

            if (TimeToNext > 0)
            {
                TimeToNext -= Time.deltaTime;

                if (MetaState.Instance.SessionFlags.Contains("BriellaIsABattleLesbian") || GameState.Instance.PlayerFlags.Contains("LucidityInstantCharge"))
                    TimeToNext = 0;
            }
            else
            {
                if (!GameState.Instance.PlayerFlags.Contains(PlayerFlags.Frozen) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.TotallyFrozen))
                {
                    if (MappedInput.GetButtonDown(DefaultControls.Sprint))
                    {
                        if (MappedInput.GetButton(DefaultControls.Jump))
                        {
                            //power jump
                            DoPowerJump();
                        }
                        else
                        {
                            Vector2 moveVector = new Vector2(MappedInput.GetAxis(DefaultControls.MoveX), MappedInput.GetAxis(DefaultControls.MoveY));
                            if (moveVector.magnitude > MoveDeadzone)
                            {
                                DoPowerDash();
                            }
                        }
                    }
                    else if (MappedInput.GetButton(DefaultControls.Sprint) && MappedInput.GetButtonDown(DefaultControls.Jump))
                    {
                        DoPowerJump();
                    }
                }
            }
        }

        private void DoPowerJump()
        {
            TimeToNext = RechargeTime;
            TimeLeftInState = JumpTime;
            MovementComponent.Velocity += Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * JumpInstantVelocity;
            CurrentState = PushState.PushingUp;
            JumpSound.Ref()?.Play();
            SpawnEffect(JumpEffect);
        }

        private void DoPowerDash()
        {
            //Debug.Log("POWER DASH!");
            //power dash
            TimeToNext = RechargeTime;
            TimeLeftInState = DashTime;
            MovementComponent.UseBraking = false;
            MovementComponent.Velocity += Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * DashInstantVelocity;
            CurrentState = PushState.PushingForward;
            DashSound.Ref()?.Play();
            SpawnEffect(DashEffect);
        }

        

        private void HandlePush()
        {
            if (CurrentState == PushState.Idle)
                return;

            if(TimeLeftInState <= 0)
            {
                TimeLeftInState = 0;
                CurrentState = PushState.Idle;
                MovementComponent.UseBraking = true;
                return;
            }

            //handle pushing the player around
            if(CurrentState == PushState.PushingUp)
            {
                MovementComponent.Velocity += Time.deltaTime * (Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * JumpAcceleration);
            }
            else if(CurrentState == PushState.PushingForward)
            {
                MovementComponent.Velocity += Time.deltaTime * (Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * DashAcceleration);

                //clamp max velocity since we've disabled this on the movementcomponent
                Vector2 moveFlatVec = new Vector2(MovementComponent.Velocity.x, MovementComponent.Velocity.z);
                float moveMagnitude = Mathf.Min(moveFlatVec.magnitude, DashMaxVelocity);
                var moveFlatDir = moveFlatVec.normalized;
                Vector2 clampedMoveVec = moveMagnitude * moveFlatDir;
                MovementComponent.Velocity = new Vector3(clampedMoveVec.x, MovementComponent.Velocity.y, clampedMoveVec.y);

                //handle push force/damage
                var colliders = Physics.OverlapSphere(transform.position, DashPushRadius, WorldUtils.GetAttackLayerMask(), QueryTriggerInteraction.Collide);
                foreach(var collider in colliders)
                {
                    var hitbox = collider.GetComponent<IHitboxComponent>();
                    ITakeDamage itd = null;
                    if(hitbox != null && hitbox.ParentController != null && hitbox.ParentController != PlayerController)
                    {
                        itd = hitbox.ParentController as ITakeDamage;
                    }
                    else
                    {
                        var controller = collider.GetComponent<BaseController>();
                        if (controller != PlayerController)
                            itd = controller as ITakeDamage;
                    }

                    if(itd != null)
                    {
                        itd.TakeDamage(new ActorHitInfo(DashPushDamage, 0, 0, 0, 0, PlayerController));

                        if(itd is BaseController bc && !bc.Tags.Contains("Unpushable"))
                        {
                            //try the simplest thing first: push *away*
                            Vector3 vecToTarget = collider.transform.position - transform.position;
                            Vector3 dirToTarget = vecToTarget.normalized;
                            Vector2 flatVecToTarget = new Vector2(dirToTarget.x, dirToTarget.z);

                            var targetTransform = ((MonoBehaviour)itd).transform;
                            targetTransform.Translate(new Vector3(flatVecToTarget.x, 0, flatVecToTarget.y) * Time.deltaTime * DashPushForce, Space.World);
                        }
                    }
                }
            }

            TimeLeftInState -= Time.deltaTime;
        }

        private void SpawnEffect(string effect)
        {
            if (string.IsNullOrEmpty(effect))
                return;

            WorldUtils.SpawnEffect(effect, transform.position, transform.eulerAngles, transform);
        }

        public float DashCharge => (RechargeTime - TimeToNext) / RechargeTime;

        public enum PushState
        {
            Idle, PushingForward, PushingUp
        }


    }

    
}