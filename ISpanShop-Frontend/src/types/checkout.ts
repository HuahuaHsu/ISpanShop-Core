import type { CartItem } from '../stores/cart';

export interface CheckoutRequest {
  storeId: number;
  usePoints: boolean;
  items: CheckoutCartItem[];
  recipientName: string;
  recipientPhone: string;
  recipientAddress: string;
}

export interface CheckoutCartItem {
  productId: number;
  variantId: number;
  productName: string;
  variantName: string;
  unitPrice: number;
  quantity: number;
}

export interface CheckoutResponse {
  success: boolean;
  message: string;
  orderNumber: string;
  paymentUrl: string;
}
