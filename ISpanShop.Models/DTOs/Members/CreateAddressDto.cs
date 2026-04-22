using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Members;

public class CreateAddressDto
{
    [Required]
    [MaxLength(50)]
    public string RecipientName { get; set; }

    [Required]
    [MaxLength(20)]
    public string RecipientPhone { get; set; }

    [Required]
    [MaxLength(50)]
    public string City { get; set; }

    [Required]
    [MaxLength(50)]
    public string Region { get; set; }

    [Required]
    [MaxLength(200)]
    public string Street { get; set; }

    public bool IsDefault { get; set; }
}
