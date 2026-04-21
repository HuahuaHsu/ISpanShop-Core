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
    pendingOrders: number
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
}
