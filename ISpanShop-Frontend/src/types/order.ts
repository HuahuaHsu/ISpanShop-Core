export interface OrderListItem {
  id: number;
  orderNumber: string;
  createdAt: string;
  finalAmount: number;
  discountAmount?: number;
  levelDiscount?: number;
  pointDiscount?: number;
  promotionDiscount?: number;
  status: number;
  statusName: string;
  storeName: string;
  storeId: number;
  sellerId: number;
  firstProductName: string;
  firstProductImage: string;
  productNames: string;
  totalItemCount: number;
  isReviewed: boolean;
}

export interface OrderItem {
  id: number;
  productId: number;
  variantId: number | null;
  productName: string;
  variantName: string;
  coverImage: string;
  price: number;
  quantity: number;
  promotionTags?: string[];
}

export interface OrderDetail {
  id: number;
  orderNumber: string;
  createdAt: string;
  paymentDate: string | null;
  completedAt: string | null;
  totalAmount: number;
  shippingFee: number | null;
  pointDiscount: number | null;
  discountAmount: number | null;
  levelDiscount: number | null;
  couponId: number | null;
  couponTitle: string | null;
  promotionDiscount?: number | null;
  finalAmount: number;
  status: number;
  statusName: string;
  storeId: number;
  sellerId: number;
  storeName: string;
  recipientName: string;
  recipientPhone: string;
  recipientAddress: string;
  note: string;
  items: OrderItem[];
  isReviewed: boolean;
  hasAppealed: boolean;
  returnInfo?: ReturnDetail;
}

export interface ReturnDetail {
  reasonCategory: string;
  reasonDescription: string;
  refundAmount: number;
  status: number;
  createdAt: string;
  imageUrls: string[];
  items: ReturnItem[];
}

export interface ReturnItem {
  productName: string;
  variantName: string;
  coverImage: string;
  price: number;
  returnQuantity: number;
  promotionTags?: string[];
}
