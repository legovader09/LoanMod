using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace LoanMod
{
    public partial class ModEntry : Mod
    {
        internal ModConfig Config;
        private bool borrowProcess, repayProcess;
        private int amount, duration;
        private float interest;
        internal List<Response> menuItems, repayMenuItems, durationMenu, menuYesNo;
        internal ITranslationHelper i18n => Helper.Translation;
        private LoanManager loanManager;
        //private TextBox input;
        //private readonly int TEXT_WIDTH = Game1.tileSize * 3, TEXT_HEIGHT = Game1.tileSize * 2;

        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += this.GameLoaded;
            helper.Events.GameLoop.Saving += this.Saving;
            helper.Events.GameLoop.DayEnding += this.DayEnding;
            helper.Events.GameLoop.DayStarted += this.DayStarted;
            helper.Events.Display.MenuChanged += this.MenuChanged;
            
            Config = helper.ReadConfig<ModConfig>();

            menuItems = new List<Response>
            {
                new Response("money_500", $"{Config.MoneyAmount1}g"),
                new Response("money_1k", $"{Config.MoneyAmount2}g"),
                new Response("money_5k", $"{Config.MoneyAmount3}g"),
                new Response("money_10k", $"{Config.MoneyAmount4}g"),
                new Response("money_Cancel", i18n.Get("menu.cancel"))
            };

            durationMenu = new List<Response>
            {
                new Response("time_3D", $"{Config.DayLength1} {i18n.Get("menu.days")} @ {Config.InterestModifier1 * 100}%"),
                new Response("time_7D", $"{Config.DayLength2} {i18n.Get("menu.days")} @ {Config.InterestModifier2 * 100}%"),
                new Response("time_14D", $"{Config.DayLength3} {i18n.Get("menu.days")} @ {Config.InterestModifier3 * 100}%"),
                new Response("time_28D", $"{Config.DayLength4} {i18n.Get("menu.days")} @ {Config.InterestModifier4 * 100}%"),
                new Response("time_Cancel", i18n.Get("menu.cancel"))
            };

            repayMenuItems = new List<Response>
            {
                new Response("repay_show_Balance", i18n.Get("menu.showbalance")),
                new Response("repay_Full", i18n.Get("menu.repayfull")),
                new Response("repay_Leave", i18n.Get("menu.leave"))
            };

            menuYesNo = new List<Response>
            {
                new Response("menu_Yes", i18n.Get("menu.yes")),
                new Response("menu_No", i18n.Get("menu.no")),
                new Response("menu_Leave", i18n.Get("menu.leave"))
            };

        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            //if (!Game1.IsMasterGame)
            //{
            //    Game1.addHUDMessage(new HUDMessage(i18n.Get("msg.hostonly"), 3));
            //    return;
            //}

            if (!Context.CanPlayerMove)
                return;

            if (this.Helper.Input.IsDown(Config.LoanButton))
            {
                StartBorrow(1, "Key_Amount");
                this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Info);
            }

        }

        private void StartBorrow(int stage, string key)
        {
            var Gamer = Game1.currentLocation;
            //check if player isnt already borrowing
            if (!loanManager.IsBorrowing)
            {
                switch (stage)
                {
                    case 1:
                        if (Config.CustomMoneyInput)
                            Game1.activeClickableMenu = new NumberSelectionMenu(i18n.Get("msg.startborrow-1"), (val, cost, farmer) => ProcessBorrowing(val, cost, farmer, key), -1, 100, 999999, 500);
                        else
                            Gamer.createQuestionDialogue(i18n.Get("msg.startborrow-1"), menuItems.ToArray(), BorrowMenu);
                        break;
                    case 2:
                        //Game1.activeClickableMenu = new NumberSelectionMenu(i18n.Get("msg.startborrow-2"), (val, cost, farmer) => ProcessBorrowing(val, cost, farmer, key), -1, 1);
                        Gamer.createQuestionDialogue(i18n.Get("msg.startborrow-2"), durationMenu.ToArray(), BorrowDuration);
                        break;
                }
            }
            else
            { 
                switch (stage)
                {
                    case 1:
                        Gamer.createQuestionDialogue(i18n.Get("msg.menu-1"), repayMenuItems.ToArray(), RepayMenu);
                        break;
                    case 3:
                        Gamer.createQuestionDialogue(i18n.Get("msg.menu-2", new { Balance = loanManager.Balance }), menuYesNo.ToArray(), RepayFullMenu);
                        break;
                }
            }
        }

        private void ProcessBorrowing(int val, int cost, Farmer who, string key)
        {
            switch (key)
            {
                case "Key_Amount":
                    amount = val;
                    borrowProcess = true;
                    this.Monitor.Log($"Selected {amount}g", LogLevel.Info);
                    Game1.activeClickableMenu = null;
                    StartBorrow(2, "Key_Duration");
                    break;
                default:
                    break;
                //case "Key_Duration":
                //    duration = val;
                //    this.Monitor.Log($"Selected {amount} days.", LogLevel.Info);
                //    break;

            }
        }

        private void MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (borrowProcess && Game1.player.canMove == true)
            {
                if (amount >= 0 && duration == 0)
                {
                    StartBorrow(2, "Key_Duration");
                }
                else if (amount >= 0 && duration > 0)
                {
                    InitiateBorrow(amount, duration, interest);
                    //interest = (float)(loanManager.EstimateInterest(amount, duration, Config));
                    //Game1.currentLocation.createQuestionDialogue(i18n.Get("msg.confirmborrow", new { Amount = amount, Days = duration, Interest = interest * 100 }), menuYesNo.ToArray(), ConfirmMenu); ;
                }
            }
            if (repayProcess && Game1.player.canMove == true)
            {
                StartBorrow(3, "Key_Repay");
            }
        }

        //private void ConfirmMenu(Farmer who, string key)
        //{
        //    switch (key)
        //    {
        //        case "menu_Yes":
        //            InitiateBorrow(amount, duration, interest);
        //            this.Monitor.Log("Loan application approved.", LogLevel.Info);
        //            break;
        //        default:
        //            this.Monitor.Log("Loan application cancelled by user.", LogLevel.Info);
        //             break;
        //    }
        //}

        private void BorrowMenu(Farmer who, string menu)
        {
            switch (menu)
            {
                case "money_500":
                    amount = Config.MoneyAmount1;
                    borrowProcess = true;
                    this.Monitor.Log($"Selected 500g.", LogLevel.Info);
                    break;
                case "money_1k":
                    amount = Config.MoneyAmount2;
                    borrowProcess = true;
                    this.Monitor.Log($"Selected 1,000g.", LogLevel.Info);
                    break;
                case "money_5k":
                    amount = Config.MoneyAmount3;
                    borrowProcess = true;
                    this.Monitor.Log($"Selected 5,000g.", LogLevel.Info);
                    break;
                case "money_10k":
                    amount = Config.MoneyAmount4;
                    borrowProcess = true;
                    this.Monitor.Log($"Selected 10,000g.", LogLevel.Info);
                    break;
                case "money_Cancel":
                    borrowProcess = false;
                    this.Monitor.Log($"Option Cancel");
                    break;
                default:
                    break;
            }
        }

        private void BorrowDuration(Farmer who, string dur)
        {
            switch (dur)
            {
                case "time_3D":
                    duration = Config.DayLength1;
                    interest = Config.InterestModifier1;
                    this.Monitor.Log($"Selected {Config.DayLength1} days.");
                    break;
                case "time_7D":
                    duration = Config.DayLength2;
                    interest = Config.InterestModifier2;
                    this.Monitor.Log($"Selected {Config.DayLength2} days.");
                    break;
                case "time_14D":
                    duration = Config.DayLength3;
                    interest = Config.InterestModifier3;
                    this.Monitor.Log($"Selected {Config.DayLength3} days.");
                    break;
                case "time_28D":
                    duration = Config.DayLength4;
                    interest = Config.InterestModifier4;
                    this.Monitor.Log($"Selected {Config.DayLength4} days.");
                    break;
                case "time_Cancel":
                    borrowProcess = false;
                    this.Monitor.Log($"Option Cancel");
                    break;
                default:
                    break;
            }
        }
        private void RepayMenu(Farmer who, string option)
        {
            switch (option)
            {
                case "repay_show_Balance":
                    this.Monitor.Log($"Option show balance", LogLevel.Info);
                    Game1.addHUDMessage(new HUDMessage(i18n.Get("msg.payment.remaining", new { Balance = loanManager.Balance, Duration = loanManager.Duration, DailyAmount = loanManager.DailyAmount }), HUDMessage.newQuest_type));
                    break;
                case "repay_Full":
                    this.Monitor.Log($"Option repay Full", LogLevel.Info);
                    repayProcess = true;
                    break;
                case "repay_Leave":
                    this.Monitor.Log($"Option Leave", LogLevel.Info);
                    break;
                default:
                    break;
            }
        }
        private void RepayFullMenu(Farmer who, string option)
        {
            switch (option)
            {
                case "menu_Yes":
                    this.Monitor.Log($"Option Yes", LogLevel.Info);
                    InitiateRepayment(true);
                    Game1.addHUDMessage(new HUDMessage(i18n.Get("msg.payment.full"), HUDMessage.achievement_type));
                    break;
                case "menu_No":
                    this.Monitor.Log($"Option No", LogLevel.Info);
                    repayProcess = false;
                    break;
                case "menu_Leave":
                    this.Monitor.Log($"Option Leave", LogLevel.Info);
                    repayProcess = false;
                    break;
                default:
                    break;
            }
        }

        private void InitiateBorrow(int option, int duration, float interest)
        {
            loanManager.AmountBorrowed = option;
            loanManager.Duration = duration;
            loanManager.Interest = interest;
            loanManager.Balance = (int)loanManager.CalculateBalance;
            loanManager.DailyAmount = (int)loanManager.CalculateDailyAmount;

            this.Monitor.Log($"Amount: {option}, Duration: {duration}, Interest: {interest}.", LogLevel.Info);

            Game1.player.Money += option;

            loanManager.IsBorrowing = true;
            borrowProcess = false;

            this.Monitor.Log($"Is Borrowing: {loanManager.IsBorrowing}.", LogLevel.Info);

            amount = 0;
            this.duration = 0;
            this.interest = 0;

            AddMessage(i18n.Get("msg.payment.credited", new { creditAmount = loanManager.AmountBorrowed }));

        }

        private void InitiateRepayment(bool full)
        {
            if (loanManager.IsBorrowing && loanManager.Balance > 0 )
            {
                //check if player wants to repay in full.
                if (full) 
                {
                    if (Game1.player.Money >= loanManager.Balance)
                    {   //Repays the remaining balance
                        Game1.player.Money -= loanManager.Balance;
                        repayProcess = false;
                        loanManager.InitiateReset();
                    }
                    else
                    {
                        AddMessage(i18n.Get("msg.payment.failed", new { DailyAmount = loanManager.Balance }));
                        repayProcess = false;
                    }
                    return;
                }

                //Check if you are still in loan contract
                if (loanManager.Duration > 0)
                {

                    //If player has enough Money for the daily deduction amount
                    if (Game1.player.Money >= loanManager.DailyAmount)
                    {
                        //Checks if the balance is greater than or equal to the daily repayment amount
                        if (loanManager.Balance > loanManager.DailyAmount)
                        {
                            Game1.player.Money -= loanManager.DailyAmount;
                            loanManager.AmountRepaid += loanManager.DailyAmount;
                            loanManager.Balance -= loanManager.DailyAmount;
                            loanManager.hasPaid = true;
                        }
                        else
                        {
                            //Repays the remaining balance
                            Game1.player.Money -= loanManager.Balance;
                            loanManager.hasPaid = true;
                            loanManager.IsBorrowing = false;
                            AddMessage(i18n.Get("msg.payment.full"));
                            //Game1.addHUDMessage(new HUDMessage("Thank you for your payment! You have successfully paid any outstanding balance in full.", HUDMessage.achievement_type));
                        }
                        loanManager.Duration -= 1;
                        loanManager.LateDays = 0;
                    }
                    else
                    {
                        if (Config.LatePaymentChargeRate != 0)
                        {
                            loanManager.LateChargeRate = Config.LatePaymentChargeRate;
                            loanManager.LateChargeAmount = (int)loanManager.CalculateLateFees;
                            AddMessage(i18n.Get("msg.payment.failed", new { DailyAmount = loanManager.DailyAmount }));
                            if (loanManager.LateDays == 0)
                            {
                                Game1.addHUDMessage(new HUDMessage(i18n.Get("msg.payment.missed-1", new { LateChargeAmount = loanManager.LateChargeAmount }), HUDMessage.error_type));
                                loanManager.LateDays += 1;
                            }
                            else
                            {
                                Game1.addHUDMessage(new HUDMessage(i18n.Get("msg.payment.missed-2", new { LateChargeAmount = loanManager.LateChargeAmount }), HUDMessage.error_type));
                                loanManager.Balance += loanManager.LateChargeAmount;
                            }
                        }
                        loanManager.hasPaid = false;
                    }
                }
            }

        }
        
        private void GameLoaded(object sender, SaveLoadedEventArgs e)
        {
            //if (!Game1.IsMasterGame)
            //    return;

            this.Monitor.Log("Current Locale: " + i18n.LocaleEnum, LogLevel.Info);

            try
            {
                //checks if player is currently taking any loans, if so it will load all the loan data.
                if (Game1.player.IsMainPlayer)
                    loanManager = Helper.Data.ReadSaveData<LoanManager>("Doomnik.MoneyManage");

                if (loanManager == null || Config.Reset)
                {
                    loanManager = new LoanManager();
                    loanManager.InitiateReset();
                    Config.Reset = false;
                    AddMessage(i18n.Get("msg.create"));
                }

                if (Game1.player.IsMainPlayer) //immediately saves loan file.
                    Helper.Data.WriteSaveData("Doomnik.MoneyManage", loanManager);
            }
            catch { }
        }

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            //checks if player has made payment.
            if (loanManager.IsBorrowing)
            {
                if (loanManager.hasPaid)
                {
                    AddMessage(i18n.Get("msg.payment.complete", new { DailyAmount = loanManager.DailyAmount }));
                    //Game1.addHUDMessage(new HUDMessage($"Thanks for making a payment of {moneyManage.DailyAmount}g.", HUDMessage.achievement_type));
                    loanManager.hasPaid = false;
                }
                //else 
                //{
                //    Game1.addHUDMessage(new HUDMessage("We weren't able to deduct your daily amount today, we will re-attempt tomorrow.", HUDMessage.error_type));
                //}

                if (loanManager.Balance < loanManager.DailyAmount) { loanManager.DailyAmount = loanManager.Balance; }

            }
            else
            {
                loanManager.InitiateReset();
            }

            if (loanManager.Balance < 0)
            {
                this.Monitor.Log($"Amount Borrowed vs Repaid: {loanManager.AmountBorrowed} / {loanManager.AmountRepaid}, Duration: {loanManager.Duration}. Interest: {loanManager.Interest}", LogLevel.Info);
                loanManager.InitiateReset();
                AddMessage(i18n.Get("msg.payment.error"));
            }
        }
        internal bool canSave;
        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            //if (!Game1.IsMasterGame)
            //    return;

            canSave = true;
        }

        private void Saving(object sender, SavingEventArgs e)
        {
            if (Game1.player.IsMainPlayer)
            {
                if (canSave)
                {
                    InitiateRepayment(false);
                    canSave = false;
                }
                //saving data
                Helper.Data.WriteSaveData("Doomnik.MoneyManage", loanManager);
                Helper.WriteConfig(Config);
            }
        }
    }
}