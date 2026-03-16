namespace MaxPlus.IPTV.Core.Entities;

public class Customer
{
    public Guid     Id             { get; set; }
    public string   Name           { get; set; } = string.Empty;
    public string?  Phone          { get; set; }
    public string?  Address        { get; set; }
    public string?  Email          { get; set; }

    public ICollection<CustomerSubscription> Subscriptions { get; set; } = new List<CustomerSubscription>();

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
