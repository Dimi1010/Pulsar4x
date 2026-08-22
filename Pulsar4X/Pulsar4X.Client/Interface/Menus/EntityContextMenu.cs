using ImGuiNET;
using Pulsar4X.Api;
using Pulsar4X.Client.Interface;
using System;
using System.Collections.Generic;

namespace Pulsar4X.Client
{
    /// <summary>
    /// A base class for entity actions that are displayed in <see cref="EntityContextMenu"/>.
    /// </summary>
    public abstract class EntityContextAction
    {
        public abstract string Name { get; }

        /// <summary>
        /// Checks if the action can be performed for the selected entity.
        /// </summary>
        /// <param name="entity">The entity to check against.</param>
        /// <param name="state">The current global state.</param>
        /// <returns>True if the action can be performed, false otherwise.</returns>
        public abstract bool CanPerformAction(EntityState entity, GlobalUIState state);

        /// <summary>
        /// Performs the action.
        /// </summary>
        /// <param name="entity">The entity to perform the action on.</param>
        /// <param name="state">The current global state</param>
        /// <remarks>
        /// This method should only be called if <see cref="CanPerformAction"/> returns true for the given entity and state.
        /// </remarks>
        public abstract void PerformAction(EntityState entity, GlobalUIState state);
    }

    public class EntityContextMenu(GlobalUIState state)
    {
        private readonly List<EntityContextAction> _actions = [
            new SelectPrimaryEntityAction(),
            new PinCameraAction(),
            new RenameEntityAction(),
            new OpenFireControlAction(),
            new OpenCargoTransferAction(),
            new GotoSystemAction(),
            new OpenWarpToAction(),
            new ChangeOrbitAction(),
            new OpenNavigationAction(),
            new OpenOrdersAction(),
        ];

        GlobalUIState _state = state;
        EntityState? _entityState;

        public void SetEntity(int entityGuid)
        {
            var systemId = _state.SelectedStarSystemId;
            var snapshot = _state.GameClient?.Galaxy.GetSystem(systemId)?.GetEntity(entityGuid);
            if (snapshot != null)
                _entityState = new EntityState(snapshot, systemId);
        }

        public void ClearEntity()
        {
            _entityState = null;
        }

        internal void Display()
        {
            if (_entityState == null) return;

            ImGui.BeginGroup();

            // Creates the context buttons for the current entity.
            foreach (var action in _actions)
            {
                if (action.CanPerformAction(_entityState, _state))
                {
                    if (ImGui.SmallButton(action.Name))
                    {
                        action.PerformAction(_entityState, _state);
                    }
                }
            }
            ImGui.EndGroup();
        }
    }

    #region Actions

    public class SelectPrimaryEntityAction : EntityContextAction
    {
        public override string Name => "Select as primary";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state) => true;

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            if (entity.StarSystemId != null)
            {
                state.EntitySelectedAsPrimary(entity.Id, entity.StarSystemId);
            }
        }

    }

    public class PinCameraAction : EntityContextAction
    {
        public override string Name => "Pin Camera";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state) => true;

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            if (entity.StarSystemId is not null)
                state.Camera.PinToEntity(entity.Id, entity.StarSystemId, state);
        }
    }

    public class RenameEntityAction : EntityContextAction
    {
        public override string Name => "Rename";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state) => true;

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            var renameWindow = RenameWindow.GetInstance();
            renameWindow.SetTarget(entity.Id, entity.Name);
            state.ActiveWindow = renameWindow;
        }
    }

    public class OpenFireControlAction : EntityContextAction
    {
        public override string Name => "Fire Control";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state)
        {
            return EntityUtility.ResolveEntity(entity, state)?.HasView<FireControlView>() ?? false;
        }

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            var instance = FireControl.GetInstance(entity);
            instance.ToggleActive();
            state.ActiveWindow = instance;
        }
    }

    public class OpenCargoTransferAction : EntityContextAction
    {
        public override string Name => "Cargo";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state)
        {
            return EntityUtility.ResolveEntity(entity, state)?.HasView<CargoStorageView>() ?? false;
        }

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            if (entity.StarSystemId != null)
            {
                var instance = CreateTransferWindow.GetInstance();
                instance.SetLeft(entity.Id, entity.StarSystemId);
                instance.ToggleActive();
                state.ActiveWindow = instance;
            }
        }
    }

    public class GotoSystemAction : EntityContextAction
    {
        public override string Name => "Go to system";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state)
        {
            return EntityUtility.ResolveEntity(entity, state)?.GetView<GravSurveyView>()?.JumpPointToSystemId != null;
        }

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            var destId = EntityUtility.ResolveEntity(entity, state)?.GetView<GravSurveyView>()?.JumpPointToSystemId;
            if (destId is not null)
            {
                state.SetActiveSystem(destId);
            }
        }
    }

    public class OpenWarpToAction : EntityContextAction
    {
        public override string Name => "Warp to new orbit";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state)
        {
            return EntityUtility.ResolveEntity(entity, state)?.HasView<WarpAbilityView>() ?? false;
        }

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            var instance = WarpOrderWindow.GetInstance(entity);
            instance.ToggleActive();
            state.ActiveWindow = instance;
        }
    }

    public class ChangeOrbitAction : EntityContextAction
    {
        public override string Name => "Change current orbit";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state)
        {
            return EntityUtility.ResolveEntity(entity, state)?.HasView<ThrustView>() ?? false;
        }

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            var instance = ChangeCurrentOrbitWindow.GetInstance(entity);
            instance.SetActive(true);
            state.ActiveWindow = instance;
        }
    }

    public class OpenNavigationAction : EntityContextAction
    {
        public override string Name => "Nav window";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state)
        {
            return EntityUtility.ResolveEntity(entity, state)?.HasView<ThrustView>() ?? false;
        }

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            var instance = NavWindow.GetInstance(entity);
            instance.ToggleActive();
            state.ActiveWindow = instance;
        }
    }

    public class OpenOrdersAction : EntityContextAction
    {
        public override string Name => "Orders window";

        public override bool CanPerformAction(EntityState entity, GlobalUIState state)
        {
            return EntityUtility.ResolveEntity(entity, state)?.HasView<OrdersView>() ?? false;
        }

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            state.WindowManager.ActivateOrderListWindow(entity);
        }
    }

    #endregion
}
