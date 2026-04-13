<template>
  <div class="home">
    <!-- 輪播 + 右側兩個 banner -->
    <section class="banner-section">
      <div class="main-carousel">
        <el-carousel height="320px" arrow="always">
          <el-carousel-item v-for="(banner, i) in banners" :key="i">
            <div class="carousel-slide" :style="{ background: banner.bg }">
              <div class="slide-content">
                <div class="slide-tag">{{ banner.tag }}</div>
                <h2>{{ banner.title }}</h2>
                <p>{{ banner.subtitle }}</p>
                <el-button type="primary" round size="large">立即搶購</el-button>
              </div>
              <div class="slide-emoji">{{ banner.emoji }}</div>
            </div>
          </el-carousel-item>
        </el-carousel>
      </div>
      <div class="side-banners">
        <div class="side-banner" v-for="sb in sideBanners" :key="sb.title" :style="{ background: sb.bg }">
          <div class="sb-tag">{{ sb.tag }}</div>
          <h3>{{ sb.title }}</h3>
          <p>{{ sb.desc }}</p>
          <span class="sb-emoji">{{ sb.emoji }}</span>
        </div>
      </div>
    </section>

    <!-- 圓形快捷服務 -->
    <section class="quick-icons">
      <div v-for="item in quickItems" :key="item.label" class="quick-item">
        <div class="quick-circle">{{ item.icon }}</div>
        <div class="quick-label">{{ item.label }}</div>
      </div>
    </section>

    <!-- 商品分類網格 -->
    <section class="category-section">
      <div class="section-title">分類</div>
      <div class="category-grid">
        <div v-for="cat in categories" :key="cat.name" class="category-item">
          <div class="cat-image">{{ cat.icon }}</div>
          <div class="cat-name">{{ cat.name }}</div>
        </div>
      </div>
    </section>

    <!-- 每日新發現 -->
    <section ref="sectionRef" class="products-section">
      <div class="discovery-header">
        <h2 class="discovery-title">每日新發現</h2>
      </div>

      <!-- 骨架屏：載入中 -->
      <div v-if="loading" class="product-grid">
        <el-skeleton
          v-for="n in 20"
          :key="n"
          animated
          class="skeleton-card"
        >
          <template #template>
            <el-skeleton-item variant="image" class="skeleton-image" />
            <div class="skeleton-body">
              <el-skeleton-item variant="p" style="width: 90%" />
              <el-skeleton-item variant="p" style="width: 70%" />
              <el-skeleton-item variant="p" style="width: 50%" />
            </div>
          </template>
        </el-skeleton>
      </div>

      <!-- 商品牆 -->
      <div v-else-if="products.length > 0" class="product-grid">
        <ProductCard
          v-for="product in products"
          :key="product.id"
          :product="product"
        />
      </div>

      <!-- 沒資料 -->
      <el-empty v-else description="目前沒有商品" :image-size="120" />

      <!-- 分頁 -->
      <div v-if="total > 0" class="pagination-wrap">
        <el-pagination
          background
          layout="prev, pager, next, jumper, total"
          :total="total"
          :page-size="pageSize"
          :current-page="currentPage"
          :disabled="loading"
          @current-change="onPageChange"
        />
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import ProductCard from '@/components/product/ProductCard.vue'
import { fetchProductList } from '@/api/product'
import type { ProductListItem } from '@/types/product'

// ── 每日新發現狀態 ────────────────────────────────────────────────
const products = ref<ProductListItem[]>([])
const loading = ref<boolean>(false)
const currentPage = ref<number>(1)
const pageSize = ref<number>(30)
const total = ref<number>(0)
const sectionRef = ref<HTMLElement | null>(null)

async function loadProducts(): Promise<void> {
  loading.value = true
  try {
    const res = await fetchProductList({
      page: currentPage.value,
      pageSize: pageSize.value,
      sortBy: 'latest',
    })
    if (res.success) {
      products.value = res.data.items
      total.value = res.data.total
    } else {
      ElMessage.error(res.message || '載入失敗')
    }
  } catch {
    ElMessage.error('載入失敗，請稍後再試')
  } finally {
    loading.value = false
  }
}

function onPageChange(page: number): void {
  currentPage.value = page
  void loadProducts().then(() => {
    sectionRef.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  })
}

onMounted(() => {
  void loadProducts()
})

// ── 輪播資料 ──────────────────────────────────────────────────────
const banners = [
  { tag: '🎉 會員專屬', title: '購物節送 8 折券', subtitle: '全站 $49 起免運', bg: 'linear-gradient(135deg, #1e293b 0%, #1e1b4b 100%)', emoji: '🚚' },
  { tag: '🔥 限時搶購', title: '3C 家電季', subtitle: '滿萬折千 再送好禮', bg: 'linear-gradient(135deg, #064e3b 0%, #022c22 100%)', emoji: '📱' },
  { tag: '💚 新品上架', title: '春夏新品', subtitle: '時尚穿搭一次擁有', bg: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 100%)', emoji: '👗' },
]

const sideBanners = [
  { tag: '商城', title: '新品喇叭上市', desc: '領券現折 $100', bg: 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)', emoji: '🔊' },
  { tag: '商城', title: '幫你換新機', desc: 'AI 筆電專區', bg: 'linear-gradient(135deg, #1e293b 0%, #334155 100%)', emoji: '💻' },
]

const quickItems = [
  { icon: '🚚', label: '全站大免運' },
  { icon: '📦', label: '免運$99起' },
  { icon: '💰', label: '全額$49免運' },
  { icon: '👑', label: 'VIP 獨享 18%' },
  { icon: '🛍️', label: 'HowBuy 商城' },
  { icon: '✈️', label: '海外直送' },
  { icon: '💳', label: '銀行刷卡優惠' },
  { icon: '💻', label: 'HowBuy 3C' },
  { icon: '🔥', label: '天天超划算' },
  { icon: '🎁', label: '活動合集' },
]

const categories = [
  { name: '女生衣著', icon: '👗' }, { name: '男生衣著', icon: '👔' },
  { name: '運動/健身', icon: '🏀' }, { name: '男女鞋', icon: '👟' },
  { name: '女生配件/黃金', icon: '👜' }, { name: '美妝保養', icon: '💄' },
  { name: '娛樂、收藏', icon: '🎮' }, { name: '寵物', icon: '🐶' },
  { name: '手機平板與周邊', icon: '📱' }, { name: '3C 與筆電', icon: '💻' },
  { name: '書籍及雜誌期刊', icon: '📚' }, { name: '居家生活', icon: '🛋️' },
  { name: '美食、伴手禮', icon: '🍰' }, { name: '汽機車零件', icon: '🚗' },
  { name: '電玩遊戲', icon: '🎮' }, { name: '保健、護理', icon: '💊' },
  { name: '嬰幼童與母親', icon: '🍼' }, { name: '女生包包/精品', icon: '👛' },
  { name: '男生包包與配件', icon: '🎒' }, { name: '戶外/旅行', icon: '🚴' },
]
</script>

<style scoped>
.home {
  max-width: 1400px;
  margin: 0 auto;
  padding: 20px 30px;
}

/* 輪播區 */
.banner-section {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 16px;
  margin-bottom: 24px;
}
.main-carousel {
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
}
.carousel-slide {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 60px;
  color: white;
  position: relative;
}
.slide-content { z-index: 1; }
.slide-tag {
  display: inline-block;
  background: rgba(238,77,45,0.2);
  color: #EE4D2D;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 13px;
  margin-bottom: 16px;
  border: 1px solid rgba(238,77,45,0.3);
}
.carousel-slide h2 {
  font-size: 42px;
  margin: 0 0 10px;
  font-weight: 800;
}
.carousel-slide p {
  font-size: 18px;
  opacity: 0.85;
  margin-bottom: 24px;
}
.slide-emoji {
  font-size: 180px;
  filter: drop-shadow(0 10px 30px rgba(238,77,45,0.3));
}

.side-banners {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.side-banner {
  flex: 1;
  border-radius: 12px;
  padding: 20px 24px;
  color: white;
  position: relative;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  cursor: pointer;
  transition: transform 0.3s;
}
.side-banner:hover { transform: translateY(-3px); }
.sb-tag {
  display: inline-block;
  background: #EE4D2D;
  color: white;
  padding: 3px 10px;
  border-radius: 4px;
  font-size: 11px;
  margin-bottom: 8px;
}
.side-banner h3 { margin: 0 0 6px; font-size: 18px; }
.side-banner p { margin: 0; font-size: 13px; opacity: 0.85; }
.sb-emoji {
  position: absolute;
  right: 16px;
  bottom: 10px;
  font-size: 70px;
  opacity: 0.7;
}

/* 圓形快捷區 */
.quick-icons {
  background: white;
  border-radius: 12px;
  padding: 24px;
  display: grid;
  grid-template-columns: repeat(10, 1fr);
  gap: 12px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.quick-item {
  text-align: center;
  cursor: pointer;
  transition: transform 0.2s;
}
.quick-item:hover { transform: translateY(-4px); }
.quick-circle {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  background: linear-gradient(135deg, #FFF0ED 0%, #FCDBD5 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  margin: 0 auto 8px;
  box-shadow: 0 2px 6px rgba(238,77,45,0.15);
}
.quick-label { font-size: 12px; color: #475569; }

/* 商品分類網格 */
.category-section {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.section-title {
  font-size: 18px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 20px;
  padding-left: 12px;
  border-left: 4px solid #EE4D2D;
}
.category-grid {
  display: grid;
  grid-template-columns: repeat(10, 1fr);
  gap: 12px;
}
.category-item {
  text-align: center;
  padding: 16px 8px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}
.category-item:hover {
  background: #FFF5F3;
  transform: translateY(-3px);
}
.cat-image {
  width: 70px;
  height: 70px;
  border-radius: 50%;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 36px;
  margin: 0 auto 10px;
}
.cat-name { font-size: 13px; color: #334155; }

/* 每日新發現 */
.products-section {
  background: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}

/* 標題：橘紅底線樣式 */
.discovery-header {
  text-align: center;
  margin-bottom: 24px;
  position: relative;
}
.discovery-title {
  display: inline-block;
  font-size: 22px;
  font-weight: 700;
  color: #1e293b;
  margin: 0;
  padding-bottom: 8px;
  border-bottom: 3px solid #EE4D2D;
}
.discovery-header::before,
.discovery-header::after {
  content: '';
  position: absolute;
  top: 50%;
  width: calc(50% - 90px);
  height: 1px;
  background: #e2e8f0;
  transform: translateY(-6px);
}
.discovery-header::before { left: 0; }
.discovery-header::after  { right: 0; }

/* 商品格線（響應式：大 6、中 4、手機 2） */
.product-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 12px;
}
@media (max-width: 1200px) {
  .product-grid { grid-template-columns: repeat(4, 1fr); }
}
@media (max-width: 600px) {
  .product-grid { grid-template-columns: repeat(2, 1fr); }
}

/* 分頁 */
.pagination-wrap {
  display: flex;
  justify-content: center;
  margin-top: 28px;
  padding-top: 20px;
  border-top: 1px solid #f1f5f9;
}

/* 骨架屏卡片 */
.skeleton-card {
  border-radius: 4px;
  overflow: hidden;
  border: 1px solid #f1f5f9;
}
.skeleton-image {
  width: 100%;
  aspect-ratio: 1 / 1;
}
.skeleton-body {
  padding: 10px 12px 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

/* 修正輪播圓點顏色 */
:deep(.el-carousel__indicator--horizontal .el-carousel__button) {
  background: rgba(255,255,255,0.5);
  width: 30px;
  height: 4px;
  border-radius: 2px;
}
:deep(.el-carousel__indicator--horizontal.is-active .el-carousel__button) {
  background: #EE4D2D;
}
:deep(.el-carousel__arrow) {
  background: rgba(0,0,0,0.4);
}
:deep(.el-carousel__arrow:hover) {
  background: #EE4D2D;
}
</style>
