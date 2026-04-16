<script setup lang="ts">
import { useRouter } from "vue-router";
import { useAuthStore } from "../../stores/auth";

// ── Icons ──────────────────────────────────────────
const IconGear = () => (
  `<svg width="17" height="17" fill="none" stroke="currentColor" stroke-width="1.8" viewBox="0 0 24 24">
    <circle cx="12" cy="12" r="3" />
    <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z" />
  </svg>`
);
const IconCart = () => (
  `<svg width="17" height="17" fill="none" stroke="currentColor" stroke-width="1.8" viewBox="0 0 24 24">
    <circle cx="9" cy="21" r="1" /><circle cx="20" cy="21" r="1" />
    <path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6" />
  </svg>`
);
const IconChat = () => (
  `<svg width="17" height="17" fill="none" stroke="currentColor" stroke-width="1.8" viewBox="0 0 24 24">
    <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
  </svg>`
);

// ── State ──────────────────────────────────────────
const authStore = useAuthStore();
const router = useRouter();

// ── Lifecycle ──────────────────────────────────────
import { onMounted, ref } from "vue";
import { checkoutApi } from "@/api/checkout";

const liveBalance = ref<number | null>(null);

onMounted(async () => {
  try {
    const res = await checkoutApi.getWalletBalance();
    console.log('Member Center Wallet Sync:', res.data);
    liveBalance.value = res.data.pointBalance ?? res.data.balance ?? 0;
    // 同步更新 store 中的資料並持久化
    authStore.updatePoints(liveBalance.value);
  } catch (err) {
    console.error('Failed to sync wallet balance', err);
  }
});

const go = (name: string) => {
  switch (name) {
    case '設定':
      router.push('/member/settings');
      break;
    case '所有訂單':
    case '待付款':
    case '待出貨':
    case '待收貨':
    case '評價':
      router.push('/member/orders');
      break;
    case '紅利點數':
      router.push('/member/wallet');
      break;
    case '優惠券':
      router.push('/member/coupons');
      break;
    case '我的賣場':
      router.push('/member/mystore');
      break;
    case '購物車':
      router.push('/cart');
      break;
    default:
      router.push({ name: 'wip', query: { title: name } });
  }
};

// ── Mock Data ──────────────────────────────────────
const orders = [
  { label: "待付款", icon: "💳", badge: 2 },
  { label: "待出貨", icon: "📦", badge: 0 },
  { label: "待收貨", icon: "🚚", badge: 1 },
  { label: "評價",   icon: "⭐", badge: 0 },
];

const services = [
  { label: "我的賣場", icon: "🏪", bg: "#FFF0EB" },
  { label: "再買一次", icon: "🔁", bg: "#EDF6FF" },
  { label: "會員權益", icon: "👑", bg: "#FFF8E8" },
  { label: "客服專區", icon: "🎧", bg: "#EDFAF4" },
];
</script>

<template>
  <div class="page">
    <!-- 原本的 Header 已經整合到 MemberLayout 中 -->

    <!-- 購買清單 -->
    <div class="card">
      <div class="card-header">
        <span class="card-label">購買清單</span>
        <span class="see-all" @click="go('所有訂單')">查看全部 ›</span>
      </div>
      <div class="order-grid">
        <div
          v-for="item in orders"
          :key="item.label"
          class="order-item"
          @click="go(item.label)"
        >
          <div class="order-icon-wrap">
            <span class="order-icon">{{ item.icon }}</span>
            <span v-if="item.badge > 0" class="badge-count">{{ item.badge }}</span>
          </div>
          <span class="order-label">{{ item.label }}</span>
        </div>
      </div>
    </div>

    <!-- 我的錢包 -->
    <div class="card">
      <div class="card-header">
        <span class="card-label">我的錢包</span>
      </div>
      <div class="wallet-grid">
        <div class="wallet-item" @click="go('紅利點數')">
          <span class="wallet-icon">🪙</span>
          <span class="wallet-value">{{ (liveBalance ?? authStore.memberInfo?.pointBalance ?? 0).toLocaleString() }}</span>
          <span class="wallet-label">我的蝦幣</span>
        </div>
        <div class="wallet-item" @click="go('優惠券')">
          <span class="wallet-icon">🎟</span>
          <span class="wallet-value">99+ 張</span>
          <span class="wallet-label">優惠券</span>
        </div>
      </div>
    </div>

    <!-- 更多服務 -->
    <div class="card">
      <div class="card-header">
        <span class="card-label">更多服務</span>
      </div>
      <div class="services-grid">
        <div
          v-for="item in services"
          :key="item.label"
          class="service-item"
          @click="go(item.label)"
        >
          <div class="service-circle" :style="{ background: item.bg }">{{ item.icon }}</div>
          <span class="service-label">{{ item.label }}</span>
        </div>
      </div>
    </div>

    <div style="height: 32px"></div>
  </div>
</template>

<style scoped>
/* ── Variables ───────────────────────────────────── */
:root {
  --brand-color: #EE4D2D;
}

.page {
  min-height: 100vh;
  background: #F5F5F5;
  font-family: 'Noto Sans TC', sans-serif;
  max-width: 1200px;
  margin: 0 auto;
  position: relative;
  overflow-x: hidden;
}

@media (max-width: 1200px) {
  .page {
    max-width: 100%;
  }
}

/* ── Header ────────────────────────────────────── */
.header {
  background: #EE4D2D; /* BRAND */
  padding: 14px 16px 18px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.header-left {
  display: flex;
  align-items: center;
  gap: 10px;
}
.avatar {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: rgba(255,255,255,0.25);
  border: 2px solid rgba(255,255,255,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 600;
  color: #fff;
  flex-shrink: 0;
}
.user-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.username {
  font-size: 15px;
  font-weight: 600;
  color: #fff;
  line-height: 1;
}
.level-badge {
  display: inline-flex;
  align-items: center;
  gap: 3px;
  background: rgba(255,255,255,0.2);
  border: 1px solid rgba(255,255,255,0.35);
  border-radius: 20px;
  padding: 2px 9px;
  font-size: 11px;
  color: #fff;
  cursor: pointer;
  width: fit-content;
}
.header-right {
  display: flex;
  align-items: center;
  gap: 6px;
}
.icon-btn {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: rgba(255,255,255,0.15);
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  position: relative;
  flex-shrink: 0;
}
.cart-dot {
  position: absolute;
  top: 5px;
  right: 5px;
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #FFD700;
  border: 1.5px solid #EE4D2D;
}

/* ── Section Card ───────────────────────────────── */
.card {
  background: #fff;
  margin: 8px 12px;
  border-radius: 12px;
  overflow: hidden;
  border: 0.5px solid #E8E8E8;
}
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 14px 6px;
}
.card-label {
  font-size: 13px;
  font-weight: 600;
  color: #333;
}
.see-all {
  font-size: 11px;
  color: #999;
  cursor: pointer;
}

/* ── Order Grid ──────────────────────────────────── */
.order-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  padding: 4px 0 14px;
}
@media (min-width: 768px) {
  .order-grid {
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  }
}
.order-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 8px 4px;
  cursor: pointer;
  border-radius: 8px;
}
.order-icon-wrap {
  width: 40px;
  height: 40px;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}
.order-icon {
  font-size: 24px;
}
.badge-count {
  position: absolute;
  top: -4px;
  right: -4px;
  min-width: 16px;
  height: 16px;
  border-radius: 8px;
  background: #EE4D2D;
  color: #fff;
  font-size: 10px;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 3px;
}
.order-label {
  font-size: 11px;
  color: #666;
  text-align: center;
}

/* ── Wallet Grid ─────────────────────────────────── */
.wallet-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  padding: 4px 10px 14px;
  gap: 8px;
}
@media (min-width: 768px) {
  .wallet-grid {
    grid-template-columns: repeat(4, 1fr);
  }
}
.wallet-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
  padding: 12px 8px;
  cursor: pointer;
  background: #FFFBF0;
  border-radius: 10px;
  border: 0.5px solid #FFE0A0;
}
.wallet-icon {
  font-size: 22px;
}
.wallet-value {
  font-size: 20px;
  font-weight: 700;
  color: #EE4D2D;
}
.wallet-label {
  font-size: 11px;
  color: #A07820;
}

/* ── Services Grid ───────────────────────────────── */
.services-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  padding: 4px 0 14px;
}
@media (min-width: 768px) {
  .services-grid {
    grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  }
}
.service-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 10px 4px;
  cursor: pointer;
  border-radius: 8px;
}
.service-circle {
  width: 46px;
  height: 46px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
}
.service-label {
  font-size: 11px;
  color: #666;
  text-align: center;
}

/* ── Transitions ─────────────────────────────────── */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateX(100%);
}
</style>