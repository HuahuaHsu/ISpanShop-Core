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
