/** 對應後端 ProductListItemDto */
export interface ProductListItem {
  id: number
  name: string
  /** 多規格取最低價 */
  price: number
  /** 原價（目前後端固定回傳 null） */
  originalPrice: number | null
  imageUrl: string
  soldCount: number
  location: string
  categoryId: number
  /** 平均評分（目前後端固定回傳 null） */
  rating: number | null
}

/** 對應後端 ProductListResponseDto */
export interface ProductListResponse {
  items: ProductListItem[]
  total: number
  page: number
  pageSize: number
}

/** 統一 API 回傳包裝 */
export interface ApiResponse<T> {
  success: boolean
  data: T
  message: string
}

/** GET /api/products 查詢參數 */
export interface FetchProductsParams {
  page?: number
  pageSize?: number
  sortBy?: 'latest' | 'priceAsc' | 'priceDesc' | 'soldCount'
  categoryId?: number
  subCategoryId?: number
  brandIds?: number[]
  minPrice?: number
  maxPrice?: number
  keyword?: string
}
