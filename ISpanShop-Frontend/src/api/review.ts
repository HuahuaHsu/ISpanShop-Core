import axios from './axios'

export interface SubmitReviewParams {
  orderId: number
  rating: number
  comment: string
  imageUrls?: string[]
}

/**
 * 取得特定商品的評論列表 (與後端 OrdersController 路由對接)
 */
export async function fetchProductReviews(productId: number): Promise<any[]> {
  const response = await axios.get<any[]>(`/api/front/orders/product/${productId}`)
  return response.data
}
