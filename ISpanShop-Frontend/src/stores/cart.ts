import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'

export interface CartItem {
  productId: number
  variantId: number | null
  name: string
  image: string
  price: number
  quantity: number
  selected: boolean
  /** 規格描述，例如 "顏色: 紅、尺寸: M"，無規格時為空字串 */
  specLabel: string
  storeId: number
  storeName: string
  }

  const CART_KEY = 'cart_items'

  export const useCartStore = defineStore('cart', () => {
  const items = ref<CartItem[]>(
    (() => {
      try {
        const stored = JSON.parse(localStorage.getItem(CART_KEY) ?? '[]') as CartItem[]
        // Ensure all items have a selected property
        return stored.map(item => ({ ...item, selected: item.selected ?? true }))
      } catch {
        return []
      }
    })(),
  )

  /** 商品種類數（幾種不同商品/規格），用於 Header 徽章 */
  const totalCount = computed(() => items.value.length)

  /** 所有商品數量加總 */
  const totalQuantity = computed(() =>
    items.value.reduce((sum, item) => sum + item.quantity, 0),
  )

  /** 總計金額（所有商品） */
  const totalPrice = computed(() =>
    items.value.reduce((sum, item) => sum + item.price * item.quantity, 0),
  )

  /** 已勾選商品的數量加總 */
  const selectedQuantity = computed(() =>
    items.value.filter(i => i.selected).reduce((sum, item) => sum + item.quantity, 0),
  )

  /** 已勾選商品的總計金額 */
  const selectedPrice = computed(() =>
    items.value.filter(i => i.selected).reduce((sum, item) => sum + item.price * item.quantity, 0),
  )

  /** 是否全選 */
  const isAllSelected = computed({
    get: () => items.value.length > 0 && items.value.every(i => i.selected),
    set: (val: boolean) => {
      items.value.forEach(i => i.selected = val)
    }
  })

  /** 已勾選的商品清單 */
  const selectedItems = computed(() => items.value.filter(i => i.selected))

  function addItem(newItem: Omit<CartItem, 'quantity' | 'selected'> & { quantity?: number, selected?: boolean }): void {
    const qty = newItem.quantity ?? 1
    const selected = newItem.selected ?? true
    const existing = items.value.find(
      (i) => i.productId === newItem.productId && i.variantId === newItem.variantId,
    )
    if (existing) {
      existing.quantity += qty
    } else {
      items.value.push({ ...newItem, quantity: qty, selected })
    }
  }

  function removeItem(productId: number, variantId: number | null): void {
    items.value = items.value.filter(
      (i) => !(i.productId === productId && i.variantId === variantId),
    )
  }

  function updateQty(productId: number, variantId: number | null, qty: number): void {
    const item = items.value.find(
      (i) => i.productId === productId && i.variantId === variantId,
    )
    if (!item) return
    if (qty <= 0) {
      removeItem(productId, variantId)
    } else {
      item.quantity = qty
    }
  }

  /** 清除已勾選的商品（結帳後呼叫） */
  function clearSelectedItems(): void {
    items.value = items.value.filter(i => !i.selected)
  }

  function clearCart(): void {
    items.value = []
  }

  function setItems(newItems: CartItem[]): void {
    items.value = newItems
  }

  // 持久化到 localStorage
  watch(items, (val) => {
    localStorage.setItem(CART_KEY, JSON.stringify(val))
  }, { deep: true })

  return { 
    items, 
    totalCount, 
    totalQuantity, 
    totalPrice, 
    selectedQuantity,
    selectedPrice,
    selectedItems,
    isAllSelected,
    addItem, 
    removeItem, 
    updateQty, 
    clearSelectedItems,
    clearCart, 
    setItems 
    }
    })