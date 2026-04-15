/** 對應後端 GET /api/brands 回傳的單一品牌 */
export interface Brand {
  id: number
  name: string
  productCount: number
}

export interface ApiBrandsResponse {
  success: boolean
  data: Brand[]
  message: string
}
