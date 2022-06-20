using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.Scripting;
using CommonCore.State;
using UnityEngine.UI;

namespace Lucidity
{

    public static class LucidityEasterEgg 
    {
        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.AfterMainMenuCreate)]
        public static void ApplyEasterEggTitlepic()
        {
            if (MetaState.Instance.SessionFlags.Contains("BriellaIsABattleLesbian"))
            {
                Debug.Log("I blame Snaden");
                var go = GameObject.Find("TitlePic");
                var rawImage = go.GetComponent<RawImage>();
                rawImage.texture = CoreUtils.LoadResource<Texture2D>("DynamicTextures/TITLEPIC2");
            }
        }

        [Command(alias = "BriellaIsABattleLesbian", useClassName = false)]
        public static void EnableEasterEggMode()
        {
            Debug.Log("So all of them look like anime fairys and yours looks like a battle lesbian");
            MetaState.Instance.SessionFlags.Add("BriellaIsABattleLesbian");
        }
    }
}