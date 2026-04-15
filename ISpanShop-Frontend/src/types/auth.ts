export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  memberId: number;
  email: string;
  account: string;
  memberName: string;
  levelName: string;
  pointBalance: number;
}

export interface RegisterRequest {
  account: string;
  password: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
}
