export interface StoreApplyRequest {
  storeName: string
  description?: string
  logoUrl?: string
}

export type StoreStatus = 'NotApplied' | 'Pending' | 'Approved' | 'Rejected'

export interface StoreStatusResponse {
  status: StoreStatus
}

export interface StoreProfileData {
  storeName: string
  description: string
  logoUrl: string
  storeStatus: number // 1: 營業中, 2: 休假中
}

export interface SellerDashboardData {
  kpis: {
    totalRevenue: number
    totalOrders: number
    ordersLast7Days: number
    ordersGrowthRate: string
    ordersGrowthType: 'up' | 'down' | 'neutral'
    pendingOrders: number
    pendingRefundCount: number
    totalProducts: number
    lowStockCount: number
  }
  salesTrend: {
    labels: string[]
    series: Array<{
      name: string
      data: number[]
    }>
  }
  topProducts: Array<{
    productName: string
    salesVolume: number
    salesRevenue: number
  }>
  recentOrders: RecentOrder[]
}

export interface RecentOrder {
  orderId: number
  orderNumber: string
  buyerName: string
  productName: string
  amount: number
  status: string
  createdAt: string
}

export interface SellerOrder {
  id: number
  orderNumber: string
  createdAt: string
  finalAmount: number
  status: number
  statusName: string
  buyerName: string
  recipientName: string
  firstProductName: string
  firstProductImage: string
  totalItemCount: number
}

export interface SellerOrderDetail {
  id: number
  orderNumber: string
  createdAt: string
  paymentDate: string | null
  completedAt: string | null
  status: number
  statusName: string
  
  userId: number
  buyerAccount: string
  buyerName: string
  buyerPhone: string
  buyerEmail: string

  recipientName: string
  recipientPhone: string
  recipientAddress: string
  note: string

  totalAmount: number
  shippingFee: number
  discountAmount: number
  pointDiscount: number
  finalAmount: number

  items: SellerOrderItem[]
}

export interface SellerOrderItem {
  id: number
  productId: number
  variantId: number | null
  productName: string
  variantName: string
  skuCode: string
  coverImage: string
  price: number
  quantity: number
  subtotal: number
}

export interface SellerReturnItem {
  id: number
  orderNumber: string
  buyerName: string
  refundAmount: number
  reasonCategory: string
  createdAt: string
  status: number
  statusName: string
}

export interface SellerReturnDetail {
  orderId: number
  orderNumber: string
  orderCreatedAt: string
  reasonCategory: string
  reasonDescription: string
  refundAmount: number
  requestCreatedAt: string
  resolvedAt: string | null
  status: number
  statusName: string
  imageUrls: string[]
  buyerAccount: string
  items: SellerOrderItem[]
}
