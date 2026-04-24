import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { useAuthStore } from './auth'
import { 
  getCartApi, 
  addToCartApi, 
  updateCartItemApi, 
  removeCartItemApi, 
  syncCartApi,
  clearCartApi
} from '@/api/cart'

export interface CartItem {
  productId: number
  variantId: number | null
  name: string
  image: string
  price: number
  quantity: number
  selected: boolean
  specLabel: string
  storeId: number
  storeName: string
}

const CART_KEY = 'cart_items'

export const useCartStore = defineStore('cart', () => {
  const authStore = useAuthStore()
  const items = ref<CartItem[]>([])
  const loading = ref(false)

  // 初始化：從 localStorage 載入 (僅作為未登入或初始顯示用)
  const init = () => {
    try {
      const stored = JSON.parse(localStorage.getItem(CART_KEY) ?? '[]') as CartItem[]
      items.value = stored.map(item => ({ ...item, selected: item.selected ?? true }))
    } catch {
      items.value = []
    }
  }

  // ── 核心功能：同步與獲取 ──

  /** 從伺服器獲取購物車並更新本地 */
  async function fetchCart() {
    if (!authStore.isLoggedIn) return
    
    try {
      loading.value = true
      const res = await getCartApi()
      // 將後端 DTO 轉為前端格式
      items.value = res.data.map((item: any) => ({
        productId: item.productId,
        variantId: item.variantId,
        name: item.productName,
        image: item.productImage,
        price: item.unitPrice,
        quantity: item.quantity,
        selected: true, // 預設選中
        specLabel: item.variantName || '',
        storeId: item.storeId,
        storeName: item.storeName
      }))
    } catch (error) {
      console.error('獲取購物車失敗:', error)
    } finally {
      loading.value = false
    }
  }

  /** 登入時同步本地購物車到伺服器 */
  async function syncLocalCart() {
    if (!authStore.isLoggedIn || items.value.length === 0) {
      if (authStore.isLoggedIn) await fetchCart()
      return
    }

    try {
      loading.value = true
      // 將本地 items 轉為後端預期格式
      const localData = items.value.map(i => ({
        productId: i.productId,
        variantId: i.variantId,
        quantity: i.quantity
      }))
      
      const res = await syncCartApi(localData)
      // 同步完後，後端會回傳合併後的完整清單
      items.value = res.data.map((item: any) => ({
        productId: item.productId,
        variantId: item.variantId,
        name: item.productName,
        image: item.productImage,
        price: item.unitPrice,
        quantity: item.quantity,
        selected: true,
        specLabel: item.variantName || '',
        storeId: item.storeId,
        storeName: item.storeName
      }))
      // 同步完後清除 localStorage 的暫存，因為已經進資料庫了
      localStorage.removeItem(CART_KEY)
    } catch (error) {
      console.error('同步購物車失敗:', error)
      await fetchCart() // 失敗則退而求其次，直接抓取伺服器上的
    } finally {
      loading.value = false
    }
  }

  // ── 操作功能 ──

  async function addItem(newItem: Omit<CartItem, 'quantity' | 'selected'> & { quantity?: number, selected?: boolean }) {
    const qty = newItem.quantity ?? 1
    const selected = newItem.selected ?? true

    if (authStore.isLoggedIn) {
      try {
        await addToCartApi({
          productId: newItem.productId,
          variantId: newItem.variantId,
          quantity: qty
        })
        await fetchCart() // 重新獲取最新狀態
      } catch (error) {
        console.error('加入購物車 API 失敗:', error)
      }
    } else {
      const existing = items.value.find(
        (i) => i.productId === newItem.productId && i.variantId === newItem.variantId,
      )
      if (existing) {
        existing.quantity += qty
      } else {
        items.value.push({ ...newItem, quantity: qty, selected })
      }
    }
  }

  async function removeItem(productId: number, variantId: number | null) {
    if (authStore.isLoggedIn) {
      try {
        await removeCartItemApi(productId, variantId)
        await fetchCart()
      } catch (error) {
        console.error('移除購物車品項失敗:', error)
      }
    } else {
      items.value = items.value.filter(
        (i) => !(i.productId === productId && i.variantId === variantId),
      )
    }
  }

  async function updateQty(productId: number, variantId: number | null, qty: number) {
    if (authStore.isLoggedIn) {
      try {
        if (qty <= 0) {
          await removeItem(productId, variantId)
        } else {
          await updateCartItemApi({ productId, variantId, quantity: qty })
          await fetchCart()
        }
      } catch (error) {
        console.error('更新購物車數量失敗:', error)
      }
    } else {
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
  }

  async function clearCart() {
    if (authStore.isLoggedIn) {
      try {
        await clearCartApi()
        items.value = []
      } catch (error) {
        console.error('清空購物車失敗:', error)
      }
    } else {
      items.value = []
    }
  }

  async function clearSelectedItems() {
    if (authStore.isLoggedIn) {
      // 結帳後端已經清理了資料庫中對應的商品，這裡只要重新拉取最新的狀態即可
      await fetchCart()
    } else {
      items.value = items.value.filter(i => !i.selected)
    }
  }

  // ── 計算屬性 ──

  const totalCount = computed(() => items.value.length)
  const totalQuantity = computed(() => items.value.reduce((sum, item) => sum + item.quantity, 0))
  const totalPrice = computed(() => items.value.reduce((sum, item) => sum + item.price * item.quantity, 0))
  const selectedQuantity = computed(() => items.value.filter(i => i.selected).reduce((sum, item) => sum + item.quantity, 0))
  const selectedPrice = computed(() => items.value.filter(i => i.selected).reduce((sum, item) => sum + item.price * item.quantity, 0))
  const selectedItems = computed(() => items.value.filter(i => i.selected))
  const isAllSelected = computed({
    get: () => items.value.length > 0 && items.value.every(i => i.selected),
    set: (val: boolean) => { items.value.forEach(i => i.selected = val) }
  })

  // ── 監聽器與持久化 ──

  // 當登入狀態改變時
  watch(() => authStore.isLoggedIn, (isLoggedIn) => {
    if (isLoggedIn) {
      syncLocalCart()
    } else {
      init() // 登出後切換回 localStorage 內容 (或清空)
    }
  }, { immediate: true })

  // 未登入時的持久化
  watch(items, (val) => {
    if (!authStore.isLoggedIn) {
      localStorage.setItem(CART_KEY, JSON.stringify(val))
    }
  }, { deep: true })

  // 初始載入
  init()

  return { 
    items, 
    loading,
    totalCount, 
    totalQuantity, 
    totalPrice, 
    selectedQuantity,
    selectedPrice,
    selectedItems,
    isAllSelected,
    fetchCart,
    addItem, 
    removeItem, 
    updateQty, 
    clearSelectedItems,
    clearCart
  }
})
