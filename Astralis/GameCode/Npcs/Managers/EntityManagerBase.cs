using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.GameCode.Npcs.Managers
{
    internal abstract class EntityManagerBase
    {
        /// <summary>
        /// Renderer component that the entities are using to render, hook this to a console.
        /// </summary>
        public readonly SadConsole.Entities.EntityManager EntityComponent = new();
        /// <summary>
        /// Inserts the entity at the given location within the entity manager.
        /// This method is meant to insert an entity, after it was created.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="npc"></param>
        public abstract void SpawnAt(Point position, Actor npc);
        /// <summary>
        /// Creates a new entity of the given type at the specified location within the entity manager.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="position"></param>
        /// <returns></returns>
        public abstract T CreateAt<T>(Point position) where T : Actor, new();
        /// <summary>
        /// Removes all the entities from the given position, based on optional criteria.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="criteria"></param>
        /// <returns>Removed entities</returns>
        public abstract IEnumerable<Actor> RemoveAll(Point position, Func<Actor, bool> criteria = null);
        /// <summary>
        /// Removes the entity from the given position.
        /// </summary>
        /// <param name="entity"></param>
        public abstract void Remove(Actor entity);
        /// <summary>
        /// Returns true if entities exist at the given location, false if not.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>True/False</returns>
        public abstract bool EntitiesExistAt(Point point);
        /// <summary>
        /// Returns all the entities at the given position.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Entities at point</returns>
        public abstract IEnumerable<Actor> GetEntitiesAt(Point point);

        /// <summary>
        /// Returns all the entities at the given position of the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="point"></param>
        /// <returns></returns>
        public IEnumerable<T> GetEntitiesAt<T>(Point point) where T : Actor
        {
            return GetEntitiesAt(point).OfType<T>();
        }

        /// <summary>
        /// Keeps the location of each entity synced in the manager as they move around.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        protected abstract void UpdateEntityPositionWithinManager(object sender, ValueChangedEventArgs<Point> e);
    }
}
