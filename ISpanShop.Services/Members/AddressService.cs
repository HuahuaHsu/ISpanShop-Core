using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Members;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.Services.Members
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repo;

        public AddressService(IAddressRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<IEnumerable<AddressDto>> GetAddressListAsync(int userId)
        {
            var addresses = await _repo.GetAllByUserIdAsync(userId);
            return addresses.Select(MapToDto);
        }

        public async Task<AddressDto> CreateAddressAsync(int userId, CreateAddressDto dto)
        {
            var addresses = await _repo.GetAllByUserIdAsync(userId);
            bool isFirstAddress = !addresses.Any();

            var address = new Address
            {
                UserId = userId,
                RecipientName = dto.RecipientName,
                RecipientPhone = dto.RecipientPhone,
                City = dto.City,
                Region = dto.Region,
                Street = dto.Street,
                IsDefault = dto.IsDefault || isFirstAddress
            };

            await _repo.AddAsync(address);

            if (address.IsDefault == true)
            {
                await _repo.SetDefaultAsync(address.Id, userId);
                // Re-fetch to get updated state if needed, but for creation we can just return the dto
            }

            return MapToDto(address);
        }

        public async Task<bool> UpdateAddressAsync(int userId, UpdateAddressDto dto)
        {
            var address = await _repo.GetByIdAsync(dto.Id, userId);
            if (address == null) return false;

            address.RecipientName = dto.RecipientName;
            address.RecipientPhone = dto.RecipientPhone;
            address.City = dto.City;
            address.Region = dto.Region;
            address.Street = dto.Street;
            
            // If it was default and now it's not, we might need to be careful, 
            // but usually we set another one as default.
            // If it's being set to default, use the repo method to clear others.
            if (dto.IsDefault && address.IsDefault != true)
            {
                address.IsDefault = true;
                await _repo.UpdateAsync(address);
                await _repo.SetDefaultAsync(address.Id, userId);
            }
            else
            {
                // If it was default and we are trying to set it to false, 
                // we should probably check if it's the only one.
                // For simplicity, we just update it.
                address.IsDefault = dto.IsDefault;
                await _repo.UpdateAsync(address);
            }

            return true;
        }

        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {
            var address = await _repo.GetByIdAsync(addressId, userId);
            if (address == null) return false;

            bool wasDefault = address.IsDefault == true;

            await _repo.DeleteAsync(addressId, userId);

            if (wasDefault)
            {
                var remaining = await _repo.GetAllByUserIdAsync(userId);
                if (remaining.Any())
                {
                    await _repo.SetDefaultAsync(remaining.First().Id, userId);
                }
            }

            return true;
        }

        public async Task<bool> SetDefaultAddressAsync(int userId, int addressId)
        {
            var address = await _repo.GetByIdAsync(addressId, userId);
            if (address == null) return false;

            await _repo.SetDefaultAsync(addressId, userId);
            return true;
        }

        private AddressDto MapToDto(Address address)
        {
            return new AddressDto
            {
                Id = address.Id,
                RecipientName = address.RecipientName,
                RecipientPhone = address.RecipientPhone,
                City = address.City,
                Region = address.Region,
                Street = address.Street,
                IsDefault = address.IsDefault
            };
        }
    }
}
