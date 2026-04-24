<template>
  <div class="layout">
    <!-- 帳號停權全域彈窗 (外掛植入) -->
    <el-dialog
      v-model="showBlacklistDialog"
      title="帳號狀態異常"
      width="420px"
      :show-close="false"
      :close-on-click-modal="false"
      :close-on-press-escape="false"
      align-center
      class="blacklist-dialog"
    >
      <div class="blacklist-content">
        <el-icon color="#f56c6c" size="48"><CircleCloseFilled /></el-icon>
        <h3>您的帳號已被停權</h3>
        <p>經系統偵測，您的帳號因違反平台規範已暫時停權。目前無法進行購物、結帳或使用賣家功能。</p>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button type="primary" @click="handleGoMemberCenter">查看我的帳戶</el-button>
          <el-button @click="handleLogout">登出</el-button>
        </div>
      </template>
    </el-dialog>

    <div class="top-bar">
      <div class="top-bar-inner">
        <div class="top-left">
          <a href="#" @click.prevent="handleSellerCenterClick">賣家中心</a>
          <span class="divider">|</span>
          <span class="welcome">🎉 全站滿千免運中</span>
        </div>
        <div class="top-right">
          <template v-if="!authStore.isLoggedIn">
            <a href="#" @click.prevent="authStore.openLoginDialog()">會員中心</a>
            <span class="divider">|</span>
            <a href="#" @click.prevent="router.push('/register')">註冊</a>
            <a href="#" @click.prevent="authStore.openLoginDialog()">登入</a>
          </template>

          <template v-else>
            <el-dropdown trigger="hover" @command="handleDropdownCommand">
              <span class="user-dropdown-trigger">
                <el-icon><User /></el-icon>
                {{ authStore.memberInfo.account }}
                <el-icon class="dropdown-arrow"><ArrowDown /></el-icon>
              </span>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="member">我的帳戶</el-dropdown-item>
                  <el-dropdown-item command="orders">購買清單</el-dropdown-item>
                  <el-dropdown-item command="logout" divided>登出</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </template>
        </div>
      </div>
    </div>

    <header class="main-header">
      <div class="main-header-inner">
        <div class="logo" @click="$router.push('/')">
          <img src="@/assets/images/howbuyLogo.png" class="logo-icon" alt="HowBuy Logo">
          <span class="logo-text">HowBuy</span>
        </div>

        <div class="search-box">
          <div class="search-bar-container">
            <el-autocomplete
              v-model="searchText"
              :fetch-suggestions="fetchSuggestions"
              :debounce="300"
              :trigger-on-focus="false"
              placeholder="搜尋商品、品牌或關鍵字..."
              class="seamless-input"
              size="large"
              @select="handleAutoSelect"
              @keyup.enter="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-autocomplete>

            <button class="seamless-btn" @click="handleSearch">搜尋</button>
          </div>

          <div class="hot-keywords">
            <span class="hot-label">🔥 熱搜:</span>
            <a 
              v-for="keyword in hotKeywords" 
              :key="keyword" 
              href="#" 
              @click.prevent="handleHotKeywordClick(keyword)"
            >{{ keyword }}</a>
          </div>
        </div>

        <div class="header-actions">
          <el-popover
            placement="bottom-end"
            :width="400"
            trigger="hover"
            popper-class="cart-popover"
            :disabled="cartStore.totalCount === 0 || authStore.isBlacklisted"
          >
            <template #reference>
              <div class="action-icon cart" @click="handleActionClick('/cart')">
                <el-badge :value="cartStore.totalCount" :hidden="cartStore.totalCount === 0 || authStore.isBlacklisted" :max="99">
                  <el-icon :size="24"><ShoppingCart /></el-icon>
                </el-badge>
                <div class="action-label">購物車</div>
              </div>
            </template>
            <!-- 購物車預覽內容 -->
            <div class="cart-preview">
              <div class="cart-preview-header">最近加入的商品</div>
              <div class="cart-preview-list">
                <div v-for="item in cartStore.items.slice(0, 5)" :key="item.productId + '-' + item.variantId" class="cart-preview-item">
                  <img :src="item.image" class="preview-img" alt="商品圖片" />
                  <div class="preview-info">
                    <div class="preview-name">{{ item.name }}</div>
                  </div>
                  <div class="preview-price">${{ item.price.toLocaleString() }}</div>
                </div>
              </div>
              <div class="cart-preview-footer">
                <span class="more-items" v-if="cartStore.items.length > 5">
                  {{ cartStore.items.length - 5 }} 件商品尚未展示
                </span>
                <span v-else></span>
                <button class="view-cart-btn" @click="$router.push('/cart')">
                  查看我的購物車
                </button>
              </div>
            </div>
          </el-popover>
        </div>
      </div>
    </header>

    <main class="main-content">
      <router-view />
    </main>

    <footer class="footer">
      <div class="footer-top">
        <div class="footer-features">
          <div class="feature-item">
            <el-icon :size="36"><Van /></el-icon>
            <div><h4>全台快速配送</h4><p>滿千免運費</p></div>
          </div>
          <div class="feature-item">
            <el-icon :size="36"><Lock /></el-icon>
            <div><h4>安全付款保障</h4><p>SSL 加密交易</p></div>
          </div>
          <div class="feature-item">
            <el-icon :size="36"><RefreshRight /></el-icon>
            <div><h4>七天鑑賞期</h4><p>無條件退貨</p></div>
          </div>
          <div class="feature-item">
            <el-icon :size="36"><Service /></el-icon>
            <div><h4>專業客服</h4><p>24小時線上服務</p></div>
          </div>
        </div>
      </div>

      <div class="footer-inner">
        <div class="footer-col footer-brand">
          <div class="footer-logo">
            <img src="@/assets/images/howbuyLogo.png" class="footer-logo-img" alt="HowBuy Logo">
            HowBuy
          </div>
          <p>讓購物變得更簡單、更美好</p>
        </div>
        <div class="footer-col">
          <h4>客戶服務</h4>
          <a href="#">幫助中心</a><a href="#">如何購買</a><a href="#">付款方式</a><a href="#">退換貨服務</a>
        </div>
        <div class="footer-col">
          <h4>關於 HowBuy</h4>
          <a href="#">關於我們</a><a href="#">加入我們</a><a href="#">隱私權政策</a><a href="#">服務條款</a>
        </div>
        <div class="footer-col">
          <h4>聯絡我們</h4>
          <p>📧 service@howbuy.com</p>
          <p>📞 0800-123-456</p>
          <p>🕐 週一至週五 9:00-18:00</p>
        </div>
      </div>
      <div class="footer-bottom">
        © 2026 HowBuy. All rights reserved. | Made with 🧡 in Taiwan
      </div>
    </footer>

    <!-- 聊聊懸浮按鈕與視窗 (停權時隱藏) -->
    <ChatFloat v-if="!authStore.isBlacklisted" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import ChatFloat from '../components/chat/ChatFloat.vue'
import {
  Search, ShoppingCart, Promotion, Van, Lock, RefreshRight, Service, 
  ChatDotRound, Share, User, ArrowDown, CircleCloseFilled
} from '@element-plus/icons-vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useAuthStore } from '../stores/auth'
import { useCartStore } from '../stores/cart'
import { getProductSuggestions, fetchProductList } from '../api/product'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const cartStore = useCartStore()
const searchText = ref('')
const hotKeywords = ref<string[]>(['iPhone 16', '無線耳機', '機械鍵盤', '運動鞋'])

// ── 停權控制邏輯 ──────────────────────────────────
const showBlacklistDialog = computed(() => {
  const isProtectedPath = route.path.startsWith('/member') || 
                          route.path.startsWith('/member/support') ||
                          route.path.startsWith('/member/orders');
  return authStore.isLoggedIn && authStore.isBlacklisted && !isProtectedPath;
});

function handleGoMemberCenter() { router.push('/member'); }
function handleLogout() { authStore.logout(); router.push('/'); }

/** 導向搜尋結果頁 */
function handleSearch(): void {
  const kw = searchText.value.trim()
  void router.push(kw ? { path: '/products', query: { keyword: kw } } : { path: '/products' })
}

/** el-autocomplete 點選建議項目 */
function handleAutoSelect(item: { value: string }): void {
  searchText.value = item.value
  void router.push({ path: '/products', query: { keyword: item.value } })
}

/** 點擊熱搜關鍵字 */
function handleHotKeywordClick(keyword: string): void {
  searchText.value = keyword
  void router.push({ path: '/products', query: { keyword } })
}

/** fetch-suggestions callback */
function fetchSuggestions(queryString: string, cb: (results: { value: string }[]) => void): void {
  if (!queryString.trim()) { cb([]); return }
  getProductSuggestions(queryString)
    .then((names) => cb(names.map((n) => ({ value: n }))))
    .catch(() => cb([]))
}

function handleDropdownCommand(command: string) {
  if (command === 'member') router.push('/member')
  else if (command === 'orders') router.push('/member/orders')
  else if (command === 'logout') handleLogout()
}

function handleSellerCenterClick() {
  if (!authStore.isLoggedIn) { authStore.openLoginDialog(); return; }
  if (authStore.isBlacklisted) { ElMessage.error('您的帳號已被停權，無法使用賣家中心'); return; }
  if (authStore.memberInfo.isSeller === true) router.push('/seller')
  else router.push('/member/mystore')
}

function handleActionClick(path: string) {
  if (!authStore.isLoggedIn) { authStore.openLoginDialog(); return; }
  if (authStore.isBlacklisted && path === '/cart') { ElMessage.error('停權帳號無法使用購物車'); return; }
  router.push(path)
}

async function loadHotKeywords(): Promise<void> {
  try {
    const res = await fetchProductList({ pageSize: 50, page: 1 })
    if (res.success && res.data.items.length > 0) {
      const sorted = [...res.data.items].sort((a, b) => b.soldCount - a.soldCount)
      hotKeywords.value = sorted.slice(0, 5).map(p => p.name.length > 6 ? p.name.substring(0, 6) : p.name)
    }
  } catch { console.warn('熱搜關鍵字載入失敗') }
}

onMounted(() => {
  void loadHotKeywords()
  if (authStore.isLoggedIn) { authStore.fetchUserInfo(); }
})
</script>

<style scoped>
.layout { min-height: 100vh; display: flex; flex-direction: column; }

/* 停權彈窗樣式 */
.blacklist-content { display: flex; flex-direction: column; align-items: center; text-align: center; padding: 10px 0; }
.blacklist-content h3 { margin: 16px 0 8px; color: #303133; }
.blacklist-content p { color: #606266; line-height: 1.6; }
.dialog-footer { display: flex; justify-content: center; gap: 12px; }

/* 頂部公告列 */
.top-bar { background: #0f172a; color: #cbd5e1; font-size: 13px; padding: 8px 0; border-bottom: 1px solid rgba(255,255,255,0.05); }
.top-bar-inner { max-width: 1400px; margin: 0 auto; display: flex; justify-content: space-between; padding: 0 30px; }
.top-bar a { color: #cbd5e1; text-decoration: none; margin: 0 10px; transition: color 0.2s; }
.top-bar a:hover { color: #EE4D2D; }
.divider { opacity: 0.3; margin: 0 5px; }
.welcome { color: #fbbf24; margin-left: 10px; }
.user-dropdown-trigger { display: inline-flex; align-items: center; gap: 5px; color: #cbd5e1; cursor: pointer; transition: color 0.2s; margin: 0 10px; outline: none; }
.user-dropdown-trigger:hover { color: #EE4D2D; }

/* 主 Header — 漸層背景 (還原) */
.main-header {
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 50%, #1e1b4b 100%);
  padding: 24px 0 20px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.15);
}
.main-header-inner { max-width: 1400px; margin: 0 auto; display: flex; align-items: center; gap: 40px; padding: 0 30px; }

.logo { display: flex; align-items: center; gap: 8px; cursor: pointer; transition: transform 0.3s; }
.logo:hover { transform: scale(1.05); }
.logo-icon { height: 70px; width: auto; object-fit: contain; }
.logo-text {
  font-size: 32px; font-weight: 800; letter-spacing: 1px;
  background: linear-gradient(135deg, #EE4D2D 0%, #F3826C 50%, #F7A696 100%);
  -webkit-background-clip: text; background-clip: text; -webkit-text-fill-color: transparent;
}

.search-box { flex: 1; }
.search-bar-container {
  display: flex; width: 100%; height: 44px; border: 2px solid #EE4D2D;
  border-radius: 4px; background: white; overflow: hidden;
}
.seamless-input { flex: 1; }
.seamless-input :deep(.el-input__wrapper) { box-shadow: none !important; background: transparent; padding-left: 16px; }
.seamless-btn {
  background: #EE4D2D; color: white; border: none; padding: 0 40px;
  font-size: 16px; font-weight: 600; cursor: pointer; white-space: nowrap;
}
.seamless-btn:hover { background: #BE3E24; }
.hot-keywords { margin-top: 10px; font-size: 12px; display: flex; align-items: center; gap: 4px; }
.hot-label { color: #fbbf24; margin-right: 6px; }
.hot-keywords a { color: #cbd5e1; text-decoration: none; margin-right: 12px; }
.hot-keywords a:hover { color: #EE4D2D; }

.header-actions { display: flex; gap: 30px; color: white; }
.action-icon { cursor: pointer; text-align: center; transition: transform 0.2s; }
.action-icon:hover { transform: translateY(-3px); color: #EE4D2D; }
.action-label { font-size: 12px; margin-top: 4px; }

.main-content { flex: 1; background: linear-gradient(180deg, #f1f5f9 0%, #e2e8f0 100%); }

.footer { background: #0f172a; color: #cbd5e1; }
.footer-top { background: #1e293b; padding: 30px 0; border-top: 4px solid #EE4D2D; }
.footer-features { max-width: 1400px; margin: 0 auto; padding: 0 30px; display: grid; grid-template-columns: repeat(4, 1fr); gap: 30px; }
.feature-item { display: flex; align-items: center; gap: 16px; }
.feature-item :deep(.el-icon) { color: #EE4D2D; }
.feature-item h4 { color: white; font-size: 15px; margin: 0; }

.footer-inner { max-width: 1400px; margin: 0 auto; display: grid; grid-template-columns: 1.5fr 1fr 1fr 1.2fr; gap: 50px; padding: 50px 30px 30px; }
.footer-brand .footer-logo { display: flex; align-items: center; gap: 8px; font-size: 26px; font-weight: 800; color: #EE4D2D; margin-bottom: 12px; }
.footer-logo-img { height: 30px; }
.footer-col h4 { color: white; font-size: 15px; margin-bottom: 18px; position: relative; padding-bottom: 10px; }
.footer-col h4::after { content: ''; position: absolute; bottom: 0; left: 0; width: 30px; height: 2px; background: #EE4D2D; }
.footer-col a { display: block; color: #94a3b8; text-decoration: none; font-size: 13px; margin-bottom: 10px; }
.footer-bottom { text-align: center; padding: 20px; background: #020617; color: #64748b; font-size: 13px; }

:global(.cart-popover) { padding: 0 !important; border-radius: 4px; }
.cart-preview { display: flex; flex-direction: column; }
.cart-preview-header { padding: 12px 16px; color: #94a3b8; font-size: 14px; }
.cart-preview-list { max-height: 400px; overflow-y: auto; }
.cart-preview-item { display: flex; align-items: center; padding: 12px 16px; gap: 12px; }
.preview-img { width: 48px; height: 48px; object-fit: cover; border-radius: 2px; }
.preview-price { color: #EE4D2D; font-weight: 500; }
.cart-preview-footer { display: flex; justify-content: space-between; align-items: center; padding: 12px 16px; background: #f8fafc; border-top: 1px solid #f1f5f9; }
.view-cart-btn { background: #EE4D2D; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer; }
</style>
