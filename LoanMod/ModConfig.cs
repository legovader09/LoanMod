using StardewModdingAPI;

namespace LoanMod
{
    public partial class ModEntry
    {
        internal class ModConfig
        {
            public SButton LoanButton { get; set; } = SButton.L;
            public bool CustomMoneyInput { get; set; } = true;
            public float LatePaymentChargeRate { get; set; } = 0.1F;
            public float InterestModifier1 { get; set; } = 0.5F;
            public float InterestModifier2 { get; set; } = 0.25F;
            public float InterestModifier3 { get; set; } = 0.1F;
            public float InterestModifier4 { get; set; } = 0.05F;
            public int MoneyAmount1 { get; set; } = 500;
            public int MoneyAmount2 { get; set; } = 1000;
            public int MoneyAmount3 { get; set; } = 5000;
            public int MoneyAmount4 { get; set; } = 10000;
            public int DayLength1 { get; set; } = 3;
            public int DayLength2 { get; set; } = 7;
            public int DayLength3 { get; set; } = 14;
            public int DayLength4 { get; set; } = 28;
            public bool Reset { get; set; } = false;
        }
    }
}