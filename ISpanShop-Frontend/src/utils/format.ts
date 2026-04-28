/**
 * 格式化銷量顯示
 * @param count 銷量數字
 * @returns 格式化後的字串（已售出 X / X千+ / X萬+）
 */
export function formatSoldCount(count: number | null | undefined): string {
  const n = Number(count) || 0
  if (n === 0) return '已售出 0'
  if (n < 1000) return `已售出 ${n}`
  if (n < 10000) return `已售出 ${Math.floor(n / 1000)}千+`
  return `已售出 ${Math.floor(n / 10000)}萬+`
}

/**
 * 格式化價格顯示（加千分位）
 * @param price 價格數字
 * @returns 格式化後的字串
 */
export function formatPrice(price: number): string {
  return price.toLocaleString('zh-TW')
}

/**
 * 組合完整圖片 URL
 * @param url 相對或絕對路徑
 * @returns 完整 URL
 */
export function getFullImageUrl(url: string | null | undefined): string {
  if (!url) return ''
  if (url.startsWith('blob:') || url.startsWith('http') || url.startsWith('data:')) return url
  const baseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7125'
  // 確保 url 開頭有 /
  const normalizedUrl = url.startsWith('/') ? url : `/${url}`
  return `${baseUrl}${normalizedUrl}`
}

/**
 * 格式化相對時間顯示
 * @param dateStr ISO 日期字串
 * @returns 相對時間格式（例如：3年前、2個月前、5天前、今天）
 */
export function formatRelativeTime(dateStr: string | null | undefined): string {
  if (!dateStr) return '—'
  try {
    const now = new Date()
    const date = new Date(dateStr)
    const diffMs = now.getTime() - date.getTime()
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))
    if (diffDays < 1) return '今天'
    if (diffDays < 30) return `${diffDays}天前`
    if (diffDays < 365) return `${Math.floor(diffDays / 30)}個月前`
    return `${Math.floor(diffDays / 365)}年前`
  } catch {
    return '—'
  }
}
