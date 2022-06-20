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

namespace Lucidity.WarVillageScene
{

    /// <summary>
    /// After-battle village sequence. It's a bit fucky for two reasons:
    ///     - The in-engine rout sequence didn't work so we had to do it as a graphic
    ///     - There was supposed to be a conversation, but there _no guarantee there's anyone with you to converse with_
    /// </summary>
    public class WarVillageSequenceScript : MonoBehaviour
    {
        [SerializeField]
        private Image BackgroundImage = null;

        private Coroutine CurrentCoroutine = null;

        private void Start()
        {

            StartSequence();
        }

        private void StartSequence()
        {
            CurrentCoroutine = StartCoroutine(CoSequence());
        }

        private IEnumerator CoSequence()
        {
            //play music
            AudioPlayer.Instance.SetMusic("victory", MusicSlot.Event, 0.5f, true, false); //reuse tension1 for now
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //show rout
            ScreenFader.FadeTo(Color.black, 0.01f, true, false, false);
            SetBackgroundImage("war_rout");
            yield return null;
            ScreenFader.FadeFrom(Color.black, 1.0f, false, false, false);
            if(GameState.Instance.CampaignState.HasFlag("WarFormationNoGuards"))
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("I routed them...", 5.0f));
            else
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("We routed them...", 5.0f));
            yield return SkippableWait.WaitForSeconds(5.0f);

            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Wait, are those fires? Oh no...", 7.0f));
            yield return SkippableWait.WaitForSeconds(7.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            //fade out, swap music
            ScreenFader.FadeTo(Color.black, 1.0f, false, false, false);
            yield return new WaitForSeconds(1.5f);
            SetBackgroundImage("war_village");
            AudioPlayer.Instance.SetMusic("sadness", MusicSlot.Event, 0.7f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);
            ScreenFader.FadeFrom(null, 1.0f, false, false, false);
            yield return new WaitForSeconds(1.0f);

            //village dialogues
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("The village...", 5.0f));
            yield return SkippableWait.WaitForSeconds(5.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("They burned the village...", 7.0f));
            yield return SkippableWait.WaitForSeconds(7.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            //fadeout
            ScreenFader.FadeTo(Color.black, 3.0f, false, false, false);
            yield return new WaitForSeconds(3.0f);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);

            //set quest stage
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 330);

            //exit
            SharedUtils.ChangeScene("WarQuietScene");

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