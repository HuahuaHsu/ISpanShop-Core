export interface OrderListItem {
  id: number;
  orderNumber: string;
  createdAt: string;
  finalAmount: number;
  status: number;
  statusName: string;
  storeName: string;
  firstProductName: string;
  firstProductImage: string;
  totalItemCount: number;
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
  subTotal: number;
}

export interface OrderDetail {
  id: number;
  orderNumber: string;
  receiverName: string;
  receiverPhone: string;
  receiverAddress: string;
  finalAmount: number;
  /** 
   * ?€?‹å??‰ï?
   * 0: å¾…ä?æ¬? 1: å¾…å‡ºè²? 2: å¾…æ”¶è²? 3: å·²å??? 4: å·²å?æ¶?   */
  status: number;
  createdAt: string;
  totalAmount: number;
  paymentDate: string | null;
  completedAt: string | null;
  shippingFee: number | null;
  finalAmount: number;
  status: number;
  statusName: string;
  createdAt: string;
  storeName: string;
}
  recipientName: string;
  recipientPhone: string;
  recipientAddress: string;
  note: string;
  items: OrderItem[];
