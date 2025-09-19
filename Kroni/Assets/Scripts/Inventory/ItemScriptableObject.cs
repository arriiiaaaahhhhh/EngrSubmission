using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "item", menuName = "ScriptableObjects/itemScriptableObject", order = 1)]
public class ItemScriptableObject : ScriptableObject
{
    public int id;
    public Vector2 size = Vector2.one; // default size is one by one instead of 0 by 0
    public Sprite sprite;
    public GameObject WorldPrefab;
}
