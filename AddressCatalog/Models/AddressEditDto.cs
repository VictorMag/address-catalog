using System.ComponentModel.DataAnnotations;

namespace AddressCatalog.Models;

public class AddressEditDto
{
    [Required]
    public int AddressID { get; set; }

    [Required(ErrorMessage = "La ciudad es requerida")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado / provincia es requerido")]
    public string StateProvince { get; set; } = string.Empty;
}
