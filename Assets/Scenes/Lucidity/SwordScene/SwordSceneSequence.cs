using CommonCore;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.Messaging;
using CommonCore.RpgGame.Dialogue;
using CommonCore.State;
using CommonCore.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lucidity.SwordScene
{

    public class SwordSceneSequence : MonoBehaviour
    {

        [SerializeField]
        private Image BackgroundImage = null;

        private Coroutine CurrentCoroutine = null;

        private void Start()
        {
            StartNextSequence();
        }

        private void StartNextSequence()
        {
            int questStage = GameState.Instance.CampaignState.GetQuestStage("MainQuest");

            if(questStage <= 100)
            {
                StartTakeSequence(); //approach and try to take the sword
            }
            else if(questStage <= 110)
            {
                StartHistorySequence(); //the flashback
            }
            else if(questStage <= 120)
            {
                StartDreamSequence(); //the dream (will probably be omitted)
            }
            else if(questStage <= 130)
            {
                StartEndSequence(); //the end sequence
            }
        }


        private void StartTakeSequence()
        {
            /*
            DialogueInitiator.InitiateDialogue("", false, () => {
                GameState.Instance.CampaignState.SetQuestStage("MainQuest", 110);
                StartHistorySequence();
            });
            */


            CurrentCoroutine = StartCoroutine(CoTakeSequence());
        }

        private IEnumerator CoTakeSequence()
        {
            // set music and leave it
            AudioPlayer.Instance.SetMusic("tension1", MusicSlot.Event, 1.0f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event); //you need both calls because I fucked up the API design 2 years ago

            SetBackgroundImage("swordscene_greathall");
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Silvergleam. House Brukton's supposedly magic sword.", 10.0f)); //this is how subtitles are done, yes
            yield return SkippableWait.WaitForSeconds(10.0f);

            SetBackgroundImage("swordscene_armor");
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("I am the heir. It's my right as much as anyone's.", 10.0f));
            yield return SkippableWait.WaitForSeconds(10.0f);

            SetBackgroundImage("swordscene_closeup1");
            if (GameState.Instance.CampaignState.HasFlag("IntroDidDisappointFather") || GameState.Instance.CampaignState.HasFlag("IntroDidNotMissFather"))
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Father wouldn't have agreed. But he's dead.", 10.0f));
            else
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Or, it should be, anyway. Right?", 10.0f));
            yield return SkippableWait.WaitForSeconds(10.0f);

            SetBackgroundImage("swordscene_closeup2");
            AudioPlayer.Instance.PlaySound("cine_swordglow", SoundType.Sound, false);
            if(GameState.Instance.CampaignState.HasFlag("IntroDidDisappointFather") || GameState.Instance.CampaignState.HasFlag("IntroDidNotMissFather"))
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Really, I should have done this years ago.", 10.0f));
            else
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Okay. Deep breaths. Just a minor betrayal, no big deal.", 10.0f));
            yield return SkippableWait.WaitForSeconds(10.0f);

            ScreenFader.FadeTo(Color.black, 1.0f, false, false, false);
            yield return new WaitForSeconds(1.0f);

            //any cleanup if we need it
            SetBackgroundImage(null);
            //do not unset music, we will use it for the next sequence
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            //start the next sequence at the end
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 110);
            GameState.Instance.CampaignState.AddFlag("BriellaIsAMagicalGirl");
            CurrentCoroutine = null;
            StartNextSequence();

        }

        private void StartHistorySequence()
        {
            CurrentCoroutine = StartCoroutine(CoHistorySequence());
        }

        private IEnumerator CoHistorySequence()
        {
            ScreenFader.FadeFrom(Color.black, 2.0f, false, false, false);
            yield return null;
            SetBackgroundImage("swordscene_closeupold2");

            //AudioPlayer.Instance.PlaySound("cine_swordglow", SoundType.Sound, false);

            //yield return SkippableWait.WaitForSeconds(2.0f);
            yield return new WaitForSeconds(0.5f);

            //SetBackgroundImage("swordscene_closeupold");
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("4 years ago", 5.0f));
            yield return new WaitForSeconds(1.5f);
            yield return SkippableWait.WaitForSeconds(5.0f-1.5f);

            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));
            SetBackgroundImage(null);

            yield return null;

            bool doContinue = false;

            //throw the dialogue sequence up
            DialogueInitiator.InitiateDialogue("SwordHistory", false, () => {
                doContinue = true;
            });

            while (!doContinue)
            {
                yield return null; //yes, we seriously busywait for the dialogue to end. We did this a lot in Whistler, too
            }

            //the actual grab sequence (slide show, probably)
            ScreenFader.FadeTo(Color.black, 0.1f); //hack
            SetBackgroundImage("swordscene_closeup2");
            ScreenFader.FadeFrom(Color.black, 1.0f, false, false, false);
            yield return new WaitForSeconds(1.0f);            
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Here we go...", 7.0f));
            yield return SkippableWait.WaitForSeconds(7.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            AudioPlayer.Instance.StopMusic(MusicSlot.Event); //probably here

            //sword grab sequence
            AudioPlayer.Instance.PlaySound("cine_swordglow2", SoundType.Sound, false);
            SetBackgroundImage("swordscene_grab1");
            yield return new WaitForSeconds(0.33f);
            SetBackgroundImage("swordscene_grab2");
            yield return new WaitForSeconds(0.33f);
            SetBackgroundImage("swordscene_grab3");
            yield return new WaitForSeconds(0.5f);

            //Briella throw sequence
            //we may eventually replace this with an FMV sequence or something HAHA THATS NEVER GOING TO HAPPEN
            AudioPlayer.Instance.PlaySound("cine_swordthrow", SoundType.Sound, false);
            SetBackgroundImage("swordscene_throw0");
            yield return new WaitForSeconds(0.33f);

            SetBackgroundImage("swordscene_throw1");
            yield return new WaitForSeconds(0.1f);
            SetBackgroundImage("swordscene_throw2");
            yield return new WaitForSeconds(0.1f);
            SetBackgroundImage("swordscene_throw3");
            yield return new WaitForSeconds(0.1f);
            SetBackgroundImage("swordscene_throw4");
            yield return new WaitForSeconds(0.1f);
            SetBackgroundImage("swordscene_throw5");
            yield return new WaitForSeconds(0.1f);
            SetBackgroundImage("swordscene_throw6");
            yield return new WaitForSeconds(0.1f);

            AudioPlayer.Instance.PlaySound("cine_swordland", SoundType.Sound, false);
            SetBackgroundImage("swordscene_throw0");
            yield return new WaitForSeconds(0.2f);

            ScreenFader.FadeTo(Color.black, 0.1f, false, false, false);

            yield return new WaitForSeconds(2.0f);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event); //certainly here

            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 120);
            CurrentCoroutine = null;
            StartNextSequence();
        }

        private void StartDreamSequence()
        {
            Debug.Log("Skipped dream sequence!");
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 130);
            StartNextSequence();
        }

        private void StartEndSequence()
        {
            CurrentCoroutine = StartCoroutine(CoEndSequence());
        }

        private IEnumerator CoEndSequence()
        {
            //brief shot of Briella waking up, then we see Annike and possibly the guard captain (Timo)

            AudioPlayer.Instance.SetMusic("tension2", MusicSlot.Event, 1.0f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            ScreenFader.FadeFrom(null, 5.0f, false, false, false);
            SetBackgroundImage("swordscene_land1");
            yield return new WaitForSeconds(5.0f);

            //brief scene of Briella waking up
            //SetBackgroundImage("swordscene_land1");
            //yield return new WaitForSeconds(2.0f);
            SetBackgroundImage("swordscene_land2");
            AudioPlayer.Instance.PlaySound("cine_awake", SoundType.Sound, false);
            yield return new WaitForSeconds(3.0f);

            //then run a dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("SwordEnd", false, () => {
                doContinue = true;
            });
            SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            //go to the next scene
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 140);
            SharedUtils.ChangeScene("TrainingScene");
        }

        private void SetBackgroundImage(string background)
        {
            //holy fuck this is some halfassed code

            if(string.IsNullOrEmpty(background))
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
            catch(Exception e)
            {
                Debug.LogError($"Failed to set background image because of {e.GetType().Name}");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }
        }
    }
}