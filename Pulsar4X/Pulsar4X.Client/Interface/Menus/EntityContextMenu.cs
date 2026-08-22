using ImGuiNET;
using Pulsar4X.Api;
using Pulsar4X.Client.Interface;
using System;
using System.Collections.Generic;

namespace Pulsar4X.Client
{
    public abstract class EntityContextAction
    {
        public abstract string Name { get; }

        public abstract bool CanPerformAction(EntityState entity, GlobalUIState state);

        public abstract void PerformAction(EntityState entity, GlobalUIState state);
    }

    public class EntityContextMenu(GlobalUIState state)
    {
        private readonly List<EntityContextAction> _actions = [
            new SelectPrimaryEntityAction(),
            new PinCameraAction(),
            new RenameEntityAction(),
            new OpenFireControlAction(),
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

            void ContextButton(Type T)
            {
                //Creates a context button if it is valid
                if (EntityUIWindows.CheckIfCanOpenWindow(T, _entityState, _state))
                {
                    if (ImGui.SmallButton(GlobalUIState.NamesForMenus[T]))
                    {
                        EntityUIWindows.OpenUIWindow(T, _entityState, _state, true, true);
                    }
                }
            }

            //Creates all the context buttons
            // ContextButton(typeof(SelectPrimaryBlankMenuHelper));
            // ContextButton(typeof(PinCameraBlankMenuHelper));
            // ContextButton(typeof(RenameWindow));
            // ContextButton(typeof(FireControl));
            ContextButton(typeof(CreateTransferWindow));
            ContextButton(typeof(GotoSystemBlankMenuHelper));
            ContextButton(typeof(WarpOrderWindow));
            ContextButton(typeof(ChangeCurrentOrbitWindow));
            ContextButton(typeof(NavWindow));
            ContextButton(typeof(OrdersListWindow));
            ImGui.EndGroup();
        }
    }

    #region Actions

    public class SelectPrimaryEntityAction : EntityContextAction
    {
        public override string Name => "Select Primary";

        public override void PerformAction(EntityState entity, GlobalUIState state)
        {
            if (entity.StarSystemId != null)
            {
                state.EntitySelectedAsPrimary(entity.Id, entity.StarSystemId);
            }
        }

        public override bool CanPerformAction(EntityState entity, GlobalUIState state) => true;
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

    #endregion
}
