using CommonCore;
using CommonCore.Audio;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.World;
using CommonCore.State;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidity.WarBattleScene
{
    public class WarBattleSequenceScript : MonoBehaviour
    {
        [SerializeField]
        private float AfterKillTargetTime = 30f; //hold for this long after all bandits are dead
        [SerializeField]
        private float KillRatioForWin = 0.66f;
        [SerializeField]
        private GameObject[] DistantFireObjects = null;

        private List<BaseController> Enemies = new List<BaseController>();
        private int EnemiesToKill = 0;

        private bool AcknowledgedSufficientEnemiesDead = false;
        private bool SequenceEnding = false;
        private float TimeAfterKills = 0;

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
            //TODO the sequence

            //start music
            AudioPlayer.Instance.SetMusic("action3", MusicSlot.Ambient, 0.75f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Ambient);

            //DO NOT SPAWN EXTRA ENEMIES DURING GAMEPLAY IT WILL SHIT THE BED
            var bandits = WorldUtils.FindEntitiesWithTag("LucidityEnemy");
            Enemies.AddRange(bandits);
            EnemiesToKill = Mathf.RoundToInt((float)(bandits?.Count ?? 0) * KillRatioForWin); //what the fuck
            Debug.Log($"Found {bandits?.Count.ToString() ?? "null"} enemies, kill {EnemiesToKill} to win");

        }

        private void Update()
        {

            //TODO much the same as the village, 

            //we will try running this *every frame* and see if it works, if it doesn't we'll figure out something else

            if (SequenceEnding)
                return;

            //fuck efficiency, get a faster CPU
            if (AreSufficientEnemiesDead)
            {
                if(!AcknowledgedSufficientEnemiesDead)
                {
                    Debug.Log("Killed sufficient enemies!");

                    ShowDistantFires();
                    //MakeEnemiesRout(); //doesn't work
                    AcknowledgedSufficientEnemiesDead = true;
                }

                TimeAfterKills += Time.deltaTime;
                if (TimeAfterKills >= AfterKillTargetTime)
                {
                    StartSequenceEnd();
                }
            }
        }

        private void ShowDistantFires()
        {
            if(DistantFireObjects != null && DistantFireObjects.Length > 0)
            {
                foreach(var obj in DistantFireObjects)
                {
                    obj.SetActive(true);
                }
            }
        }

        private void MakeEnemiesRout()
        {
            //doesn't fucking work

            Debug.Log("Making enemies rout!");

            var playerT = WorldUtils.GetPlayerObject().transform;

            foreach (var enemy in Enemies)
            {
                var ac = enemy as ActorController;
                if (ac != null)
                {
                    if (ac.CurrentAiState == ActorAiState.Dead)
                        continue;

                    ac.Target = playerT;
                    ac.LockAiState = true;
                    ac.ForceEnterState(ActorAiState.Fleeing); //we enter the state but then they just fucking stand there and don't move
                }
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
            //just fade out and exit: keep it simple, stupid!

            yield return null;

            ScreenFader.FadeTo(Color.black, 5.0f, false, false, false);
            yield return new WaitForSeconds(5.0f);

            //stop music
            AudioPlayer.Instance.StopMusic(MusicSlot.Ambient);

            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 310);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.HideHud); //fix flag on exit
            SharedUtils.ChangeScene("WarVillageScene");
        }

        private bool AreSufficientEnemiesDead
        {
            get
            {

                int deadEnemies = 0;
                foreach (var enemy in Enemies)
                {
                    var ac = enemy as ActorController;
                    if (ac != null)
                    {
                        if (ac.CurrentAiState == ActorAiState.Dead)
                            deadEnemies++;
                    }
                }

                return deadEnemies >= EnemiesToKill;

            }
        }
    }
}