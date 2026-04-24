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

export interface LevelRule {
  id: number;
  levelName: string;
  minSpending: number;
  discountRate: number;
}

export interface LevelInfo {
  userId: number;
  totalSpending: number;
  currentLevelName: string;
  levels: LevelRule[];
}

export interface LevelDetailRule {
  levelId: number;
  name: string;
  minSpending: number;
  discountRate: number;
}

export interface LevelDetail {
  currentTotalSpending: number;
  currentLevelName: string;
  nextLevelName: string | null;
  nextLevelThreshold: number;
  progressPercent: number;
  allLevels: LevelDetailRule[];
  calculationStartDate: string;
  calculationEndDate: string;
}
