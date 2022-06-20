using CommonCore;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.Messaging;
using CommonCore.RpgGame;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.StringSub;
using CommonCore.UI;
using CommonCore.World;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lucidity
{
    public class LucidityHUDController : BaseHUDController
    {
        [Header("Top Bar")]
        public Text TargetText;
        public Slider TargetHealthLeft;
        public Slider TargetHealthRight;

        [Header("Left Bar")]
        public Slider HealthSlider;
        public Text HealthText;
        public Image HealthFillArea;

        public Slider ShieldSlider;
        public Text ShieldText;

        public Slider EnergySlider;
        public Text EnergyText;
        public Image EnergyFillArea;

        [Header("Lucidity")]
        public Graphic HealthBar;
        public Graphic DashIcon;
        public Graphic AttackIcon;
        public Color DepletedColor;
        public Color ChargedColor;
        public Color ReadyColor;
        public float ShieldDepletedThreshold = 0.25f; //yeah we don't actually retrieve this from LucidityDamageHandler at all

        [Header("Misc")]
        public Canvas Canvas;
        public Image Crosshair;
        public Image HitIndicator;
        public Image DamageFadeOverlay;
        public CanvasGroup DebugGroup;

        [Header("Options")]
        public float DamageFadeMin;
        public float DamageFadeMax;
        public float DamageFadeFactor;
        public float DamageFadeRate;

        //local state is, as it turns out, unavoidable
        private string OverrideTarget = null;


        private float? LastTriggeredHealthFraction = null;
        private float LastFrameHealthFraction = 1;
        private Color? HealthOriginalColor = null;
        private Coroutine HealthFlashCoroutine = null;

        private Color? EnergyOriginalColor = null;
        private Coroutine EnergyFlashCoroutine = null;

        private float DamageFadeIntensityTarget = 0;

        private Coroutine HitIndicatorFlashCoroutine = null;

        private bool AllowDamageFlash = false;

        //Lucidity hackery
        private float AttackCharge = 0;
        private float DashCharge = 0;

        protected override void Start()
        {
            base.Start();

            LastTriggeredHealthFraction = GameState.Instance.PlayerRpgState.HealthFraction; //okayish
            LastFrameHealthFraction = LastTriggeredHealthFraction.Value;
            UpdateStatusDisplays();

            ClearTarget();
            UpdateDebugHud();
        }
        
        protected override void Update()
        {
            //this is all slow and dumb and temporary... which means it'll probably be untouched until Ferelden
            base.Update();

            UpdateVisibility();
            
            UpdateStatusDisplays();
            UpdateDamageFade();
            //UpdateDebugHud();
            UpdateLucidity();
            LastFrameHealthFraction = GameState.Instance.PlayerRpgState.HealthFraction;
        }

        protected override bool HandleMessage(QdmsMessage message)
        {
            if(base.HandleMessage(message))
            {
                return true;
            }
            else if(message is HUDPushMessage)
            {
                AppendHudMessage(Sub.Macro(((HUDPushMessage)message).Contents));
                return true;
            }
            else if(message is ConfigChangedMessage)
            {
                SetDamageFadeVisibility();
                return true;
            }
            else if(message is QdmsKeyValueMessage kvmessage)
            {
                switch (kvmessage.Flag)
                {
                    case "LucidityAttackCharging":
                        AttackCharge = kvmessage.GetValue<float>("charge");
                        break;
                    case "LucidityDashCharging":
                        DashCharge = kvmessage.GetValue<float>("charge");
                        break;
                    case "PlayerHasTarget":
                        SetTargetMessage(kvmessage.GetValue<string>("Target"));
                        break;
                    case "RpgBossHealthUpdate":
                        UpdateTargetOverrideHealth(kvmessage.GetValue<string>("Target"), kvmessage.GetValue<float>("Health"));
                        break;
                    case "RpgBossAwake":
                        SetTargetOverride(kvmessage.GetValue<string>("Target"));
                        break;
                    case "RpgBossDead":
                        ClearTargetOverride(kvmessage.GetValue<string>("Target"));
                        break;
                }

                return true; //probably the wrong spot
            }
            else if(message is QdmsFlagMessage flagmessage)
            {
                switch (flagmessage.Flag)
                {
                    case "LucidityPlayerStartedHealing":
                        AllowDamageFlash = false;
                        ClearDamageFade();
                        break;
                    case "LucidityPlayerHit":
                        AllowDamageFlash = false; //we are over the pain threshold
                        FlashHitIndicator(false);
                        break;
                    case "LucidityPlayerHitPenetrated":
                        AllowDamageFlash = true; //we are under the pain threshold
                        FlashHitIndicator(true);
                        break;
                    case "PlayerChangeView":
                        SetCrosshair(message);
                        break;
                    case "PlayerClearTarget":
                        ClearTarget();
                        break;
                    case "HudEnableCrosshair":
                        Crosshair.enabled = true;
                        break;
                    case "HudDisableCrosshair":
                        Crosshair.enabled = false;
                        break;
                }

                return true;
            }

            return false;

        }

        private void SetCrosshair(QdmsMessage message)
        {
            //we actually don't care much if this fails
            //it'll throw an ugly exception but won't break anything

            //I think this is redundant now (?)

            var newView = ((QdmsKeyValueMessage)(message)).GetValue<PlayerViewType>("ViewType");
            if (newView == PlayerViewType.ForceFirst || newView == PlayerViewType.PreferFirst)
                Crosshair.gameObject.SetActive(true);
            else if(newView == PlayerViewType.ForceThird || newView == PlayerViewType.PreferThird)
                Crosshair.gameObject.SetActive(false);
            else
                Crosshair.gameObject.SetActive(false);
        }

        private void UpdateDamageFade()
        {
            if (!ConfigState.Instance.FlashEffects || !ConfigState.Instance.GetGameplayConfig().FullscreenDamageIndicator)
                return;

            Color damageFadeCurrentColor = DamageFadeOverlay.color;
            float damageFadeCurrentIntensity = damageFadeCurrentColor.a;
            if (DamageFadeIntensityTarget > 0 || (DamageFadeIntensityTarget == 0 && damageFadeCurrentIntensity > 0))
            {
                //make it a more intense red if we keep taking damage
                //if(DamageFadeIntensityTarget > 0)
                {
                    float healthLost = LastFrameHealthFraction - GameState.Instance.PlayerRpgState.HealthFraction;
                    if (!Mathf.Approximately(healthLost, 0))
                    {
                        float extraIntensity = Mathf.Clamp(DamageFadeFactor * healthLost, 0, DamageFadeMax);
                        DamageFadeIntensityTarget = Mathf.Clamp(DamageFadeIntensityTarget + extraIntensity, DamageFadeMin, DamageFadeMax);
                    }
                }

                //fade toward target
                float sign = Mathf.Sign(DamageFadeIntensityTarget - damageFadeCurrentIntensity);
                float newIntensity = Mathf.Clamp(damageFadeCurrentIntensity + sign * DamageFadeRate * Time.deltaTime, 0, 1);
                DamageFadeOverlay.color = new Color(damageFadeCurrentColor.r, damageFadeCurrentColor.g, damageFadeCurrentColor.b, newIntensity);

                //start the fade back
                if (DamageFadeIntensityTarget > 0 && newIntensity >= DamageFadeIntensityTarget)
                    DamageFadeIntensityTarget = 0;
            }            
                    
        }

        private void ClearDamageFade()
        {
            //Debug.Log("Cleared damage fade");
            Color damageFadeCurrentColor = DamageFadeOverlay.color;
            DamageFadeIntensityTarget = 0;
            DamageFadeOverlay.color = new Color(damageFadeCurrentColor.r, damageFadeCurrentColor.g, damageFadeCurrentColor.b, 0);
        }

        private void StartDamageFade(float healthLost)
        {
            //note that healthlost is positive and fractional
            DamageFadeIntensityTarget = Mathf.Clamp(DamageFadeFactor * healthLost, DamageFadeMin, DamageFadeMax);
            //Debug.LogWarning("Set DamageFadeIntensityTarget to " + DamageFadeIntensityTarget);
        }

        private void SetDamageFadeVisibility()
        {
            if (!ConfigState.Instance.FlashEffects)
            {
                Color oldColor = DamageFadeOverlay.color;
                DamageFadeOverlay.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0);
            }
        }

        private void UpdateStatusDisplays()
        {
            var player = GameState.Instance.PlayerRpgState;
            HealthText.text = Mathf.Max(0, player.Health).ToString("f0");
            if (LastTriggeredHealthFraction.HasValue && LastTriggeredHealthFraction.Value < player.HealthFraction && HealthFlashCoroutine == null)
                LastTriggeredHealthFraction = player.HealthFraction; //handle healing
            float healthLost = (LastTriggeredHealthFraction ?? HealthSlider.value) - player.HealthFraction;
            if (healthLost > GameParams.DamageFlashThreshold && AllowDamageFlash)
            {
                //FlashHealthBar();
                StartDamageFade(healthLost);
            }
            HealthSlider.value = player.HealthFraction;

            EnergyText.text = player.Energy.ToString("f0");
            EnergySlider.value = player.EnergyFraction;

            //null out the shields for now
            ShieldText.text = "";
            ShieldSlider.value = 0;

            //handle health flashing
            /*
            if(TimeSinceLastHealthSample > GameParams.DamageFlashSamplePeriod)
            {
                TimeSinceLastHealthSample = 0;
                if(LastHealthSampleValue.HasValue)
                {
                    if (LastHealthSampleValue.Value - player.HealthFraction > GameParams.DamageFlashThreshold)
                        FlashHealthBar();
                }
                LastHealthSampleValue = player.HealthFraction;
            }
            TimeSinceLastHealthSample += Time.deltaTime;
            */
        }

        private void UpdateVisibility()
        {
            if(GameState.Instance.PlayerFlags.Contains(PlayerFlags.HideHud))
            {
                if (Canvas.enabled)
                    Canvas.enabled = false;
            }
            else
            {
                if (!Canvas.enabled)
                    Canvas.enabled = true;
            }
        }

        //flash our fancy new Lucidity hit indicator
        private void FlashHitIndicator(bool penetrated)
        {
            //nop, we didn't get to it obviously
        }

        //update Lucidity HUD elements
        private void UpdateLucidity()
        {
            //health/shield bar
            float healthFraction = GameState.Instance.PlayerRpgState.HealthFraction;
            float correctedHealthFraction = Mathf.Max(0, healthFraction - ShieldDepletedThreshold) / (1.0f - ShieldDepletedThreshold);
            Color healthBarColor = Color.Lerp(DepletedColor, ReadyColor, correctedHealthFraction);
            HealthBar.color = healthBarColor;

            //attack icon
            if(AttackCharge >= 1f)
            {
                AttackIcon.color = ReadyColor;
            }
            else
            {
                Color attackIconColor = Color.Lerp(DepletedColor, ChargedColor, AttackCharge);
                AttackIcon.color = attackIconColor;
            }

            //dash icon
            if (DashCharge >= 1f)
            {
                DashIcon.color = ReadyColor;
            }
            else
            {
                Color dashIconColor = Color.Lerp(DepletedColor, ChargedColor, DashCharge);
                DashIcon.color = dashIconColor;
            }
        }

        private void UpdateDebugHud()
        {
            if (ConfigState.Instance.HasCustomFlag("UseDebugHud"))
            {
                DebugGroup.alpha = 1;
            }
            else
            {
                DebugGroup.alpha = 0;
            }
        }

        //handle target text, override target text, health bar

        private void ClearTarget()
        {
            TargetText.text = string.Empty;

            if (!string.IsNullOrEmpty(OverrideTarget))
                TargetText.text = OverrideTarget;
        }

        private void SetTargetMessage(string message)
        {
            if (!string.IsNullOrEmpty(OverrideTarget))
                return;

            TargetText.text = message;
        }

        private void SetTargetOverride(string overrideTarget)
        {
            OverrideTarget = overrideTarget;
            TargetText.text = overrideTarget;

            TargetHealthLeft.gameObject.SetActive(true);
            TargetHealthRight.gameObject.SetActive(true);
            TargetHealthLeft.value = 1;
            TargetHealthRight.value = 1;
        }

        private void UpdateTargetOverrideHealth(string overrideTarget, float health)
        {
            if (OverrideTarget == null || OverrideTarget != overrideTarget)
            {
                Debug.LogWarning($"[{nameof(LucidityHUDController)}] Updated override target health for a different target than expected (old: \"{OverrideTarget}\", new: \"{overrideTarget}\")");
                SetTargetOverride(overrideTarget);                
            }

            TargetHealthLeft.value = health;
            TargetHealthRight.value = health;
        }

        private void ClearTargetOverride(string overrideTarget)
        {
            if (OverrideTarget == null || OverrideTarget != overrideTarget)
            {
                Debug.LogWarning($"[{nameof(LucidityHUDController)}] Cleared override target for a different target than expected (old: \"{OverrideTarget}\", new: \"{overrideTarget}\")");
            }

            OverrideTarget = null;
            ClearTarget();

            TargetHealthLeft.gameObject.SetActive(false);
            TargetHealthRight.gameObject.SetActive(false);
        }
    }
}