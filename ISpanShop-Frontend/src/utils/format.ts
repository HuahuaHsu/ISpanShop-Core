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
