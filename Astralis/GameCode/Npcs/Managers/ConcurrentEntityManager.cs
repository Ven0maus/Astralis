using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Astralis.GameCode.Npcs.Managers
{
    /// <summary>
    /// A concurrent implementation for managing entities.
    /// </summary>
    internal class ConcurrentEntityManager : EntityManager
    {
        private readonly ConcurrentDictionary<Point, HashSet<Actor>> _entities = new();
        private readonly ConcurrentDictionary<Point, object> _lockObjects = new();

        /// <summary>
        /// Generates all kind of unique npc variations in the font, and sets that font as the manager's entity font.
        /// </summary>
        public void GenerateProceduralNpcsFont()
        {
            // TODO: Generate procedural npcs


            // TODO: Remove the check after generation
            // Set font as entity manager's font
            if (Game.Instance.Fonts.ContainsKey(Constants.Fonts.NpcFonts.ProceduralNpcsFont))
                EntityComponent.AlternativeFont = Game.Instance.Fonts[Constants.Fonts.NpcFonts.ProceduralNpcsFont];
        }

        /// <summary>
        /// Helper method to lock the hashset collection within.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="action"></param>
        private void ApplyActionOnHashSet(Point point, Action<HashSet<Actor>> action)
        {
            // Get or create the hash set for a specific Point
            HashSet<Actor> entities = _entities.GetOrAdd(point, _ => new HashSet<Actor>());

            // Acquire the lock object associated with the hash set
            object lockObj = _lockObjects.GetOrAdd(point, _ => new object());

            // Enter the lock
            Monitor.Enter(lockObj);
            try
            {
                action(entities);
            }
            finally
            {
                // Ensure the lock is released
                Monitor.Exit(lockObj);

                // Remove the lock object from the dictionary
                _lockObjects.TryRemove(point, out _);
            }
        }

        /// <inheritdoc/>
        public override void SpawnAt(Point position, Actor entity)
        {
            ApplyActionOnHashSet(position, (entities) =>
            {
                entities.Add(entity);
                entity.Position = position;
                entity.PositionChanged += UpdateEntityPositionWithinManager;
                EntityComponent.Add(entity);
            });
        }

        /// <inheritdoc/>
        public override T CreateAt<T>(Point position)
        {
            var entity = new T();
            SpawnAt(position, entity);
            return entity;
        }

        /// <inheritdoc/>
        public override bool EntitiesExistAt(Point point)
        {
            return _entities.ContainsKey(point);
        }

        /// <inheritdoc/>
        public override IEnumerable<Actor> GetEntitiesAt(Point point)
        {
            IEnumerable<Actor> entitiesEnumerable = Enumerable.Empty<Actor>();
            if (!EntitiesExistAt(point)) return entitiesEnumerable;
            ApplyActionOnHashSet(point, (entities) =>
            {
                entitiesEnumerable = entities.ToArray();
            });
            return entitiesEnumerable;
        }

        /// <inheritdoc/>
        public override void Remove(Actor entity)
        {
            if (!EntitiesExistAt(entity.Position)) return;
            ApplyActionOnHashSet(entity.Position, (entities) =>
            {
                entity.PositionChanged -= UpdateEntityPositionWithinManager;
                entities.Remove(entity);
            });
        }

        /// <inheritdoc/>
        public override IEnumerable<Actor> RemoveAll(Point position, Func<Actor, bool> criteria = null)
        {
            if (!EntitiesExistAt(position)) return Enumerable.Empty<Actor>();
            var removedentities = new List<Actor>();
            ApplyActionOnHashSet(position, (entities) =>
            {
                IEnumerable<Actor> entitiesToBeRemoved = entities;
                if (criteria != null)
                    entitiesToBeRemoved = entitiesToBeRemoved.Where(criteria);
                foreach (var entity in entitiesToBeRemoved.ToArray())
                {
                    EntityComponent.Remove(entity);
                    entity.PositionChanged -= UpdateEntityPositionWithinManager;
                    entities.Remove(entity);
                    removedentities.Add(entity);
                }

                if (entities.Count == 0)
                    _entities.Remove(position, out _);
            });
            return removedentities;
        }

        /// <inheritdoc/>
        protected override void UpdateEntityPositionWithinManager(object sender, ValueChangedEventArgs<Point> e)
        {
            var entity = (Actor)sender;

            // Remove from previous
            if (EntitiesExistAt(e.OldValue))
            {
                ApplyActionOnHashSet(e.OldValue, (entities) =>
                {
                    entities.Remove(entity);
                    if (entities.Count == 0)
                        _entities.Remove(e.OldValue, out _);
                });
            }

            // Add to the current
            ApplyActionOnHashSet(e.NewValue, (entities) =>
            {
                entities.Add(entity);
            });
        }
    }
}
