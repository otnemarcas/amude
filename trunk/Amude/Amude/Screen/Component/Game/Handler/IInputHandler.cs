using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amude.Domain;

namespace Amude.Screen.Component.Game.Handler
{
    internal enum Trigger
    {
        DoNothing,
        SendConfirmUpdate,
        SendConfirmAffects,
        TriggerActionBox
    }

    interface IInputHandler
    {
        void Update(float passedTime);
        void FillOptions();
        void Watch(Character character);
        void SetTrigger(Trigger trigger);
    }
}
