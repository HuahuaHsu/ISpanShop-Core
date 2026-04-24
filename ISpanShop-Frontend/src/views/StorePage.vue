<template>
  <div v-loading="storeLoading" class="store-page">

    <!-- ── 賣場資訊卡片 ── -->
    <div v-if="store" class="store-hero">
      <div class="store-hero-inner">
        <!-- 左：Logo -->
        <div class="store-logo-wrap">
          <el-avatar v-if="store.logoUrl" :src="store.logoUrl" :size="80" class="store-logo" />
          <el-avatar v-else :size="80" class="store-logo store-logo-fallback">
            {{ store.name?.charAt(0) ?? '?' }}
          </el-avatar>
        </div>

        <!-- 中：名稱 + 統計 -->
        <div class="store-meta">
          <h1 class="store-name">{{ store.name }}</h1>
          <div class="store-stats">
            <span>⭐ {{ store.rating != null ? store.rating.toFixed(1) : '—' }}</span>
            <el-divider direction="vertical" />
            <span>商品 {{ store.productCount ?? 0 }} 件</span>
            <el-divider direction="vertical" />
            <span>{{ (store.joinedYearsAgo ?? 0) === 0 ? '新加入' : `加入 ${store.joinedYearsAgo} 年` }}</span>
          </div>
        </div>

        <!-- 右：好聊按鈕 -->
        <div class="store-actions">
          <el-button type="primary" plain @click="handleOpenChat">
            <el-icon style="margin-right: 4px"><ChatDotRound /></el-icon>好聊
          </el-button>
        </div>
      </div>
    </div>

    <!-- ── 商品區 ── -->
    <div class="store-products-section">
      <h2 class="section-title">全部商品</h2>

      <!-- 載入骨架 -->
      <div v-if="productsLoading" class="product-grid">
        <el-skeleton v-for="n in 10" :key="n" animated class="skeleton-card">
          <template #template>
            <el-skeleton-item variant="image" style="width: 100%; padding-top: 100%;" />
            <div style="padding: 10px 12px;">
              <el-skeleton-item variant="p" style="width: 90%; margin-bottom: 8px;" />
              <el-skeleton-item variant="p" style="width: 50%;" />
            </div>
          </template>
        </el-skeleton>
      </div>

      <!-- 商品網格 -->
      <div v-else-if="products.length > 0" class="product-grid">
        <ProductCard v-for="p in products" :key="p.id" :product="p" />
      </div>

      <!-- 空狀態 -->
      <el-empty
        v-else
        description="該賣場目前沒有商品"
        :image-size="120"
        style="padding: 60px 0"
      />

      <!-- 分頁器 -->
      <div v-if="total > pageSize" class="pagination-wrap">
        <el-pagination
          v-model:current-page="currentPage"
          :page-size="pageSize"
          :total="total"
          layout="total, prev, pager, next"
          background
          @current-change="loadProducts"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { ChatDotRound } from '@element-plus/icons-vue'
import { getStoreInfo, getStoreProducts } from '@/api/store'
import ProductCard from '@/components/product/ProductCard.vue'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import type { StoreInfo } from '@/types/product'
import type { ProductListItem } from '@/types/product'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const chatStore = useChatStore()

// ── State ──────────────────────────────────────────────────────────
const storeLoading = ref<boolean>(true)
const productsLoading = ref<boolean>(false)
const store = ref<StoreInfo | null>(null)
const products = ref<ProductListItem[]>([])
const currentPage = ref<number>(1)
const pageSize = 20
const total = ref<number>(0)

// ── Init ───────────────────────────────────────────────────────────
onMounted(async () => {
  const id = Number(route.params['id'])
  if (!id) {
    router.replace('/not-found')
    return
  }
  await Promise.all([loadStore(id), loadProducts()])
})

async function loadStore(id: number): Promise<void> {
  storeLoading.value = true
  try {
    const res = await getStoreInfo(id)
    // 後端可能直接回傳物件，也可能包在 { data } 裡
    store.value = (res.data?.data ?? res.data) as StoreInfo
  } catch {
    ElMessage.error('載入賣場資訊失敗')
  } finally {
    storeLoading.value = false
  }
}

async function loadProducts(page = currentPage.value): Promise<void> {
  const id = Number(route.params['id'])
  productsLoading.value = true
  try {
    const res = await getStoreProducts(id, { page, pageSize })
    const payload = res.data?.data ?? res.data
    products.value = payload?.items ?? []
    total.value = payload?.totalCount ?? 0
    currentPage.value = page
  } catch {
    ElMessage.error('載入商品失敗')
  } finally {
    productsLoading.value = false
  }
}

// ── 好聊 ───────────────────────────────────────────────────────────
function handleOpenChat(): void {
  if (!authStore.isLoggedIn) {
    ElMessage.warning('請先登入後再使用好聊功能')
    router.push('/login')
    return
  }
  if (store.value) {
    chatStore.openChatWithUser(
      (store.value as any).userId ?? 0,
      store.value.name ?? '賣家',
    )
  }
}
</script>

<style scoped>
.store-page {
  max-width: 1280px;
  margin: 0 auto;
  padding: 24px 16px 48px;
}

/* ── 賣場資訊卡片 ── */
.store-hero {
  background: #fff;
  border: 1px solid #e8eaf0;
  border-radius: 8px;
  padding: 28px 32px;
  margin-bottom: 28px;
}

.store-hero-inner {
  display: flex;
  align-items: center;
  gap: 24px;
}

.store-logo-wrap {
  flex-shrink: 0;
}

.store-logo {
  border: 2px solid #f1f5f9;
}

.store-logo-fallback {
  background: #ee4d2d;
  color: #fff;
  font-size: 32px;
  font-weight: 700;
}

.store-meta {
  flex: 1;
  min-width: 0;
}

.store-name {
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 10px;
}

.store-stats {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px;
  font-size: 14px;
  color: #64748b;
}

.store-actions {
  flex-shrink: 0;
}

/* ── 商品區 ── */
.section-title {
  font-size: 18px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 16px;
}

.product-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 12px;
  margin-bottom: 24px;
}

@media (max-width: 1200px) {
  .product-grid { grid-template-columns: repeat(4, 1fr); }
}
@media (max-width: 900px) {
  .product-grid { grid-template-columns: repeat(3, 1fr); }
}
@media (max-width: 600px) {
  .product-grid { grid-template-columns: repeat(2, 1fr); }
  .store-hero-inner { flex-wrap: wrap; }
}

.pagination-wrap {
  display: flex;
  justify-content: center;
  padding: 16px 0;
}
</style>
