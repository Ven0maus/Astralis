using Astralis.GameCode.Items.Equipables;
using System;
using System.Collections.Generic;

namespace Astralis.GameCode.Items
{
    internal interface IInventory
    {
        event EventHandler<Item> OnItemAdded;
        event EventHandler<Item> OnItemRemoved;
        event EventHandler<Components.Inventory.InventoryItemMovedArgs> OnItemMoved;
        event EventHandler<Components.Inventory.InventoryItemChangedArgs> OnItemAmountChanged;
        int TotalSlots { get; }
        int OpenSlots { get; }
        IReadOnlyDictionary<int, Item> Items { get; }
        IReadOnlyDictionary<EquipableSlot, IEquipable> Equipment { get; }
        bool Add(Item item);
        void Remove(Item item);
        void MoveItem(Item item, int slotIndex);
        bool Equip(EquipableSlot slot, IEquipable item);
        bool Unequip(EquipableSlot slot);
    }
}
