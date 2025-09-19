using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    // So that when spawning a prefab, code knows which one is being spawned
    public ItemScriptableObject item;
    public bool isLitter = false;
}
