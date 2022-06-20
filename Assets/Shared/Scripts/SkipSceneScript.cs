using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skips a scene and goes to another
/// </summary>
public class SkipSceneScript : MonoBehaviour
{
    [SerializeField]
    private string NextScene;

    void Start()
    {
        StartCoroutine(CoSkipScene());
    }

    private IEnumerator CoSkipScene()
    {
        //wait two frames, GOTO next scene

        yield return null;
        yield return null;
        //I forget why we wait two frames but there was a reason for it

        SharedUtils.ChangeScene(NextScene);
    }

}
