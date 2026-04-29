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
  levelDiscount?: number; // 會員等級折扣金額
  promotionDiscount?: number; // 賣場活動促銷折扣總額
  items: CheckoutItem[];
  recipientName: string;
  recipientPhone: string;
  recipientAddress: string;
  paymentMethod?: string;
}

export const checkoutApi = {
  createOrder(data: CheckoutRequest) {
    return instance.post('/api/checkout', data);
  },

  /** 獲取既有訂單的支付路徑 (不產生新訂單) */
  getRepaymentUrl(orderId: number, paymentMethod: string = 'ECPay') {
    return instance.get<{ success: boolean, paymentUrl: string }>(`/api/checkout/repay/${orderId}`, {
      params: { paymentMethod }
    });
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
