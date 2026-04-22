export interface AddressDto {
  id: number;
  recipientName: string;
  recipientPhone: string;
  city: string;
  region: string;
  street: string;
  isDefault: boolean;
}

export interface CreateAddressDto {
  recipientName: string;
  recipientPhone: string;
  city: string;
  region: string;
  street: string;
  isDefault: boolean;
}

export interface UpdateAddressDto extends CreateAddressDto {
  id: number;
}
