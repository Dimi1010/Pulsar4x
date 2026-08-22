using System;
using ImGuiNET;

namespace Pulsar4X.Client
{

    public class EntityContextMenu(GlobalUIState state)
    {
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

            void ContextButton(Type T)
            {
                //Creates a context button if it is valid
                if(EntityUIWindows.CheckIfCanOpenWindow(T, _entityState, _state))
                {
                    if (ImGui.SmallButton(GlobalUIState.NamesForMenus[T]))
                    {
                        EntityUIWindows.OpenUIWindow(T, _entityState, _state, true ,true);
                    }
                }
            }

            //Creates all the context buttons
            ContextButton(typeof(SelectPrimaryBlankMenuHelper));
            ContextButton(typeof(PinCameraBlankMenuHelper));
            ContextButton(typeof(RenameWindow));
            ContextButton(typeof(FireControl));
            ContextButton(typeof(CreateTransferWindow));
            ContextButton(typeof(GotoSystemBlankMenuHelper));
            ContextButton(typeof(WarpOrderWindow));
            ContextButton(typeof(ChangeCurrentOrbitWindow));
            ContextButton(typeof(NavWindow));
            ContextButton(typeof(OrdersListWindow));
            ImGui.EndGroup();

        }
    }
}
