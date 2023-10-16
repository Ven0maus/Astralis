using Astralis.Extended.SadConsoleExt;
using Astralis.GameCode.Items;
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

        private readonly Dictionary<int, Item> _items;
        private readonly bool _isPlayerInventory;

        /// <summary>
        /// Constructor for the inventory
        /// </summary>
        /// <param name="slots">The total slots the inventory can contain</param>
        /// <param name="isPlayerInventory">Popup messages will be shown for certain actions by the player</param>
        public Inventory(int slots, bool isPlayerInventory = false)
        {
            TotalSlots = slots;

            _isPlayerInventory = isPlayerInventory;
            _items = new Dictionary<int, Item>(slots);
        }

        /// <summary>
        /// Adds a new item to the inventory
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="amount">Total to add</param>
        public void Add(Item item)
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
                    return;
                }
                var nextSlotIndex = _items.Count;
                _items.Add(nextSlotIndex, item);
                OnItemAdded?.Invoke(this, item);
            }
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
