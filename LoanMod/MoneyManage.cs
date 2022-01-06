using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanMod
{
    class MoneyManage
    {
        /// <summary>
        /// <param name="IsBorrowing">Indicates whether the player is currently borrowing money.</param>
        /// </summary>
        public bool IsBorrowing { get; set; } = false; 
        /// <summary>
        /// <param name="AmountBorrowed">Shows the amount of money the player is borrowing.</param>
        /// </summary>
        public int AmountBorrowed { get; set; } = 0;
        /// <summary>
        /// <param name="Duration">Shows the duration (in days) of the loan.</param>
        /// </summary>
        public int Duration { get; set; } = 0;
        /// <summary>
        /// <param name="Interest">Shows the interest rate of the loan.</param>
        /// </summary>
        public float Interest { get; set; } = 0;
        /// <summary>
        /// <param name="AmountRepaid">Shows the amount of money the player has already repayed.</param>
        /// </summary>
        public int AmountRepaid { get; set; } = 0;
        /// <summary>
        /// <param name="hasPaid">Indicates if the player has already made a payment on the current day.</param>
        /// </summary>
        public bool hasPaid { get; set; } = false;
        /// <summary>
        /// <param name="DailyAmount">Shows the daily repayment amount.</param>
        /// </summary>
        public int DailyAmount { get; set; } = 0;
        /// <summary>
        /// <param name="ShowBalance">Shows the current balance remaining to be paid off.</param>
        /// </summary>
        public int Balance { get; set; } = 0;
        /// <summary>
        /// <param name="CalculateBalance">Calculates the current balance remaining to be paid off.</param>
        /// </summary>
        internal double CalculateBalance
        {
            get
            {
                double bal = (AmountBorrowed - AmountRepaid);
                double balinterest = bal * Interest;

                var result = Math.Round(bal + balinterest, MidpointRounding.AwayFromZero);

                return result;
            }
        }
        /// <summary>
        /// <param name="CalculateDailyAmount">Calculates the daily amount based on the balance left to pay.</param>
        /// </summary>
        internal double CalculateDailyAmount
        {
            get
            {
                double daily = CalculateBalance / Duration;

                var result = Math.Round(daily, MidpointRounding.AwayFromZero);

                return result;
            }
        }
        /// <summary>
        /// <param name="InitiateReset">Resets the mod.</param>
        /// </summary>
        internal void InitiateReset()
        {
            IsBorrowing = false;
            AmountBorrowed = 0;
            Duration = 0;
            Interest = 0;
            AmountRepaid = 0;
            hasPaid = false;
            Balance = 0;
            DailyAmount = 0;
        }

    }
}
