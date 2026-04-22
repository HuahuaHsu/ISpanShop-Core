export interface LoginRequest {
  account: string;
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
  isSeller: boolean;
  avatarUrl?: string;
}

export interface RegisterRequest {
  account: string;
  password: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string; // 從 URL query 取得
  newPassword: string;
  confirmPassword: string;
}
