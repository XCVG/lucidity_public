using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidity
{

    public class InjectCutsceneHudScript : MonoBehaviour
    {
        private void Awake()
        {
            Instantiate(CoreUtils.LoadResource<GameObject>("UI/DefaultCutsceneHud"), CoreUtils.GetUIRoot());
            Destroy(gameObject);
        }
    }
}