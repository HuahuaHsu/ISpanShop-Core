<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../../stores/auth";
import { checkoutApi } from "@/api/checkout";
import { getMyCoupons } from "@/api/coupon";
import { getMyOrdersApi } from "@/api/order";
import { fetchActivePromotions } from "@/api/promotion";
import type { Promotion } from "@/types/promotion";

// ── State ──────────────────────────────────────────
const authStore = useAuthStore();
const router = useRouter();

const liveBalance = ref<number | null>(null);
const liveCouponCount = ref<number>(0);
const orderCounts = ref({
  pending: 0,
  processing: 0,
  shipped: 0,
  completed: 0
});

// ── 活動 API 資料 ──────────────────────────────────
const promotions = ref<Promotion[]>([]);

// 用於 API 活動的玻璃感邊框與 Tag 配色 (不使用強烈底色)
const promoGlassColors = [
  { border: 'rgba(238, 77, 45, 0.3)', tag: '#EE4D2D' }, // 蝦皮紅
  { border: 'rgba(45, 130, 238, 0.3)', tag: '#2D82EE' }, // 藍
  { border: 'rgba(22, 163, 74, 0.3)', tag: '#16A34A' },  // 綠
  { border: 'rgba(217, 119, 6, 0.3)', tag: '#D97706' },  // 橘
];

// ── 首頁靜態活動資料 (同步 HomeView.vue) ──
const staticBanners = [
  { tag: '🎉 會員專屬', title: '購物節送 8 折券', subtitle: '全站 $49 起免運', bg: 'linear-gradient(135deg, #1e293b 0%, #1e1b4b 100%)', emoji: '🚚' },
  { tag: '🔥 限時搶購', title: '3C 家電季', subtitle: '滿萬折千 再送好禮', bg: 'linear-gradient(135deg, #064e3b 0%, #022c22 100%)', emoji: '📱' },
  { tag: '💚 新品上架', title: '春夏新品', subtitle: '時尚穿搭一次擁有', bg: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 100%)', emoji: '👗' },
];

const staticSideBanners = [
  { tag: '商城', title: '新品喇叭上市', desc: '領券現折 $100', bg: 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)', emoji: '🔊' },
  { tag: '商城', title: '幫你換新機', desc: 'AI 筆電專區', bg: 'linear-gradient(135deg, #1e293b 0%, #334155 100%)', emoji: '💻' },
];

const allStaticBanners = computed(() => [...staticBanners, ...staticSideBanners]);

onMounted(async () => {
  // 活動資料與其他 API 並行
  void fetchActivePromotions().then(res => {
    if (res.success) promotions.value = res.data;
  }).catch(() => { /* 靜默失敗，fallback 到假資料 */ });

  try {
    const [walletRes, couponsRes, ordersRes] = await Promise.all([
      checkoutApi.getWalletBalance(),
      getMyCoupons(),
      getMyOrdersApi()
    ]);

    console.log('Member Center Data Sync:', { wallet: walletRes.data, orders: ordersRes.data });

    liveBalance.value = walletRes.data.pointBalance ?? walletRes.data.balance ?? 0;
    liveCouponCount.value = couponsRes.data.length;

    const allOrders = ordersRes.data;
    orderCounts.value = {
      pending: allOrders.filter(o => o.status === 0).length,
      processing: allOrders.filter(o => o.status === 1).length,
      shipped: allOrders.filter(o => o.status === 2).length,
      completed: allOrders.filter(o => o.status === 3 && !o.isReviewed && !(o as any).IsReviewed).length
    };

    authStore.updatePoints(liveBalance.value);
  } catch (err) {
    console.error('Failed to sync member data', err);
  }
});

const go = (name: string) => {
  switch (name) {
    case '設定': router.push('/member/settings'); break;
    case '所有訂單': router.push('/member/orders'); break;
    case '待付款': router.push({ path: '/member/orders', query: { tab: '0' } }); break;
    case '待出貨': router.push({ path: '/member/orders', query: { tab: '1' } }); break;
    case '待收貨': router.push({ path: '/member/orders', query: { tab: '2' } }); break;
    case '待評價': router.push({ path: '/member/orders', query: { tab: '3' } }); break;
    case '紅利點數': router.push('/member/wallet'); break;
    case '優惠券': router.push('/member/coupons'); break;
    case '我的賣場': router.push('/seller'); break;
    case '再買一次': router.push({ path: '/member/orders', query: { tab: '3' } }); break;
    case '會員權益': router.push('/member/level'); break;
    case '客服專區': router.push('/member/support'); break;
    case '領券中心': router.push('/coupons'); break;
    case '購物車': router.push('/cart'); break;
    default: router.push({ name: 'wip', query: { title: name } });
  }
};

// ── UI Data ──────────────────────────────────────
const orders = computed(() => [
  { label: "待付款", icon: "💳", badge: orderCounts.value.pending },
  { label: "待出貨", icon: "📦", badge: orderCounts.value.processing },
  { label: "待收貨", icon: "🚚", badge: orderCounts.value.shipped },
  { label: "待評價",   icon: "⭐", badge: orderCounts.value.completed },
]);

const services = [
  { label: "我的賣場", icon: "🏪", bg: "#FFF0EB" },
  { label: "領券中心", icon: "🎫", bg: "#FEF2F2" },
  { label: "再買一次", icon: "🔁", bg: "#EDF6FF" },
  { label: "會員權益", icon: "👑", bg: "#FFF8E8" },
  { label: "客服專區", icon: "🎧", bg: "#EDFAF4" },
];

// ── 活動跳轉（同步首頁邏輯） ───────────────────────
const goToPromo = (banner: any) => {
  if (!banner) return;
  const title = banner.title || '';
  const query: Record<string, string> = {};
  if (title) query['promoText'] = title;
  router.push({ path: '/products', query });
};

// ── Fake Data ─────────────────────────────────────
const fakeProducts = [
  { name: 'iPhone 16 保護殼', price: '299', emoji: '📱', bg: '#EEF2FF' },
  { name: '珍珠奶茶杯套', price: '89', emoji: '🧋', bg: '#FFF7ED' },
  { name: '無線藍牙耳機', price: '1,290', emoji: '🎧', bg: '#F0FDF4' },
  { name: '韓系帆布包', price: '680', emoji: '👜', bg: '#FDF4FF' },
];

const fakeRecommend = [
  { name: '復古祖母綠鍋具組', price: '2,580', emoji: '🍳', bg: '#ECFDF5', sold: '1.2k' },
  { name: 'realme C65 手機', price: '5,990', emoji: '📲', bg: '#EFF6FF', sold: '856' },
  { name: '璀璨亮片晚宴包', price: '1,180', emoji: '✨', bg: '#FFF7ED', sold: '432' },
  { name: '不鏽鋼隔空炸鍋', price: '3,490', emoji: '🥘', bg: '#FDF2F8', sold: '2.1k' },
];
</script>

<template>
  <div class="page">
    <div v-if="authStore.isBlacklisted" class="blacklist-banner">
      <el-alert
        title="您的帳號目前已停權"
        type="error"
        description="您的帳號因違反平台規範已暫時停權，目前的權限為「唯讀」。如有任何疑問或欲進行復權申訴，請聯繫平台管理員。"
        show-icon
        :closable="false"
      >
        <template #default>
          <div class="banner-actions">
            <el-button type="danger" size="small" @click="go('客服專區')">
              前往申訴管道
            </el-button>
          </div>
        </template>
      </el-alert>
    </div>

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

    <div class="card">
      <div class="card-header">
        <span class="card-label">我的錢包</span>
      </div>
      <div class="wallet-grid">
        <div class="wallet-item" @click="go('紅利點數')">
          <span class="wallet-icon">💎</span>
          <span class="wallet-value">{{ (liveBalance ?? authStore.memberInfo?.pointBalance ?? 0).toLocaleString() }}</span>
          <span class="wallet-label">我的點數</span>
        </div>
        <div class="wallet-item" @click="go('優惠券')">
          <span class="wallet-icon">🎟</span>
          <span class="wallet-value">{{ liveCouponCount }} 張</span>
          <span class="wallet-label">優惠券</span>
        </div>
      </div>
    </div>

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

    <div class="card promo-card-section" v-if="promotions.length > 0 || staticBanners.length > 0">
      <div class="card-header">
        <span class="card-label">🔥 限時活動</span>
        <span class="see-all" @click="router.push('/products')">查看全部 ›</span>
      </div>
      <div class="promo-list">
        <template v-if="promotions.length > 0">
          <div
            v-for="(promo, i) in promotions.slice(0, 4)"
            :key="promo.id"
            class="promo-item glass-item"
            :style="{
              borderColor: promoGlassColors[i % promoGlassColors.length].border
            }"
            @click="goToPromo(promo)"
          >
            <span
              class="promo-tag"
              :style="{ background: promoGlassColors[i % promoGlassColors.length].tag }"
            >{{ promo.typeLabel || '活動' }}</span>
            <div class="promo-title">{{ promo.title }}</div>
            <div v-if="promo.subtitle" class="promo-sub">{{ promo.subtitle }}</div>
          </div>
        </template>

        <template v-else>
          <div
            v-for="(banner, idx) in allStaticBanners"
            :key="idx"
            class="promo-item glass-item fallback-glass"
            @click="goToPromo(banner)"
          >
            <div class="promo-content">
              <span class="promo-tag gray-tag">{{ banner.tag }}</span>
              <div class="promo-title dark-text">{{ banner.title }}</div>
              <div v-if="banner.subtitle || (banner as any).desc" class="promo-sub dark-sub">
                {{ banner.subtitle || (banner as any).desc }}
              </div>
            </div>
            <div class="promo-emoji">{{ banner.emoji }}</div>
          </div>
        </template>
      </div>
    </div>

    <div class="card">
      <div class="card-header">
        <span class="card-label">最近瀏覽</span>
        <span class="see-all">查看全部 ›</span>
      </div>
      <div class="recent-grid">
        <div class="recent-item" v-for="p in fakeProducts" :key="p.name">
          <div class="recent-img" :style="{ background: p.bg }">{{ p.emoji }}</div>
          <div class="recent-name">{{ p.name }}</div>
          <div class="recent-price">NT$ {{ p.price }}</div>
        </div>
      </div>
    </div>

    <div class="card">
      <div class="card-header">
        <span class="card-label">為你推薦</span>
      </div>
      <div class="recommend-grid">
        <div class="rec-item" v-for="r in fakeRecommend" :key="r.name">
          <div class="rec-img" :style="{ background: r.bg }">{{ r.emoji }}</div>
          <div class="rec-info">
            <div class="rec-name">{{ r.name }}</div>
            <div class="rec-row">
              <span class="rec-price">NT$ {{ r.price }}</span>
              <span class="rec-sold">已售 {{ r.sold }}</span>
            </div>
          </div>
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

.blacklist-banner {
  margin: 12px;
}
.banner-actions {
  margin-top: 10px;
}

@media (max-width: 1200px) {
  .page {
    max-width: 100%;
  }
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

/* ── Promo (活動區塊外層) ───────────────────────── */
.promo-card-section {
  /* 給區塊一個淡淡的底色，讓玻璃感更明顯 */
  background: #fdfdfd;
}
.promo-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 8px 14px 16px;
}

/* ── Glassmorphism Item (玻璃感卡片核心樣式) ────── */
.glass-item {
  background: rgba(255, 255, 255, 0.6); /* 半透明白色底 */
  backdrop-filter: blur(10px);         /* 模糊背景 */
  -webkit-backdrop-filter: blur(10px); /* 為了 Safari 開發者 */
  border-radius: 12px;
  padding: 14px 16px;
  position: relative;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.3s ease;

  /* 預設邊框 (極細且淡) */
  border: 1px solid rgba(255, 255, 255, 0.2);

  /* 柔和的立體陰影 */
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03);
}
.glass-item:hover {
  transform: translateY(-2px);
  background: rgba(255, 255, 255, 0.75);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.06);
}

/* API 活動專用：保留配色的 Tag 樣式 */
.promo-tag {
  display: inline-block;
  color: #fff;
  font-size: 11px;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 20px;
  margin-bottom: 8px;
}
.promo-title { font-size: 14px; font-weight: 600; color: #333; margin-bottom: 3px; }
.promo-sub   { font-size: 12px; color: #666; }

/* ── Fallback Glass Styles (靜態活動專用) ────────── */
.fallback-glass {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-color: rgba(200, 200, 200, 0.2); /* 靜態活動使用灰邊框 */
}
.promo-content {
  flex: 1;
}
.gray-tag {
  background: #f0f0f0; /* 靜態 Tag 使用淡灰色 */
  color: #666;
  display: inline-block;
  font-size: 11px;
  font-weight: 600;
  padding: 3px 8px;
  border-radius: 20px;
  margin-bottom: 8px;
}
.dark-text {
  color: #333; /* 文字改為深色 */
}
.dark-sub {
  color: #777; /* 副標題改為中灰色 */
}
.promo-emoji {
  font-size: 36px;
  line-height: 1;
  margin-left: 15px;
  opacity: 0.8;
  /*Emoji 淡淡的投影*/
  filter: drop-shadow(0 2px 4px rgba(0,0,0,0.1));
}

/* ── Recent ──────────────────────────────────────── */
.recent-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 8px;
  padding: 4px 12px 14px;
}
@media (min-width: 768px) {
  .recent-grid {
    grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  }
}
.recent-item { cursor: pointer; }
.recent-img {
  width: 100%;
  aspect-ratio: 1;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  margin-bottom: 5px;
}
.recent-name  { font-size: 11px; color: #444; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.recent-price { font-size: 12px; color: #EE4D2D; font-weight: 600; margin-top: 2px; }

/* ── Recommend ───────────────────────────────────── */
.recommend-grid {
  display: flex;
  flex-direction: column;
  padding: 4px 14px 14px;
  gap: 10px;
}
.rec-item {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  padding: 6px 0;
  border-bottom: 0.5px solid #F0F0F0;
}
.rec-item:last-child { border-bottom: none; }
.rec-img {
  width: 52px;
  height: 52px;
  border-radius: 8px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
}
.rec-info  { flex: 1; min-width: 0; }
.rec-name  { font-size: 13px; color: #333; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.rec-row   { display: flex; align-items: center; justify-content: space-between; margin-top: 4px; }
.rec-price { font-size: 14px; color: #EE4D2D; font-weight: 700; }
.rec-sold  { font-size: 11px; color: #999; }
</style>
