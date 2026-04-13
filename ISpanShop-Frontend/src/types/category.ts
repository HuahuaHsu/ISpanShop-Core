/** 對應後端 GET /api/categories 回傳的單一分類 */
export interface Category {
  id: number
  name: string
  iconUrl: string | null
  sortOrder: number
  productCount: number
}

export interface ApiCategoriesResponse {
  success: boolean
  data: Category[]
  message: string
}
