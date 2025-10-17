using UnityEngine;
using System.Collections.Generic;

// 단서 아이템을 나타내는 클래스

[System.Serializable]
public class ClueItem : IItem
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public bool IsClue => true;

    public ClueItem(string id, string name, string desc)
    {
        itemId = id;
        itemName = name;
        description = desc;
    }
}
