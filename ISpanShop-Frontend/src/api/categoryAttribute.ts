import request from './request'

export interface AttributeOption {
  id: number
  value: string
}

export interface CategoryAttribute {
  id: number
  name: string
  isRequired: boolean
  isMultiple: boolean
  maxSelect: number | null
  allowCustom?: boolean  // 是否允許賣家自填選項（不在預設清單中的值）
  options: AttributeOption[]
}

export interface CategoryAttributesResponse {
  success: boolean
  data: CategoryAttribute[]
  message: string
}

/**
 * 取得指定分類的屬性列表
 * @param categoryId 分類 ID
 * @returns Promise<CategoryAttributesResponse>
 */
export async function getCategoryAttributes(
  categoryId: number
): Promise<CategoryAttributesResponse> {
  try {
    const response = await request.get<CategoryAttributesResponse>(
      `/api/categories/${categoryId}/attributes`
    )
    
    // TODO: 等後端補上 allowCustom 欄位後移除此預設值
    // 目前後端尚未回傳 allowCustom，手動補上預設值 true
    if (response.data.success && response.data.data) {
      response.data.data.forEach(attr => {
        if (attr.allowCustom === undefined) {
          attr.allowCustom = true
        }
      })
    }
    
    return response.data
  } catch (error) {
    console.error(`取得分類 ${categoryId} 的屬性失敗:`, error)
    // 發生錯誤時返回空陣列，避免畫面崩潰
    return {
      success: false,
      data: [],
      message: '取得分類屬性失敗',
    }
  }
}
