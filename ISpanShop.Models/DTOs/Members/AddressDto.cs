namespace ISpanShop.Models.DTOs.Members;

public class AddressDto
{
    public int Id { get; set; }
    public string RecipientName { get; set; }
    public string RecipientPhone { get; set; }
    public string City { get; set; }
    public string Region { get; set; }
    public string Street { get; set; }
    public bool? IsDefault { get; set; }
}
