﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    public abstract class BaseController : MonoBehaviour
    {
        //a gross hack to prevent overwriting
        //I call it the "work around unity" design pattern
        public string EditorFormID;
        public string FormID { get; private set; }
        public string[] EntityTags;

        public int HitMaterial = 0;

        public HashSet<string> Tags; //these are NOT unity tags!

        public virtual void Awake()
        {
            FormID = EditorFormID;
            
            if(FormID == name)
            {
                Debug.Log("FID: " + FormID + " TID: " + name);
                Debug.LogWarning("TID is the same as FID (did you forget to assign TID?)");
            }

            if(Tags == null)
            {
                Tags = new HashSet<string>();
            }

            if (EntityTags != null && EntityTags.Length > 0)
                Tags.UnionWith(EntityTags);
        }

        // Use this for initialization
        public virtual void Start()
        {
            //set Unity tag
            //if (tag == null || tag == "Untagged")
            //    tag = "CCEntity";
        }

        // Update is called once per frame
        public virtual void Update()
        {

        }

        //TODO rename GetExtraData and SetExtraData to Commit and Restore ?

        //save/restore methods
        //probably should have used properties but oh well
        public virtual Dictionary<string, System.Object> GetExtraData()
        {
            return null;
        }
        public virtual bool GetVisibility()
        {
            return true;
        }
        public virtual void SetExtraData(Dictionary<string, System.Object> data)
        {
            return;
        }
        public virtual void SetVisibility(bool visible)
        {
            return;
        }

    }
}