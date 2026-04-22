import request from './request'
import type {
  ApiResponse,
  FetchProductsParams,
  ProductListResponse,
  ProductDetail,
  ProductListItem,
  SellerProductListResponse,
} from '@/types/product'

export async function fetchProductList(
  params: FetchProductsParams = {},
): Promise<ApiResponse<ProductListResponse>> {
  const response = await request.get<ApiResponse<ProductListResponse>>('/api/products', { params })
  return response.data
}

export async function fetchProductDetail(id: number): Promise<ApiResponse<ProductDetail>> {
  const response = await request.get<ApiResponse<ProductDetail>>(`/api/products/${id}`)
  return response.data
}

export async function fetchRelatedProducts(
  id: number,
  limit = 12,
): Promise<ApiResponse<ProductListItem[]>> {
  const response = await request.get<ApiResponse<ProductListItem[]>>(
    `/api/products/${id}/related`,
    { params: { limit } },
  )
  return response.data
}

/**
 * 搜尋建議：傳入關鍵字，回傳最多 limit 筆商品名稱供 autocomplete 使用。
 * NOTE: 後端尚未實作 GET /api/products/suggest?keyword=&limit= 專用端點，
 *       目前暫時借用商品列表 API 取前幾筆名稱。
 *       建議後端新增：GET /api/products/suggest?keyword=&limit=10 → string[]
 */
export async function getProductSuggestions(keyword: string, limit = 8): Promise<string[]> {
  if (!keyword.trim()) return []
  console.log('[TODO] 建議後端實作 GET /api/products/suggest?keyword=&limit=10，目前使用列表 API 替代')
  try {
    const res = await fetchProductList({ keyword, pageSize: limit, page: 1 })
    if (res.success) {
      return res.data.items.map((p) => p.name)
    }
  } catch {
    // 靜默失敗：autocomplete 建議失敗不影響主流程
  }
  return []
}

/**
 * 取得賣家商品列表（僅回傳當前登入賣家的商品）
 * GET /api/seller/products
 * 自動帶入 JWT token，後端從 token 解析賣家 ID
 * 
 * 後端直接回傳分頁資料，不包在 ApiResponse 裡
 */
export async function fetchSellerProducts(
  params: FetchProductsParams = {},
): Promise<SellerProductListResponse> {
  const response = await request.get<SellerProductListResponse>('/api/seller/products', {
    params,
  })
  return response.data
}

/**
 * 新增賣家商品
 * POST /api/seller/products
 * 不需要前端傳 StoreId，後端從 JWT token 解析
 */
export async function createSellerProduct(data: unknown): Promise<ApiResponse<unknown>> {
  const response = await request.post<ApiResponse<unknown>>('/api/seller/products', data)
  return response.data
}
