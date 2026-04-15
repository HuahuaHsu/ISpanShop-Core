// ─── 商品詳情相關型別 ───────────────────────────────────────────

export interface CategoryPathItem {
  id: number
  name: string
}

export interface BrandInfo {
  id: number
  name: string
  logoUrl: string | null
}

export interface StoreInfo {
  id: number
  name: string
  logoUrl: string | null
  rating: number | null
  productCount: number
  followerCount: number | null
  location: string | null
  responseRate: number | null
  joinedYearsAgo: number
}

export interface ProductImage {
  id: number
  url: string
  isMain: boolean
  sortOrder: number
}

export interface PriceRange {
  min: number
  max: number
}

export interface ProductSpec {
  name: string
  options: string[]
}

export interface ProductVariant {
  id: number
  specValues: Record<string, string>
  price: number
  originalPrice: number | null
  stock: number
  imageUrl: string | null
}

export interface ProductDetail {
  id: number
  name: string | null
  description: string | null
  categoryId: number
  categoryPath: CategoryPathItem[] | null
  brand: BrandInfo | null
  store: StoreInfo | null
  images: ProductImage[] | null
  priceRange: PriceRange | null
  originalPriceRange: PriceRange | null
  discountRate: number | null
  specs: ProductSpec[] | null
  variants: ProductVariant[] | null
  totalStock: number | null
  soldCount: number | null
  rating: number | null
  reviewCount: number | null
  isOnShelf: boolean
  createdAt: string | null
}

// ─── 商品列表相關型別 ───────────────────────────────────────────

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
