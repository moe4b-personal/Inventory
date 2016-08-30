using UnityEngine;

[System.Serializable]
[CreateAssetMenu()]
public class InventoryItem : ScriptableObject
{
    [SerializeField]
    new string name;
    [SerializeField]
    public string Name { get { return name; } }

    [SerializeField]
    string itemID;
    public string ItemID { get { return itemID; } }

    [SerializeField]
    Sprite sprite;
    public Sprite Sprite { get { return sprite; } internal set { sprite = value; } }

    [SerializeField]
    GameObject model;
    public GameObject Model { get { return model; } }

    [SerializeField]
    bool stackable;
    public bool Stackable { get { return stackable; } }

    [SerializeField]
    uint maxStack;
    public uint MaxStack { get { return maxStack; } }
}