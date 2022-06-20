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

namespace Lucidity.DanceOpenScene
{

    /// <summary>
    /// Sequence script for DanceOpenScene (tray bucket scene)
    /// </summary>
    public class DanceOpenSequenceScript : MonoBehaviour
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
            AudioPlayer.Instance.SetMusic("war2", MusicSlot.Event, 0.5f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //show opening
            ScreenFader.FadeTo(Color.black, 0.01f, true, false, false);
            SetBackgroundImage("launch_closeup");
            yield return null;
            ScreenFader.FadeFrom(Color.black, 5.0f, false, false, false);
            yield return new WaitForSeconds(5.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Are you sure this is going to work?", 5.0f));
            yield return SkippableWait.WaitForSeconds(5.0f);

            SetBackgroundImage("launch_map");
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Well, sure. You've just got to make it to the keep in the middle.", 10.0f));
            yield return SkippableWait.WaitForSeconds(10.0f);

            //show inside
            SetBackgroundImage("launch_trebuchet");
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("I wasn't talking about that part of the plan", 7.0f));
            yield return SkippableWait.WaitForSeconds(7.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("DanceOpen", false, () => {
                doContinue = true;
            });
            //SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            //launch animation

            yield return new WaitForSeconds(1f);
            AudioPlayer.Instance.PlaySound("cine_trebuchet", SoundType.Any, true);
            SetBackgroundImage("launch_anim1");
            yield return new WaitForSeconds(0.2f);
            SetBackgroundImage("launch_anim2");
            yield return new WaitForSeconds(0.2f);
            SetBackgroundImage("launch_anim3");
            yield return new WaitForSeconds(0.2f);
            SetBackgroundImage("launch_anim4");
            yield return new WaitForSeconds(0.5f);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);

            //set quest stage
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 400);

            //exit
            SharedUtils.ChangeScene("DanceCastleScene");

        }

        private void SetBackgroundImage(string background)
        {
            //holy fuck this is some halfassed code
            //and yes it is copy/pasted 3 or 4 times

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