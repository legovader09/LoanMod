//using StardewValley;
//using StardewValley.Menus;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace LoanMod
//{
//    partial class ModEntry
//    {
//        internal class TextInput : IClickableMenu
//        {
//            private TextBox input;
//            private readonly int TEXT_WIDTH = Game1.tileSize * 3, TEXT_HEIGHT = Game1.tileSize * 2;

//            public ShowBox(String title)
//                : base(0,0,0,0,true)
//            {
//                this.input = new TextBox(null, null, Game1.dialogueFont, Game1.textColor)
//                {
//                    X = Game1.viewportCenter.X - (TEXT_WIDTH/2),
//                    Y = Game1.viewportCenter.Y - (TEXT_HEIGHT/2),
//                    Width = TEXT_WIDTH,
//                    Height = TEXT_HEIGHT,
//                    Selected = false,
//                    Text = "500",
//                    TitleText = title
//                };
//                Game1.keyboardDispatcher.Subscriber = input;
//                return input;
//            }
//        }
//    }
//}