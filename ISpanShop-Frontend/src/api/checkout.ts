import instance from './axios';

export interface CheckoutItem {
  productId: number;
  variantId: number;
  unitPrice: number;
  quantity: number;
  productName: string;
  variantName: string;
}

export interface CheckoutRequest {
  userId: number;
  storeId: number;
  usePoints: boolean;
  couponId: number | null;
  items: CheckoutItem[];
  recipientName: string;
  recipientPhone: string;
  recipientAddress: string;
}

export const checkoutApi = {
  createOrder(data: CheckoutRequest) {
    return instance.post('/api/checkout', data);
  },
  
  getAvailableCoupons(storeId: number, subtotal: number, productIds: number[]) {
    return instance.get('/api/coupon/available', {
      params: {
        storeId,
        subtotal,
        productIds: productIds.join(',')
      }
    });
  },

  getWalletBalance() {
    return instance.get('/api/member/wallet-balance'); // 假設有這個 endpoint
  }
};
