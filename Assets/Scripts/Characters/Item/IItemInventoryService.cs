using System;
using System.Collections.Generic;

public interface IItemInventoryService
{
    event Action OnItemInventoryUpdated;

    List<ItemData> GetOwnedItems();
    void AddItem(ItemData item);
    bool RemoveItem(ItemData item);
    int GetItemCount(ItemData item);
    void ClearInventory();
}
