<template>
  <div class="seller-layout">
    <!-- 1. 停權提示彈窗 (僅在 isSuspended 為 true 時顯示) -->
    <el-dialog
      v-model="isSuspended"
      title="商店狀態提示"
      width="480px"
      :show-close="false"
      :close-on-click-modal="false"
      :close-on-press-escape="false"
      align-center
      class="suspension-dialog"
    >
    <div class="suspension-content">
        <el-icon color="#f56c6c" size="120"><WarningFilled /></el-icon>
        <h2>您的商店已被停權</h2>
        <p>目前賣場已「停權」，暫時無法使用賣家中心相關功能。您可以查看諮詢紀錄或提交新的諮詢與平台管理員聯繫。</p>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="router.push('/')">回到首頁</el-button>
          <el-button type="primary" @click="router.push('/member/support')">聯繫客服</el-button>
        </div>
      </template>
    </el-dialog>

    <!-- 頂部導覽列 -->
    <header class="seller-header">
      <div class="header-left">
        <div class="seller-logo" @click="router.push('/seller')">
          <img src="@/assets/images/howbuyLogo.png" class="logo-icon" alt="HowBuy Logo">
          <span class="logo-text">HowBuy <span class="logo-sub">賣家中心</span></span>
        </div>
      </div>
      <div class="header-right">
        <el-dropdown trigger="click" @command="handleCommand">
          <span class="account-trigger">
            <el-avatar :size="32" class="account-avatar">
              {{ authStore.memberInfo.account?.charAt(0).toUpperCase() ?? 'S' }}
            </el-avatar>
            <span class="account-name">{{ authStore.memberInfo.account ?? '賣家' }}</span>
            <el-icon class="account-arrow"><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="storefront">
                <el-icon><House /></el-icon> 回到首頁
              </el-dropdown-item>
              <el-dropdown-item command="logout" divided>
                <el-icon><SwitchButton /></el-icon> 登出
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </header>

    <div class="seller-body">
      <!-- 2. 停權網底屏蔽層 (毛玻璃效果) -->
      <div v-if="isSuspended" class="suspension-mask-layer"></div>

      <!-- 左側選單 (完整恢復) -->
      <aside class="seller-sidebar" :class="{ collapsed: isCollapsed, 'is-suspended': isSuspended }">
        <el-menu
          :default-active="activeMenu"
          :collapse="isCollapsed"
          :collapse-transition="true"
          router
          class="sidebar-menu"
        >
          <!-- 首頁 -->
          <el-menu-item index="/seller">
            <el-icon><DataAnalysis /></el-icon>
            <template #title>我的賣場</template>
          </el-menu-item>

          <!-- 賣場管理 -->
          <el-sub-menu index="products">
            <template #title>
              <el-icon><Box /></el-icon>
              <span>賣場管理</span>
            </template>
            <el-menu-item index="/seller/profile">
              <el-icon><Setting /></el-icon>
              <template #title>賣場介紹</template>
            </el-menu-item>
            <el-menu-item index="/seller/products">
              <el-icon><List /></el-icon>
              <template #title>我的商品</template>
            </el-menu-item>
            <el-menu-item index="/seller/products/new">
              <el-icon><Plus /></el-icon>
              <template #title>新增商品</template>
            </el-menu-item>
          </el-sub-menu>

          <!-- 訂單管理 -->
          <el-sub-menu index="orders">
            <template #title>
              <el-icon><Document /></el-icon>
              <span>訂單管理</span>
            </template>
            <el-menu-item index="/seller/orders">
              <el-icon><List /></el-icon>
              <template #title>我的訂單</template>
            </el-menu-item>
            <el-menu-item index="/seller/returns">
              <el-icon><RefreshLeft /></el-icon>
              <template #title>退貨 / 退款</template>
            </el-menu-item>
          </el-sub-menu>

          <!-- 促銷管理 -->
          <el-sub-menu index="promotions">
            <template #title>
              <el-icon><PriceTag /></el-icon>
              <span>促銷管理</span>
            </template>
            <el-menu-item index="/seller/promotions">
              <el-icon><StarFilled /></el-icon>
              <template #title>行銷活動</template>
            </el-menu-item>
            <el-menu-item index="/seller/coupons">
              <el-icon><Ticket /></el-icon>
              <template #title>優惠券</template>
            </el-menu-item>
          </el-sub-menu>

          <!-- 數據中心 -->
          <el-sub-menu index="analytics">
            <template #title>
              <el-icon><TrendCharts /></el-icon>
              <span>數據中心</span>
            </template>
            <el-menu-item index="/seller/analytics/sales">
              <el-icon><Histogram /></el-icon>
              <template #title>銷售報表</template>
            </el-menu-item>
            <el-menu-item index="/seller/analytics/traffic">
              <el-icon><DataLine /></el-icon>
              <template #title>流量分析</template>
            </el-menu-item>
          </el-sub-menu>

          <!-- 聊聊管理 -->
          <el-menu-item index="/seller/chat">
            <el-icon><ChatDotRound /></el-icon>
            <template #title>聊聊管理</template>
          </el-menu-item>
        </el-menu>

        <div class="collapse-btn" @click="toggleCollapse">
          <el-icon>
            <component :is="isCollapsed ? DArrowRight : DArrowLeft" />
          </el-icon>
          <span v-if="!isCollapsed" class="collapse-label">收合選單</span>
        </div>
      </aside>

      <!-- 3. 主內容區 -->
      <main class="seller-main" :style="{ marginLeft: isCollapsed ? '64px' : '220px' }">
        <!-- 如果正在檢查狀態且不是 Suspended，則顯示載入中 (避免正常用戶看到閃爍) -->
        <div v-if="checkingStatus && !isSuspended" v-loading="true" class="loading-placeholder"></div>

        <template v-else>
          <!-- 停權時顯示客服內容作為網底 -->
          <div v-if="isSuspended" class="suspended-bg-content">
            <SupportTicketsView />
          </div>
          <!-- 正常時顯示路由內容 -->
          <router-view v-else />
        </template>
      </main>
    </div>

    <!-- 聊聊懸浮按鈕與視窗 -->
    <ChatFloat />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useSellerStore } from '../stores/seller'
import ChatFloat from '../components/chat/ChatFloat.vue'
import {
  ArrowDown, House, SwitchButton,
  DataAnalysis, Box, List, Plus, Document, RefreshLeft,
  PriceTag, StarFilled, Ticket, TrendCharts, Histogram, DataLine,
  ChatDotRound, DArrowLeft, DArrowRight, Setting, WarningFilled
} from '@element-plus/icons-vue'
import { useAuthStore } from '../stores/auth'
import { getStoreStatusApi } from '../api/store'
import SupportTicketsView from '../views/member/SupportTicketsView.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const sellerStore = useSellerStore()

onMounted(() => {
  void sellerStore.fetchBanStatus()
})

const isCollapsed = ref<boolean>(false)
const isSuspended = ref<boolean>(false)
const checkingStatus = ref<boolean>(true)
const activeMenu = computed<string>(() => route.path)

async function checkStoreStatus() {
  // 如果已經知道是正常狀態且 token 沒變，可以考慮從 store 快取讀取以減少請求
  // 但為了安全，我們在 Layout 層級至少做一次即時檢查
  try {
    const res = await getStoreStatusApi()
    const currentStatus = res.data.status

    if (currentStatus === 'Suspended') {
      isSuspended.value = true
    } else if (currentStatus === 'Approved') {
      isSuspended.value = false
      authStore.updateSellerStatus(true)
    } else {
      // Pending, Rejected 等狀態導回檢查頁
      await router.replace('/member/mystore')
    }
  } catch (error) {
    console.error('[SellerLayout] Status Check Failed:', error)
  } finally {
    checkingStatus.value = false
  }
}

onMounted(() => {
  checkStoreStatus()
})

function toggleCollapse(): void {
  isCollapsed.value = !isCollapsed.value
}

function handleCommand(command: string): void {
  if (command === 'storefront') router.push('/')
  else if (command === 'logout') {
    authStore.logout()
    ElMessage.success('已登出')
    router.push('/login')
  }
}
</script>

<style scoped>
.seller-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: #f5f6fa;
}

/* ── 停權屏蔽遮罩 (僅屏蔽互動) ── */
.suspension-mask-layer {
  position: fixed;
  top: 56px;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.3);
  backdrop-filter: blur(6px);
  z-index: 2000;
  cursor: not-allowed;
}

/* ── 停權背景內容樣式 ── */
.suspended-bg-content {
  opacity: 0.7;
  filter: grayscale(0.3);
  pointer-events: none;
}

.loading-placeholder {
  height: 400px;
  width: 100%;
}

/* ── 頂部導覽列 ── */
.seller-header {
  height: 56px;
  background: #1a1a2e;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 2001;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.seller-logo { display: flex; align-items: center; gap: 8px; cursor: pointer; }
.logo-icon { height: 32px; }
.logo-text { font-size: 18px; font-weight: 700; color: white; }
.logo-sub { font-size: 14px; color: #ee4d2d; margin-left: 6px; }

.account-trigger { display: flex; align-items: center; gap: 8px; cursor: pointer; color: #cbd5e1; outline: none; }
.account-name { font-size: 14px; }

/* ── 主體 ── */
.seller-body {
  display: flex;
  flex: 1;
  margin-top: 56px;
  position: relative;
}

.seller-sidebar {
  width: 220px;
  min-height: calc(100vh - 56px);
  background: white;
  position: fixed;
  left: 0;
  top: 56px;
  bottom: 0;
  border-right: 1px solid #e8eaf0;
  transition: width 0.3s;
  z-index: 900;
}
.seller-sidebar.is-suspended {
  pointer-events: none;
  opacity: 0.6;
}
.seller-sidebar.collapsed { width: 64px; }
.sidebar-menu { border-right: none !important; }

.collapse-btn {
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  cursor: pointer;
  color: #64748b;
  border-top: 1px solid #e8eaf0;
}

.seller-main {
  flex: 1;
  min-height: calc(100vh - 56px);
  padding: 24px;
  transition: margin-left 0.3s;
}

/* 彈窗樣式微調 */
.suspension-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 10px 0;
}
.suspension-content h3 { margin: 16px 0 10px; color: #303133; }
.suspension-content p { color: #606266; line-height: 1.6; }
.dialog-footer { display: flex; justify-content: center; }
</style>
