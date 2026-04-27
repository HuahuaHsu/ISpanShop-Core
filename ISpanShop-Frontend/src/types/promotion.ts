/** 對應後端 GET /api/promotions/active 回傳的單一活動 */
export interface Promotion {
  id: number
  title: string
  subtitle: string | null
  type: string
  typeLabel: string
  bannerImageUrl: string | null
  productImages: string[]
  linkUrl: string
  startDate: string
  endDate: string
}

export interface ApiPromotionsResponse {
  success: boolean
  data: Promotion[]
  message: string
}
