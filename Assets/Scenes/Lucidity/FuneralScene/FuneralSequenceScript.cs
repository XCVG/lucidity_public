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

namespace Lucidity.FuneralScene
{

    public class FuneralSequenceScript : MonoBehaviour
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
            AudioPlayer.Instance.SetMusic("sadness2", MusicSlot.Event, 0.5f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //show opening
            ScreenFader.FadeTo(Color.black, 0.01f, true, false, false);
            SetBackgroundImage("intro_graveyard");
            yield return null;
            ScreenFader.FadeFrom(Color.black, 1.0f, false, false, false);
            yield return SkippableWait.WaitForSeconds(6.0f);

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("PromiseFuneral", false, () => {
                doContinue = true;
            });
            //SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            //fadeout
            ScreenFader.FadeTo(Color.black, 3.0f, false, false, false);
            yield return new WaitForSeconds(3.0f);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);

            //set quest stage
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 30);

            //exit
            SharedUtils.ChangeScene("SwordScene");

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