using System;
using System.Collections.Generic;
using PMPRacing.Models;

namespace PMPRacing.ViewModels;

public class MechanicDashboardVm
{
    public List<AssignedJobVm> IncomingJobs { get; set; } = new();
    public List<ActiveJobVm> ProcessingJobs { get; set; } = new();
    public List<WorkSchedule> MySchedule { get; set; } = new();
}

public class AssignedJobVm
{
    public int ReceiptId { get; set; }
    public string CustomerName { get; set; } = "";
    public string BikeModel { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public DateTime ReceivedAt { get; set; }
    public string? Note { get; set; }
}

public class ActiveJobVm
{
    public int OrderId { get; set; }
    public int ReceiptId { get; set; }
    public string CustomerName { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal TotalPrice { get; set; }
}

public class JobDetailVm
{
    public int OrderId { get; set; }
    public int ReceiptId { get; set; }
    public string CustomerName { get; set; } = "";
    public string BikeModel { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public string Status { get; set; } = "";
    
    public List<OrderPart> SelectedParts { get; set; } = new();
    public List<ServiceOrderItem> SelectedServices { get; set; } = new();
    
    public decimal PartsTotal { get; set; }
    public decimal ServicesTotal { get; set; }
    public decimal GrandTotal => PartsTotal + ServicesTotal;
}

public class AddPartPostVm
{
    public int OrderId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
}

public class AddServicePostVm
{
    public int OrderId { get; set; }
    public int ServiceId { get; set; }
    public decimal Price { get; set; }
}
