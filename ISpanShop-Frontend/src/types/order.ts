/**
 * 訂單明細項目介面
 */
export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  variantName: string;
  imageUrl: string;
  unitPrice: number;
  quantity: number;
  subTotal: number;
}

/**
 * 訂單資訊介面 (對應後端 Orders 表)
 */
export interface Order {
  id: string; // 使用 string 處理 bigint
  orderNumber: string;
  receiverName: string;
  receiverPhone: string;
  receiverAddress: string;
  finalAmount: number;
  /** 
   * 狀態對應：
   * 0: 待付款, 1: 待出貨, 2: 待收貨, 3: 已完成, 4: 已取消
   */
  status: number;
  createdAt: string;
  items: OrderItem[];
}

/**
 * 訂單列表項目 (精簡版)
 */
export interface OrderListItem {
  id: number;
  orderNumber: string;
  totalAmount: number;
  status: number;
  statusName: string;
  createdAt: string;
  storeName: string;
}

/**
 * 訂單列表分頁回應
 */
export interface OrderListResponse {
  items: OrderListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
