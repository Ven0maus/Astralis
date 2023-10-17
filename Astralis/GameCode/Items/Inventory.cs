using Astralis.Extended.SadConsoleExt;
using Astralis.GameCode.Items;
using Astralis.GameCode.Items.Equipables;
using Astralis.GameCode.Npcs;
using GoRogue.DiceNotation.Terms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.GameCode.Components
{
    /// <summary>
    /// An inventory component for Actors
    /// </summary>
    internal class Inventory : IInventory, IEnumerable<KeyValuePair<int, Item>>
    {
        /// <summary>
        /// Raised when an item is added to the inventory.
        /// </summary>
        public event EventHandler<Item> OnItemAdded;
        /// <summary>
        /// Raised when an item is removed from the inventory.
        /// </summary>
        public event EventHandler<Item> OnItemRemoved;
        /// <summary>
        /// Raised when an item moves to another slot.
        /// </summary>
        public event EventHandler<InventoryItemMovedArgs> OnItemMoved;
        /// <summary>
        /// Raised when the amount of an item in the inventory changed.
        /// </summary>
        public event EventHandler<InventoryItemChangedArgs> OnItemAmountChanged;

        /// <summary>
        /// The total amount of item slots the inventory contains.
        /// </summary>
        public int TotalSlots { get; }
        /// <summary>
        /// The amount of unoccupied items slots within the inventory.
        /// </summary>
        public int OpenSlots { get { return TotalSlots - Items.Count; } }
        /// <summary>
        /// The items currently in the inventory.
        /// </summary>
        public IReadOnlyDictionary<int, Item> Items { get { return _items; } }
        /// <summary>
        /// The items currently equipped.
        /// </summary>
        public IReadOnlyDictionary<EquipableSlot, IEquipable> Equipment { get { return _equipment; } }

        private readonly Dictionary<EquipableSlot, IEquipable> _equipment;
        private readonly Dictionary<int, Item> _items;
        private readonly bool _isPlayerInventory;
        private readonly Actor _actor;

        /// <summary>
        /// Constructor for the inventory
        /// </summary>
        /// <param name="slots">The total slots the inventory can contain</param>
        /// <param name="isPlayerInventory">Popup messages will be shown for certain actions by the player</param>
        public Inventory(Actor actor, int slots)
        {
            TotalSlots = slots;

            _actor = actor;
            _isPlayerInventory = actor is Player;
            _items = new Dictionary<int, Item>(slots);
            _equipment = new Dictionary<EquipableSlot, IEquipable>();

            // Pre-fill the equipment slots
            var equipableSlots = Enum.GetValues<EquipableSlot>();
            foreach (var equipableSlot in equipableSlots)
                _equipment.Add(equipableSlot, null);
        }

        /// <summary>
        /// Adds a new item to the inventory
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="amount">Total to add</param>
        public bool Add(Item item)
        {
            if (ContainsValue(item, out int slotIndex))
            {
                var obj = _items[slotIndex];
                var oldAmount = obj.Amount;
                obj.Amount += item.Amount;
                OnItemAmountChanged?.Invoke(this, new InventoryItemChangedArgs(item, oldAmount, obj.Amount));
            }
            else
            {
                if (_items.Count == TotalSlots)
                {
                    if (_isPlayerInventory)
                        ScWindow.Message("Your inventory cannot hold more items!", "Ok");
                    return false;
                }
                _items.Add(GetNextAvailableSlotIndex(), item);
                OnItemAdded?.Invoke(this, item);
            }
            return true;
        }

        /// <summary>
        /// Removes an item from the inventory, freeing up a slot.
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="amount">Total to remove</param>
        public void Remove(Item item)
        {
            if (item.Amount <= 0) return;
            if (ContainsValue(item, out int slotIndex))
            {
                var obj = _items[slotIndex];
                var oldAmount = obj.Amount;
                if (oldAmount - item.Amount <= 0)
                {
                    _items.Remove(slotIndex);
                    OnItemRemoved?.Invoke(this, obj);
                }
                else
                {
                    obj.Amount -= item.Amount;
                    OnItemAmountChanged?.Invoke(this, new InventoryItemChangedArgs(item, oldAmount, obj.Amount));
                }
            }
        }

        /// <summary>
        /// Move an item to a different slot
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="slotIndex">The new slot to move to</param>
        public void MoveItem(Item item, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex > TotalSlots - 1 || _items.ContainsKey(slotIndex) ||
                !ContainsValue(item, out int oldSlotIndex))
            {
                return;
            }

            var obj = _items[oldSlotIndex];
            _items.Remove(oldSlotIndex);
            _items.Add(slotIndex, obj);
            OnItemMoved?.Invoke(this, new InventoryItemMovedArgs(item, oldSlotIndex, slotIndex));
        }

        /// <summary>
        /// Equip the given item in the given slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="item"></param>
        public bool Equip(EquipableSlot slot, IEquipable item)
        {
            if (!IsSlotValid(slot, item)) return false;

            var oldEquipment = _equipment[slot];

            // Check if there is space to return the item to the inventory
            if (oldEquipment != null && !HasSpaceToAddItem((Item)oldEquipment, (Item)item)) return false;

            // Remove stats from previous item
            oldEquipment?.RemoveStats(_actor);

            // Remove the new item from the inventory if its in there
            if (ContainsValue((Item)item, out _))
                Remove((Item)item);

            // Replace existing equipped item
            _equipment[slot] = item;

            // Return oldEquipment back to the inventory
            if (oldEquipment != null)
                Add((Item)oldEquipment);

            // Add stats of this item to the actor
            item.AddStats(_actor);
            return true;
        }

        /// <summary>
        /// Unequip the currently equipped item from the given slot.
        /// </summary>
        /// <param name="slot"></param>
        public bool Unequip(EquipableSlot slot)
        {
            var oldEquipment = _equipment[slot];
            if (oldEquipment == null) return true;
            if (!HasSpaceToAddItem((Item)oldEquipment)) return false;

            // Remove stats of this item from the actor
            _equipment[slot].RemoveStats(_actor);

            // Return item to inventory if exists
            Add((Item)oldEquipment);

            _equipment[slot] = null;
            return true;
        }

        private static bool IsSlotValid(EquipableSlot slot, IEquipable item)
        {
            switch (item.Type)
            {
                case EquipableType.Weapon:
                    return slot == EquipableSlot.WeaponLeft || slot == EquipableSlot.WeaponRight;
                case EquipableType.Shield:
                    return slot == EquipableSlot.WeaponRight;
                case EquipableType.Amulet:
                    return slot == EquipableSlot.Amulet;
                case EquipableType.Ring:
                    return slot == EquipableSlot.RingLeft || slot == EquipableSlot.RingRight;
                case EquipableType.Helmet:
                    return slot == EquipableSlot.Helmet;
                case EquipableType.Torso:
                    return slot == EquipableSlot.Torso;
                case EquipableType.Pants:
                    return slot == EquipableSlot.Pants;
                case EquipableType.Gloves:
                    return slot == EquipableSlot.Gloves;
                case EquipableType.Shoes:
                    return slot == EquipableSlot.Shoes;
                default:
                    return false;
            }
        }

        private bool HasSpaceToAddItem(Item item, Item replaceOldItem = null)
        {
            if (ContainsValue(item, out _))
                return true;
            if (replaceOldItem != null && ContainsValue(replaceOldItem, out int slotIndex))
                return _items[slotIndex].Amount == 1;
            return _items.Count < TotalSlots - 1;
        }

        private bool ContainsValue(Item item, out int slotIndex)
        {
            if (_items.ContainsValue(item))
            {
                slotIndex = _items.First(kv => kv.Value.Equals(item)).Key;
                return true;
            }

            slotIndex = default;
            return false;
        }

        private int GetNextAvailableSlotIndex()
        {
            int nextSlotIndex = 0;
            while (_items.ContainsKey(nextSlotIndex))
                nextSlotIndex++;
            return nextSlotIndex;
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<int, Item>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public sealed class InventoryItemMovedArgs
        {
            /// <summary>
            /// The item
            /// </summary>
            public Item Item { get; }
            /// <summary>
            /// The old slot of the item
            /// </summary>
            public int OldSlotIndex { get; }
            /// <summary>
            /// The new slot the item moved to
            /// </summary>
            public int NewSlotIndex { get; }

            public InventoryItemMovedArgs(Item item, int oldSlotIndex, int newSlotIndex)
            {
                Item = item;
                OldSlotIndex = oldSlotIndex;
                NewSlotIndex = newSlotIndex;
            }
        }

        public sealed class InventoryItemChangedArgs
        {
            /// <summary>
            /// The item
            /// </summary>
            public Item Item { get; }
            /// <summary>
            /// The original value before the change
            /// </summary>
            public int OldAmount { get; }
            /// <summary>
            /// The old amount + the change value
            /// </summary>
            public int NewAmount { get; }
            /// <summary>
            /// The change value
            /// </summary>
            public int AmountChange { get { return NewAmount - OldAmount; } }

            public InventoryItemChangedArgs(Item item, int oldAmount, int newAmount)
            {
                Item = item;
                OldAmount = oldAmount;
                NewAmount = newAmount;
            }
        }
    }
}
