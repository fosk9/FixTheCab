using System;
using System.Collections.Generic;

namespace PMPRacing.Models;

public partial class SystemBankAccount
{
    public int AccountId { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string? QrImageCode { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
