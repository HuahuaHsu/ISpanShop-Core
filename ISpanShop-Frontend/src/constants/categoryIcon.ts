const CATEGORY_ICON_MAP: Record<string, string> = {
  '3C與電子':   '💻',
  '女裝與配件': '👗',
  '男裝與配件': '👔',
  '居家與生活': '🏠',
  '美妝與保養': '💄',
  '美食與生鮮': '🍎',
  '運動與休閒': '⚽',
}

export function getCategoryIcon(name: string): string {
  return CATEGORY_ICON_MAP[name] ?? '📦'
}
