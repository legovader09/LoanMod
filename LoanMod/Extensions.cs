using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using System;

namespace LoanMod
{
    public partial class ModEntry
    {
        public void Log(object message)
        { 
            this.Monitor.Log(Convert.ToString(message), LogLevel.Info); 
        }

        public void AddMessage(string message)
        {
            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.achievement_type));
        }
    }
}
