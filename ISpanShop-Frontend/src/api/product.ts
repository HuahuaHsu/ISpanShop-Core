import request from './request'
import type {
  ApiResponse,
  FetchProductsParams,
  ProductListResponse,
  ProductDetail,
  ProductListItem,
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
