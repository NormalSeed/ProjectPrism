using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemInventoryManager : MonoBehaviour, IItemInventoryService
{
    private readonly Dictionary<ItemData, int> _items = new Dictionary<ItemData, int>();

    public event Action OnItemInventoryUpdated;

    public List<ItemData> GetOwnedItems()
    {
        var result = new List<ItemData>();
        foreach (var pair in _items)
        {
            for (int i = 0; i < pair.Value; i++)
                result.Add(pair.Key);
        }
        return result;
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;
        if (_items.ContainsKey(item))
            _items[item]++;
        else
            _items[item] = 1;

        OnItemInventoryUpdated?.Invoke();
    }

    public bool RemoveItem(ItemData item)
    {
        if (item == null || !_items.ContainsKey(item)) return false;

        _items[item]--;
        if (_items[item] <= 0)
            _items.Remove(item);

        OnItemInventoryUpdated?.Invoke();
        return true;
    }

    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;
        return _items.TryGetValue(item, out int count) ? count : 0;
    }

    public void ClearInventory()
    {
        _items.Clear();
        OnItemInventoryUpdated?.Invoke();
    }
}
