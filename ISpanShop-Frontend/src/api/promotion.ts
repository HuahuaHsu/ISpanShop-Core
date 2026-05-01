import request from './request'
import type { ApiPromotionsResponse, Promotion } from '@/types/promotion'

export async function fetchActivePromotions(limit = 10): Promise<ApiPromotionsResponse> {
  const response = await request.get<ApiPromotionsResponse>('/api/promotions/active', { params: { limit } })
  return response.data
}

export async function fetchPromotionById(id: number): Promise<{ success: boolean; data: Promotion }> {
  const response = await request.get(`/api/promotions/${id}`)
  return response.data as { success: boolean; data: Promotion }
}

export interface PromotionProductItem {
  productId: number
  productName: string
  imageUrl: string | null
  originalPrice: number
  discountPrice: number | null
  discountPercent: number | null
  soldCount: number
  quantityLimit: number | null
  stockLimit: number | null
}

export interface PromotionProductsResult {
  items: PromotionProductItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export async function fetchPublicPromotionProducts(
  id: number,
  params: { page?: number; pageSize?: number; sortBy?: string; priceOrder?: string } = {},
): Promise<{ success: boolean; data: PromotionProductsResult }> {
  const response = await request.get(`/api/promotions/${id}/products`, { params })
  return response.data as { success: boolean; data: PromotionProductsResult }
}

// ─── 賣家促銷活動管理 ────────────────────────────────────────────

/** 取得賣家自己的活動列表 */
export async function fetchSellerPromotions(params: { status?: string; page?: number; pageSize?: number } = {}) {
  const response = await request.get('/api/seller/promotions', { params })
  return response.data
}

/** 新增活動（送審） */
export async function createSellerPromotion(data: unknown) {
  const response = await request.post('/api/seller/promotions', data)
  return response.data
}

/** 編輯活動 */
export async function updateSellerPromotion(id: number, data: unknown) {
  const response = await request.put(`/api/seller/promotions/${id}`, data)
  return response.data
}

/** 刪除活動 */
export async function deleteSellerPromotion(id: number) {
  const response = await request.delete(`/api/seller/promotions/${id}`)
  return response.data
}

/** 撤銷送審（待審核活動 → 刪除） */
export async function cancelSellerPromotion(id: number) {
  const response = await request.delete(`/api/seller/promotions/${id}/cancel-review`)
  return response.data
}

/** 提早結束進行中的活動 */
export async function endSellerPromotionEarly(id: number) {
  const response = await request.put(`/api/seller/promotions/${id}/end-early`)
  return response.data
}

// ─── 活動商品綁定 ────────────────────────────────────────────────

/** 取得活動已綁定商品列表 */
export async function fetchPromotionProducts(promotionId: number) {
  const response = await request.get(`/api/seller/promotions/${promotionId}/products`)
  return response.data
}

/** 批次綁定商品到活動（body: { productIds: number[] }） */
export async function bindPromotionProducts(promotionId: number, productIds: number[]) {
  const response = await request.post(`/api/seller/promotions/${promotionId}/products`, { productIds })
  return response.data
}

/** 從活動移除單一商品 */
export async function unbindPromotionProduct(promotionId: number, productId: number) {
  const response = await request.delete(`/api/seller/promotions/${promotionId}/products/${productId}`)
  return response.data
}

/** 取得可加入此活動的商品（已排除已綁定，需先有 promotionId） */
export async function fetchAvailableProductsForPromotion(
  promotionId: number,
  params: { keyword?: string; page?: number; pageSize?: number } = {},
) {
  const response = await request.get(
    `/api/seller/promotions/${promotionId}/available-products`,
    { params },
  )
  return response.data
}

/** 取得賣家自己的商品列表（用於新增活動時的商品選擇器）
 *  回傳格式：{ items, totalCount, page, pageSize, totalPages }（無 success 包裹）
 */
export async function fetchSellerProductsSimple(params: {
  keyword?: string
  page?: number
  pageSize?: number
}) {
  const response = await request.get('/api/seller/products', { params })
  return response.data as {
    items: Array<{
      id: number
      name: string
      mainImageUrl: string | null
      minPrice: number | null
      maxPrice: number | null
      totalStock: number | null
      status: number
    }>
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
  }
}
