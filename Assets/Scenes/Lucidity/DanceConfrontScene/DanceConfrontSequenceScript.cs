using CommonCore;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.LockPause;
using CommonCore.RpgGame.Dialogue;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.World;
using CommonCore.State;
using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lucidity.DanceConfrontScene
{

    public class DanceConfrontSequenceScript : MonoBehaviour
    {
        //TODO sequence

        [SerializeField]
        private Image BackgroundImage = null;

        [SerializeField]
        private ActorController GarrickActor = null;
        [SerializeField]
        private Camera BackupCamera = null;

        private bool SequenceEnding = false;

        private void Awake()
        {
            //IMMEDIATELY inject flags

            //note: most of these aren't needed in the final game but are necessary for single-scene testing

            //GameState.Instance.PlayerFlags.Add(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoWeapons); //technically, we're unarmed
            //GameState.Instance.PlayerFlags.Add(PlayerFlags.HideHud);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.HideHud);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoFallDamage);
            GameState.Instance.CampaignState.AddFlag("BriellaIsAMagicalGirl"); //yes this is actually what the flag is called

        }

        private void Start()
        {
            //the sequence

            //start music
            AudioPlayer.Instance.SetMusic("tension3", MusicSlot.Event, 0.75f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //thing
            SetBackgroundVisibility(false);

            //opening conversation
            DialogueInitiator.InitiateDialogue("DanceConfrontBegin", true, () => {
                //doContinue = true;
                AudioPlayer.Instance.StopMusic(MusicSlot.Event);
                AudioPlayer.Instance.SetMusic("action5", MusicSlot.Ambient, 1.0f, true, false);
                AudioPlayer.Instance.StartMusic(MusicSlot.Ambient);
            });

        }

        private void Update()
        {
            if (SequenceEnding)
                return;

            if (GarrickActor.CurrentAiState == ActorAiState.Dead)
            {
                StartSequenceEnd();
            }
        }

        private void StartSequenceEnd()
        {
            Debug.Log("Starting end of sequence");

            SequenceEnding = true;

            //start the ending sequence
            StartCoroutine(CoSequenceEnd());
        }

        private IEnumerator CoSequenceEnd()
        {
            yield return null;

            //wait for the death animation
            GameState.Instance.PlayerFlags.Add(PlayerFlags.TotallyFrozen);            
            LockPauseModule.LockControls(InputLockType.GameOnly, this);            
            yield return new WaitForSeconds(1f);

            //LockPauseModule.PauseGame(this);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.HideHud);
            WorldUtils.GetPlayerObject().SetActive(false); //fuck it, nuclear option
            BackupCamera.gameObject.SetActive(true);
            SetBackgroundVisibility(true);

            //music
            AudioPlayer.Instance.SetMusic("tension3", MusicSlot.Event, 0.75f, true, false); //should we use victory music?
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("DanceConfrontEnd", true, () => {
                doContinue = true;
            });
            SetBackgroundImage("towerend_base");
            while (!doContinue)
            {
                yield return null;
            }

            //show the ending slides
            if(GameState.Instance.CampaignState.HasFlag("DanceConfrontDidKillRigan"))
            {
                //kill Garrick sequence
                yield return new WaitForSeconds(1f);
                AudioPlayer.Instance.PlaySound("cine_killgarrick", SoundType.Sound, false);
                yield return new WaitForSeconds(0.2f);
                SetBackgroundImage("towerend_kill1");
                yield return new WaitForSeconds(0.1f);
                SetBackgroundImage("towerend_kill2");

                yield return new WaitForSeconds(2f);
                SetBackgroundImage("towerend_kflag1");
                yield return new WaitForSeconds(0.2f);
                SetBackgroundImage("towerend_kflag2");
                yield return new WaitForSeconds(0.2f);
                SetBackgroundImage("towerend_kflag3");
                yield return new WaitForSeconds(0.2f);
                SetBackgroundImage("towerend_kflag4");
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                //don't kill Garrick sequence
                yield return new WaitForSeconds(1f);
                SetBackgroundImage("towerend_flag1");
                yield return new WaitForSeconds(0.2f);
                SetBackgroundImage("towerend_flag2");
                yield return new WaitForSeconds(0.2f);
                SetBackgroundImage("towerend_flag3");
                yield return new WaitForSeconds(0.2f);
                SetBackgroundImage("towerend_flag4");
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(1f);

            //fadeout
            ScreenFader.FadeTo(Color.black, 10.0f, false, false, false);
            yield return SkippableWait.WaitForSeconds(10f);

            LockPauseModule.UnlockControls(this);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.TotallyFrozen);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.HideHud);
            //LockPauseModule.UnpauseGame(this);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);

            //set quest stage
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 420); //420 BLAZE IT

            //exit
            SharedUtils.ChangeScene("EpilogueScene");
        }

        private void SetBackgroundVisibility(bool visibility)
        {
            BackgroundImage.enabled = visibility;
        }

        private void SetBackgroundImage(string background)
        {
            //holy fuck this is some halfassed code
            //and yes it is copy/pasted ~~3 or 4 times~~ 12 times

            if (string.IsNullOrEmpty(background))
            {
                BackgroundImage.color = Color.black;
                BackgroundImage.sprite = null;
                return;
            }

            try
            {
                var spr = CoreUtils.LoadResource<Sprite>("Dialogue/bg/" + background);
                if (spr == null)
                    throw new KeyNotFoundException();
                BackgroundImage.color = Color.white;
                BackgroundImage.sprite = spr;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set background image because of {e.GetType().Name}");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }
        }
    }
}