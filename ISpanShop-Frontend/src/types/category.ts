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

/** 對應後端 GET /api/categories/{id}/children 回傳的子分類 */
export interface SubCategory {
  id: number
  name: string
  sortOrder: number
  productCount: number
}

export interface ApiSubCategoriesResponse {
  success: boolean
  data: SubCategory[]
  message: string
}
