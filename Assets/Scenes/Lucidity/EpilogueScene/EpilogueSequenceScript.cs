using CommonCore;
using CommonCore.Async;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.Input;
using CommonCore.Messaging;
using CommonCore.RpgGame.Dialogue;
using CommonCore.State;
using CommonCore.UI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Lucidity.EpilogueScene
{

    /// <summary>
    /// Sequence script for DanceOpenScene (tray bucket scene)
    /// </summary>
    public class EpilogueSequenceScript : MonoBehaviour
    {
        [SerializeField]
        private Image BackgroundImage = null;
        [SerializeField]
        private VideoPlayer VideoPlayer = null;

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
            var campaign = GameState.Instance.CampaignState; //we need this

            //do a bunch of deciding

            //calculate a ruthlessness value
            int ruthlessness = 0;
            {
                //start with positivity, aggression, and compassion vars
                ruthlessness -= campaign.GetVar<int>("UnleashedReflectPositivity");
                ruthlessness += campaign.GetVar<int>("WarEndAggression");
                ruthlessness -= campaign.GetVar<int>("WarEndCompassion");

                if (campaign.HasFlag("UnleashedDidDismissJohann"))
                    ruthlessness++;
                if (campaign.HasFlag("UnleashedDidBeArrogant"))
                    ruthlessness++;
                if (campaign.HasFlag("UnleashedWasReluctant"))
                    ruthlessness--;

                if (campaign.HasFlag("WarFormationNoGuards"))
                    ruthlessness--;
                if (campaign.HasFlag("WarFormationForward"))
                    ruthlessness -= 2;
                if (campaign.HasFlag("WarFormationRear"))
                    ruthlessness++;

                campaign.SetVar("OverallRuthlessness", ruthlessness);
            }

            //determine which ending to use
            //I wanted to have more nuanced endings but this was cut for time
            bool useBadEnding;            
            if (campaign.HasFlag("DanceConfrontDidKillRigan"))
            {
                //if you killed Rigan, you _always_ get the bad ending
                useBadEnding = true;
            }
            else if(campaign.HasFlag("DanceConfrontDidConvinceRigan"))
            {
                //if you talk down Rigan, you _always_ get the good ending
                useBadEnding = false;
            }
            else
            {
                //use ruthlessness value
                useBadEnding = (ruthlessness > 0);
            }

            if (useBadEnding)
                campaign.AddFlag("GotBadEnding");

            //calculate whether we will get Anneke's bonus ending also
            bool useAnnekeEnding;
            int inspiration = 0;
            {
                inspiration = campaign.GetVar<int>("AnnekeInspire");

                if (campaign.HasFlag("IntroDidNotMissFather"))
                    inspiration--;
                if (campaign.HasFlag("IntroReallyDidNotMissFather"))
                    inspiration--;

                if (campaign.HasFlag("UnleashedWasReluctant"))
                    inspiration--;

                if (campaign.HasFlag("WarFormationNoGuards"))
                    inspiration += 2;
                if (campaign.HasFlag("WarFormationForward"))
                    inspiration++;

                if (campaign.HasFlag("DanceConfrontDidKillRigan"))
                    inspiration -= 7; //it's still possible to get the bonus if you killed Rigan but it's a very, very slim margin

                campaign.SetVar("OverallInspiration", inspiration);
                useAnnekeEnding = (inspiration > 3); //
            }
            if (useAnnekeEnding)
                campaign.AddFlag("GotAnnekeEnding");

            Debug.Log($"Ruthlessness: {ruthlessness} ({(useBadEnding ? "BAD" : "GOOD")} ending) | Inspiration: {inspiration} ({(useAnnekeEnding ? "INSPIRED" : "UNINSPIRED")})");

            DoFinalSave();

            //play music
            AudioPlayer.Instance.SetMusic("ending", MusicSlot.Event, 0.7f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //show opening
            ScreenFader.FadeTo(Color.black, 0.01f, true, false, false);
            SetBackgroundImage(useBadEnding ? "ending_bad" : "ending_good");
            yield return null;
            ScreenFader.FadeFrom(Color.black, 5.0f, false, false, false);
            yield return new WaitForSeconds(5.0f);

            if (useBadEnding)
            {
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("When I was little, I loved all the stories about brave, noble knights. But I didn't want to be rescued by a knight or marry a knight. I wanted to <i>be</i> the knight.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("All my life I'd seen my future laid out for me. I was to marry a lord and bear him many sons and die trying. And I couldn't stand it.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);                
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("But I was never meant for that. I was always meant for something greater. I was meant to wield Silvergleam once again.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Lord Brukton isn't Sir Briella. But it's mine by right, and I'm going to be a good one.", 20.0f));                
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("All eyes are on me now, in Westerhold, the Kingdom and probably beyond. Things are going to get complicated.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("I know my path. I will use my powers to preserve and further my house. I'll show the world what this <i>scared little girl</i> is capable of.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
            }
            else
            {
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("All my life I'd seen my future laid out for me. I was to marry a lord and bear him many sons or die trying. And I couldn't stand it.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("When I was little, I loved all the stories about brave, noble knights. But I didn't want to be rescued by a knight or marry a knight. I wanted to <i>be</i> the knight.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Lord Brukton isn't Sir Briella. It's not a title I really wanted, and certainly not like this. But I'm giving it my best shot.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("I don't know why I can wield Silvergleam where so many failed. Maybe it's fate, maybe it's a fluke. Whatever it is, I can't waste it.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("All eyes are on me now, in Westerhold, the Kingdom and probably beyond. Things are going to get complicated.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
                QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("I know my path. I will use my powers to help those who can't help themselves. We can make a better world if we try.", 20.0f));
                yield return SkippableWait.WaitForSeconds(20.0f);
            }
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            //fade out
            ScreenFader.FadeTo(useBadEnding ? Color.black : Color.white, 5.0f, false, false, false);
            yield return new WaitForSeconds(5.0f);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);


            //show credits
            /*
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("CREDITS GO HERE, PRESS THE ANY KEY TO SHQIP", 10.0f));
            yield return SkippableWait.WaitForSeconds(10f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));
            */
            SetBackgroundImage(null);
            SetBackgroundVisibility(false);
            VideoPlayer.Play();
            ScreenFader.ClearFade();            
            float skipElapsed = 0;
            while((!VideoPlayer.isPrepared || VideoPlayer.isPlaying) && !(skipElapsed > 2f))
            {
                if (MappedInput.GetButton(CommonCore.Input.DefaultControls.Use) || MappedInput.GetButton(CommonCore.Input.DefaultControls.Fire) || MappedInput.GetButton(CommonCore.Input.DefaultControls.Submit))
                    skipElapsed += Time.deltaTime;
                else
                    skipElapsed = 0;

                yield return null;
                //wait for video to end
            }
            if (VideoPlayer.isPlaying) //in case of skip or bug
                VideoPlayer.Stop();
            SetBackgroundVisibility(true);

            //show epilogue, if we have an epilogue
            if (useAnnekeEnding)
            {
                //anneke ending sequence

                AudioPlayer.Instance.SetMusic("stinger", MusicSlot.Event, 0.7f, true, false);
                AudioPlayer.Instance.StartMusic(MusicSlot.Event);
                
                ScreenFader.FadeFrom(Color.black, 3.0f, false, false, false);
                SetBackgroundImage("stinger_hallempty");
                yield return new WaitForSeconds(3.0f);
                
                yield return SkippableWait.WaitForSeconds(5f);

                SetBackgroundImage("stinger_hall");
                yield return SkippableWait.WaitForSeconds(5f);

                SetBackgroundImage("stinger_approach");
                yield return SkippableWait.WaitForSeconds(5f);

                SetBackgroundImage("stinger_reach");
                AudioPlayer.Instance.PlaySound("cine_swordglow", SoundType.Sound, false);
                //yield return SkippableWait.WaitForSeconds(5f);
                yield return new WaitForSeconds(2f);

                //QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("ANNEKE ENDING SEQUENCE GOES HERE, PRESS THE ANY KEY TO SHQIP", 10.0f));
                //yield return SkippableWait.WaitForSeconds(10f);
                //QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));
            }

            //cutoff and brief wait
            //ScreenFader.FadeTo(Color.black, 0.2f, false, false, false);
            AudioPlayer.Instance.StopMusic(MusicSlot.Event);
            yield return new WaitForSeconds(1f);
            SetBackgroundImage("ending_last");
            yield return SkippableWait.WaitForSeconds(5f);
            SetBackgroundImage(null);
            yield return new WaitForSeconds(1f);

            //end the game
            SharedUtils.EndGame();
        }

        private void SetBackgroundVisibility(bool visibility)
        {
            BackgroundImage.enabled = visibility;
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

        private void DoFinalSave()
        {
            try
            {
                //do the final save
                string finalSaveString = JsonConvert.SerializeObject(new { Version = CoreParams.GameVersion, CampaignIdentifier = GameState.Instance.CampaignIdentifier, CampaignState = GameState.Instance.CampaignState }, new JsonSerializerSettings() { Converters = CCJsonConverters.Defaults.Converters, Formatting = Formatting.Indented, TypeNameHandling = TypeNameHandling.Auto });

                string finalSaveName = $"finalsave_{DateTime.UtcNow.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture)}";

                //oh god what the actual fuck is going on here
                AsyncUtils.RunWithExceptionHandling(async() =>
               {

                   await Task.Run(async() =>
                   {
                       Directory.CreateDirectory(Path.Combine(CoreParams.PersistentDataPath, "finalsave"));

                       await Task.Yield();
                      
                       File.WriteAllText(Path.Combine(CoreParams.PersistentDataPath, "finalsave", $"{finalSaveName}.json"), finalSaveString);
                   });
               });
                
            }
            catch(Exception e)
            {
                Debug.LogError($"Final save failed because of {e.GetType().Name}");
                Debug.LogException(e);
            }
        }
    }
}