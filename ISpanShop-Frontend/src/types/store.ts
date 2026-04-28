export interface StoreApplyRequest {
  storeName: string
  description?: string
  logoUrl?: string
}

export type StoreStatus = 'NotApplied' | 'Pending' | 'Approved' | 'Rejected' | 'Suspended'

export interface StoreStatusResponse {
  status: StoreStatus
  isBanned: boolean // TODO: 需要後端在 GET /api/front/store/status 回傳此欄位
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
  discountAmount?: number
  levelDiscount?: number
  pointDiscount?: number
  promotionDiscount?: number
  status: number
  statusName: string
  buyerName: string
  buyerId: number
  recipientName: string
  firstProductName: string
  firstProductImage: string
  totalItemCount: number
  hasReview: boolean
  hasReplied: boolean
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
  levelDiscount?: number // 會員等級折抵
  pointDiscount: number
  promotionDiscount?: number // 活動促銷折抵
  finalAmount: number

  items: SellerOrderItem[]
  review: OrderReview | null
}

export interface OrderReview {
  id: number
  userId: number
  orderId: number
  rating: number
  comment: string
  storeReply: string | null
  createdAt: string
  imageUrls: string[]
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

  // 訂單財務資訊
  totalAmount: number
  shippingFee: number | null
  levelDiscount: number | null
  discountAmount: number | null
  pointDiscount: number | null
  promotionDiscount?: number | null // 活動促銷折抵
  finalAmount: number

  items: SellerOrderItem[]
}
