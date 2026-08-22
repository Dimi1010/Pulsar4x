using System.Linq;
using Pulsar4X.Api;
// Engine usings: Stringify formatting, plus the deferred camera-pin / maneuver-panel bridges below.

namespace Pulsar4X.Client.Interface;

internal static class EntityUtility
{
    /// <summary>
    /// Resolves an EntityState to the corresponding EntitySnapshot in the current game state, if available.
    /// </summary>
    /// <param name="entityState">The entity state to resolve.</param>
    /// <param name="state">The global UI state.</param>
    /// <returns>The corresponding EntitySnapshot if found; otherwise, null.</returns>
    public static EntitySnapshot? ResolveEntity(EntityState entityState, GlobalUIState state)
        => entityState.StarSystemId is { } systemId
            ? state.GameClient?.Galaxy.GetSystem(systemId)?.GetEntity(entityState.Id)
            : null;

    /// <summary>
    /// Gets the colony entity that is either the entity itself (if it's a colony) or the owned colony on the given body.
    /// </summary>
    /// <param name="system">The client system instance.</param>
    /// <param name="entity">The entity to check for an attached colony.</param>
    /// <returns>The attached colony entity, or null if none is found.</returns>
    public static EntitySnapshot? GetAttachedColony(IClientSystem system, EntitySnapshot entity)
    {
        if (entity.Kind == BodyKind.Colony) return entity;

        return system.Entities.FirstOrDefault(e => e.Kind == BodyKind.Colony
            && e.Relation == OwnerRelation.Owned
            && e.GetView<ColonyView>()?.PlanetEntityId == entity.Id);
    }
}
