import request from './request';

export interface Coupon {
  id: number;
  title: string;
  couponCode: string;
  couponType: number;
  discountValue: number;
  minimumSpend: number;
  maximumDiscount?: number;
  startTime: string;
  endTime: string;
  totalQuantity: number;
  usedQuantity: number;
  perUserLimit: number;
  isClaimed?: boolean;
  status?: number;
}

// ─── 會員端 ───
export const getPublicCoupons = () => {
  return request.get<Coupon[]>('/api/coupon/public-list');
};

export const claimCoupon = (id: number) => {
  return request.post<{ message: string }>(`/api/coupon/claim/${id}`);
};

export const getMyCoupons = () => {
  return request.get<any[]>('/api/coupon/mine');
};

// ─── 賣家端 ───
export const fetchSellerCoupons = () => {
  return request.get<{ success: boolean; data: Coupon[] }>('/api/seller/coupons');
};

export const createSellerCoupon = (data: Partial<Coupon>) => {
  return request.post<{ success: boolean; message: string }>('/api/seller/coupons', data);
};

export const updateSellerCoupon = (id: number, data: Partial<Coupon>) => {
  return request.put<{ success: boolean; message: string }>(`/api/seller/coupons/${id}`, data);
};

export const deleteSellerCoupon = (id: number) => {
  return request.delete<{ success: boolean; message: string }>(`/api/seller/coupons/${id}`);
};
