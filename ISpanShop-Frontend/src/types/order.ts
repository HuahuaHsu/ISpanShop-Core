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
}

export interface OrderDetail {
  id: number;
  orderNumber: string;
  createdAt: string;
  paymentDate: string | null;
  completedAt: string | null;
  totalAmount: number;
  shippingFee: number | null;
  finalAmount: number;
  status: number;
  statusName: string;
  storeName: string;
  recipientName: string;
  recipientPhone: string;
  recipientAddress: string;
  note: string;
  items: OrderItem[];
}
