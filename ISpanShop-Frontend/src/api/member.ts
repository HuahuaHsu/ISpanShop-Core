import axios from './axios';

export interface MemberDto {
  id: number;
  account: string;
  email: string;
  fullName: string;
  phoneNumber: string;
  gender: number | null;
  birthday: string | null;
  pointBalance?: number;
  levelName?: string;
  avatarUrl?: string;
}

/**
 * 取得會員個人資料
 * 注意：路徑需符合後端 [Route("api/front/profile")]
 */
export const getMemberProfile = (id: number) => {
  return axios.get<MemberDto>(`/api/front/profile/${id}`);
};

/**
 * 更新會員個人資料
 */
export const updateMemberProfile = (id: number, data: MemberDto) => {
  return axios.put<{ message: string }>(`/api/front/profile/${id}`, data);
};
