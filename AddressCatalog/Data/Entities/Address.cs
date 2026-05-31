using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AddressCatalog.Data.Entities;

[Table("Address", Schema = "SalesLT")]
public class Address
{
    [Key]
    public int AddressID { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string CountryRegion { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public Guid rowguid { get; set; }
    public DateTime ModifiedDate { get; set; }
}
