using System;
using ImGuiNET;
using Pulsar4X.Api;

namespace Pulsar4X.Client
{
    //a do nothing helper class that is plugged into generics for static checks
    public class PinCameraBlankMenuHelper : UniquePulsarGuiWindow<PinCameraBlankMenuHelper>
    {
        internal override void Display()
        {
        }
    }

    //a do nothing helper class that is plugged into generics for static checks
    public class GotoSystemBlankMenuHelper : UniquePulsarGuiWindow<GotoSystemBlankMenuHelper>
    {
        internal override void Display()
        {

        }
    }


    //a do nothing helper class that is plugged into generics for static checks
    public class SelectPrimaryBlankMenuHelper : UniquePulsarGuiWindow<SelectPrimaryBlankMenuHelper>
    {
        internal override void Display()
        {

        }
    }
}
