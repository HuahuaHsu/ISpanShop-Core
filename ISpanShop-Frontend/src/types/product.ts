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
  userId: number
  name: string
  status: number
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
  attributesJson: string | null
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
  totalStock: number
  location: string
  categoryId: number
  /** 平均評分（目前後端固定回傳 null） */
  rating: number | null
}

/** 對應後端 ProductListResponseDto */
export interface ProductListResponse {
  items: ProductListItem[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

/** 統一 API 回傳包裝 */
export interface ApiResponse<T> {
  success: boolean
  data: T
  message: string
}

/** 賣家商品列表項目（對應後端 GET /api/seller/products 回傳）
 *  status: 0=未上架, 1=已上架, 2=待審核, 3=審核退回
 *  reviewStatus: 0=待審核, 1=通過, 2=退回, 3=重新送審
 */
export interface SellerProductListItem {
  id: number
  name: string
  storeName: string
  categoryName: string
  brandName: string
  minPrice: number | null
  maxPrice: number | null
  status: number
  statusText: string
  mainImageUrl: string | null
  createdAt: string | null
  rejectReason: string | null
  reviewStatus: number
  isDeleted?: boolean
  // TODO: 以下欄位後端尚未回傳，補上後移除 nullable
  viewCount: number | null
  totalSales: number | null
  cartCount: number | null
  totalStock: number | null
}

export interface SellerProductListResponse {
  items: SellerProductListItem[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

/** 賣家商品編輯用詳情（對應後端 GET /api/seller/products/{id} 直接回傳，無 ApiResponse 包裝）
 *  status: 0=未上架, 1=已上架, 2=待審核, 3=審核退回
 *  reviewStatus: 0=待審核, 1=通過, 2=退回, 3=重新送審
 */
export interface SellerProductDetail {
  id: number
  name: string
  storeId: number
  storeName: string | null
  categoryId: number
  categoryName: string | null
  brandId: number | null
  brandName: string | null
  description: string | null
  status: number
  statusText: string
  minPrice: number | null
  maxPrice: number | null
  specDefinitionJson: string | null
  rejectReason: string | null
  reviewStatus: number
  createdAt: string | null
  updatedAt: string | null
  images: string[]
  variants: SellerVariantDetail[]
}

export interface SellerVariantDetail {
  id: number
  skuCode: string | null
  variantName: string | null
  price: number
  stock: number | null
  safetyStock: number | null
  specValueJson: string | null
}

/** 商品活動促銷規則 */
export interface ProductPromotionRule {
  ruleType: number
  threshold: number
  discountType: number
  discountValue: number
}

/** 商品目前參加的活動（對應 GET /api/promotions/product/{productId} 回傳）
 *  type: flashSale=限時特賣 / discount=滿額折扣 / limitedBuy=限量搶購
 */
export interface ProductPromotion {
  id: number
  title: string
  type: 'flashSale' | 'discount' | 'limitedBuy' | 'other'
  typeLabel: string
  endDate: string
  originalPrice: number
  discountPrice: number | null
  discountPercent: number | null
  quantityLimit: number | null
  stockLimit: number | null
  soldCount: number
  remainingStock: number | null
  linkUrl: string
  rule: ProductPromotionRule | null
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
