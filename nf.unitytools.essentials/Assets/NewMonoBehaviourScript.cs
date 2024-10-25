using UnityEngine;

using System;
using System.ComponentModel;
using Generated;
using NF.UnityTools.Essentials.DefineManagement;




public enum E_DEFINE
{
    A,
    B,
    C,
    D,
}


public enum E_DEFINE2
{
    A,
    B,
    C,
    D,
}

[UnityProjectDefine]
public enum E_DEFINE3
{
    AFFF,
    BFFF,
    CFFF,
    DFFF,
}
public class NewMonoBehaviourScript : MonoBehaviour
{
    [Description("A")]
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(TagManager.Tag.NEW_TAG);
    }

    // Update is called once per frame
    void Update()
    {
        //Generated.GenTags
        //GenTags

    }
}
