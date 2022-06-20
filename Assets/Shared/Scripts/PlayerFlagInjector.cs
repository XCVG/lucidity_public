using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.State;
using CommonCore.RpgGame;
using CommonCore.RpgGame.State;
using CommonCore.RpgGame.Rpg;

namespace Whistler
{

    //injects PlayerFlags for Whistler
    public class PlayerFlagInjector : MonoBehaviour
    {

        void Start()
        {
            //GameState.Instance.PlayerFlags.Add(PlayerFlags.Invulnerable); //that fuxxored a lot of things
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoWeapons);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.HideHud); //hidden in cutscenes; gameplay segments will set/unset this flag as needed
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoFallDamage);
        }


    }
}