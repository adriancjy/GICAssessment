using System;
using System.Collections.Generic;
using System.Globalization;

class Program
{
    static Dictionary<string, List<Transaction>> accounts = new Dictionary<string, List<Transaction>>();
    static List<InterestRule> interestRules = new List<InterestRule>();

    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
        while (true)
        {
            Console.WriteLine("[T] Input transactions");
            Console.WriteLine("[I] Define interest rules");
            Console.WriteLine("[P] Print statement");
            Console.WriteLine("[Q] Quit");
            var action = Console.ReadLine().Trim().ToLower();

            switch (action)
            {
                case "t":
                    InputTransactions();
                    break;
                case "i":
                    DefineInterestRules();
                    break;
                case "p":
                    PrintStatement();
                    break;
                case "q":
                    Console.WriteLine("Thank you for banking with AwesomeGIC Bank.\nHave a nice day!");
                    return;
                default:
                    Console.WriteLine("Invalid input, please try again.");
                    break;
            }
        }
    }

    static void InputTransactions()
    {
        while (true)
        {
            Console.WriteLine("Please enter transaction details in <Date> <Account> <Type> <Amount> format");
            Console.WriteLine("(or enter blank to go back to main menu):");
            var input = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                break; // Go back to main menu
            }

            var parts = input.Split();
            if (parts.Length != 4)
            {
                Console.WriteLine("Invalid input format. Please try again.");
                continue;
            }

            DateTime date;
            if (!DateTime.TryParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                Console.WriteLine("Invalid date format. Please try again.");
                continue;
            }

            var accountId = parts[1];
            var type = parts[2].ToUpper();
            decimal amount;
            if (!decimal.TryParse(parts[3], out amount) || amount <= 0)
            {
                Console.WriteLine("Invalid amount. Please enter a positive number.");
                continue;
            }

            if (type != "D" && type != "W")
            {
                Console.WriteLine("Invalid transaction type. Use 'D' for deposit and 'W' for withdrawal.");
                continue;
            }

            if (!accounts.ContainsKey(accountId))
            {
                accounts[accountId] = new List<Transaction>();
            }

            var accountTransactions = accounts[accountId];
            decimal currentBalance = GetBalance(accountTransactions);

            if (type == "W" && currentBalance < amount)
            {
                Console.WriteLine("Insufficient funds. Transaction cannot be processed.");
                continue;
            }

            string transactionId = GenerateTransactionId(date, accountTransactions.Count + 1);
            var transaction = new Transaction
            {
                Date = date,
                AccountId = accountId,
                Type = type,
                Amount = amount,
                TransactionId = transactionId
            };

            accountTransactions.Add(transaction);
            PrintAccountStatement(accountId);
        }
    }

    static void DefineInterestRules()
    {
        while (true)
        {
            Console.WriteLine("Please enter interest rules details in <Date> <RuleId> <Rate in %> format");
            Console.WriteLine("(or enter blank to go back to main menu):");
            var input = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                break; // Go back to main menu
            }

            var parts = input.Split();
            if (parts.Length != 3)
            {
                Console.WriteLine("Invalid input format. Please try again.");
                continue;
            }

            DateTime date;
            if (!DateTime.TryParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                Console.WriteLine("Invalid date format. Please try again.");
                continue;
            }

            var ruleId = parts[1];
            decimal rate;
            if (!decimal.TryParse(parts[2], out rate) || rate <= 0 || rate >= 100)
            {
                Console.WriteLine("Invalid rate. Please enter a value between 0 and 100.");
                continue;
            }

            interestRules.RemoveAll(r => r.Date == date);
            interestRules.Add(new InterestRule { Date = date, RuleId = ruleId, Rate = rate });

            interestRules.Sort((x, y) => x.Date.CompareTo(y.Date));

            Console.WriteLine("Interest rules:");
            Console.WriteLine("| Date     | RuleId | Rate (%) |");
            foreach (var rule in interestRules)
            {
                Console.WriteLine($"| {rule.Date:yyyyMMdd} | {rule.RuleId} | {rule.Rate:0.00} |");
            }
        }
    }

static void PrintStatement()
{
    Console.WriteLine("Please enter account and month to generate the statement <Account> <Year><Month>");
    Console.WriteLine("(or enter blank to go back to main menu):");
    var input = Console.ReadLine().Trim();

    if (string.IsNullOrWhiteSpace(input))
    {
        return; // Go back to main menu
    }

    var parts = input.Split();
    if (parts.Length != 2)
    {
        Console.WriteLine("Invalid input format. Please try again.");
        return;
    }

    var accountId = parts[0];
    var yearMonth = parts[1];

    DateTime statementMonth;
    if (!DateTime.TryParseExact(yearMonth, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out statementMonth))
    {
        Console.WriteLine("Invalid date format. Please try again.");
        return;
    }

    if (!accounts.ContainsKey(accountId))
    {
        Console.WriteLine("Account not found.");
        return;
    }

    var accountTransactions = accounts[accountId];
    decimal balance = 0;

    var eodBalances = new Dictionary<DateTime, decimal>();
    DateTime startOfMonth = new DateTime(statementMonth.Year, statementMonth.Month, 1);
    DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

    for (var day = startOfMonth; day <= endOfMonth; day = day.AddDays(1))
    {
        eodBalances[day] = balance; 
    }

    foreach (var txn in accountTransactions)
    {
        if (txn.Date.Year == statementMonth.Year && txn.Date.Month == statementMonth.Month)
        {
            if (txn.Type == "D")
            {
                balance += txn.Amount;
            }
            else if (txn.Type == "W")
            {
                balance -= txn.Amount;
            }

            eodBalances[txn.Date] = balance;
        }
    }

    DateTime lastBalanceDate = startOfMonth;
    foreach (var day in eodBalances.Keys)
    {
        if (eodBalances[day] == 0 && day > lastBalanceDate)
        {
            eodBalances[day] = eodBalances[lastBalanceDate]; 
        }
        lastBalanceDate = day;
    }

    Console.WriteLine($"Account: {accountId}");
    Console.WriteLine("| Date     | Txn Id      | Type | Amount | Balance |");

    foreach (var txn in accountTransactions)
    {
        if (txn.Date.Year == statementMonth.Year && txn.Date.Month == statementMonth.Month)
        {
            Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TransactionId} | {txn.Type}    | {txn.Amount:0.00} | {eodBalances[txn.Date]:0.00} |");
        }
    }

    decimal interest = CalculateInterest(eodBalances, statementMonth);
    balance += interest;
    if (interest > 0)
    {
        Console.WriteLine($"| {endOfMonth:yyyyMMdd} |          | I    | {interest:0.00} | {balance:0.00} |");
    }
}


    static decimal CalculateInterest(Dictionary<DateTime, decimal> eodBalances, DateTime statementMonth)
{
    decimal totalInterest = 0;
    decimal currentRate = 0;

    DateTime startOfMonth = new DateTime(statementMonth.Year, statementMonth.Month, 1);
    DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

    var sortedInterestRules = interestRules.FindAll(r => r.Date <= endOfMonth);
    if (sortedInterestRules.Count == 0)
    {
        return 0; 
    }

    sortedInterestRules.Sort((x, y) => x.Date.CompareTo(y.Date));

    DateTime currentPeriodStart = startOfMonth;
    foreach (var rule in sortedInterestRules)
    {
        DateTime currentPeriodEnd = rule.Date.AddDays(-1);

        if (currentPeriodStart <= currentPeriodEnd)
        {
            totalInterest += CalculateInterestForPeriod(eodBalances, currentPeriodStart, currentPeriodEnd, currentRate);
        }

    
        currentRate = rule.Rate;
        currentPeriodStart = rule.Date;
    }

    
    totalInterest += CalculateInterestForPeriod(eodBalances, currentPeriodStart, endOfMonth, currentRate);

    return Math.Round(totalInterest / 365, 2); 
}

static decimal CalculateInterestForPeriod(Dictionary<DateTime, decimal> eodBalances, DateTime start, DateTime end, decimal rate)
{
    decimal interest = 0;
    for (var day = start; day <= end; day = day.AddDays(1))
    {
        if (eodBalances.ContainsKey(day))
        {
            interest += eodBalances[day] * rate / 100;
        }
    }
    return interest;
}


    static decimal GetBalance(List<Transaction> transactions)
    {
        decimal balance = 0;
        foreach (var txn in transactions)
        {
            balance = txn.Type == "D" ? balance + txn.Amount : balance - txn.Amount;
        }
        return balance;
    }

    static string GenerateTransactionId(DateTime date, int sequence)
    {
        return $"{date:yyyyMMdd}-{sequence:D2}";
    }

    static void PrintAccountStatement(string accountId)
    {
        if (accounts.ContainsKey(accountId))
        {
            var accountTransactions = accounts[accountId];
            Console.WriteLine($"Account: {accountId}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount |");
            foreach (var txn in accountTransactions)
            {
                Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TransactionId} | {txn.Type}    | {txn.Amount:0.00} |");
            }
        }
    }
}

class Transaction
{
    public DateTime Date { get; set; }
    public string AccountId { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public string TransactionId { get; set; }
}

class InterestRule
{
    public DateTime Date { get; set; }
    public string RuleId { get; set; }
    public decimal Rate { get; set; }
}
