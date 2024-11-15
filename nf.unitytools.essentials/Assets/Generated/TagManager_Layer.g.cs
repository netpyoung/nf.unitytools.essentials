
// ********************************************
// DO NOT EDIT THIS FILE! ( Auto generated )
// ********************************************
using System;
using System.ComponentModel;
using UnityEngine;

namespace Generated
{
public static partial class TagManager
{

public static class LayerMASK
{
public readonly static LayerMask DEFAULT = LayerMask.GetMask("Default");
public readonly static LayerMask TRANSPARENTFX = LayerMask.GetMask("TransparentFX");
public readonly static LayerMask IGNORE_RAYCAST = LayerMask.GetMask("Ignore Raycast");
public readonly static LayerMask WATER = LayerMask.GetMask("Water");
public readonly static LayerMask UI = LayerMask.GetMask("UI");
public readonly static LayerMask ASDF = LayerMask.GetMask("asdf");
}

public static class LayerINDEX
{
public readonly static int DEFAULT = LayerMask.NameToLayer("Default");
public readonly static int TRANSPARENTFX = LayerMask.NameToLayer("TransparentFX");
public readonly static int IGNORE_RAYCAST = LayerMask.NameToLayer("Ignore Raycast");
public readonly static int WATER = LayerMask.NameToLayer("Water");
public readonly static int UI = LayerMask.NameToLayer("UI");
public readonly static int ASDF = LayerMask.NameToLayer("asdf");
}

[Flags]
public enum E_LAYER_FLAG : int
{

[Description("Default")]
DEFAULT = 1 << 0,
[Description("TransparentFX")]
TRANSPARENTFX = 1 << 1,
[Description("Ignore Raycast")]
IGNORE_RAYCAST = 1 << 2,
[Description("Water")]
WATER = 1 << 4,
[Description("UI")]
UI = 1 << 5,
[Description("asdf")]
ASDF = 1 << 8,}
}}