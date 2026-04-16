import request from './request';

export interface Coupon {
  id: number;
  title: string;
  couponCode: string;
  couponType: number;
  discountValue: number;
  minimumSpend: number;
  startTime: string;
  endTime: string;
  totalQuantity: number;
  usedQuantity: number;
  perUserLimit: number;
  isClaimed: boolean;
}

export const getPublicCoupons = () => {
  return request.get<Coupon[]>('/api/coupon/public-list');
};

export const claimCoupon = (id: number) => {
  return request.post<{ message: string }>(`/api/coupon/claim/${id}`);
};

export const getMyCoupons = () => {
  return request.get<any[]>('/api/coupon/mine');
};
