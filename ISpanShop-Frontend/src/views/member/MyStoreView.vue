<template>
  <div class="my-store-container">
    <div class="page-header">
      <div class="header-left">
        <el-button @click="router.back()" circle icon="ArrowLeft" />
        <h2 class="title">賣場狀態檢查</h2>
      </div>
    </div>

    <!-- 關鍵：使用 v-loading 覆蓋整個內容區，且在 loading 為 true 時不顯示任何狀態盒 -->
    <div v-loading="loading" class="status-content">
      <template v-if="!loading">
        <!-- 1. 尚未申請 -->
        <div v-if="status === 'NotApplied'" class="status-box">
          <el-empty description="您尚未擁有賣場">
            <el-button type="primary" @click="router.push('/member/seller-apply')">
              立即申請成為賣家
            </el-button>
          </el-empty>
        </div>

        <!-- 2. 審核中 -->
        <div v-else-if="status === 'Pending'" class="status-box">
          <el-empty image="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg" description="賣場申請審核中">
            <template #extra>
              <p class="status-tip">管理員正在審核您的申請，請耐心等候。</p>
              <el-button @click="checkStatus">重新整理狀態</el-button>
            </template>
          </el-empty>
        </div>

        <!-- 3. 審核被駁回 -->
        <div v-else-if="status === 'Rejected'" class="status-box">
          <el-result icon="error" title="申請被駁回" sub-title="很抱歉，您的賣場申請未通過審核。">
            <template #extra>
              <el-button type="primary" @click="router.push('/member/seller-apply')">查看詳情並重新申請</el-button>
            </template>
          </el-result>
        </div>
      </template>

      <!-- 4. 審核通過：不顯示 UI，由腳本在背景完成跳轉 -->
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowLeft } from '@element-plus/icons-vue';
import { getStoreStatusApi } from '@/api/store';
import { useAuthStore } from '@/stores/auth';
import type { StoreStatus } from '@/types/store';
import { ElMessage } from 'element-plus';

const router = useRouter();
const authStore = useAuthStore();
const loading = ref(true); // 預設為 true，避免一進來就閃現內容
const status = ref<StoreStatus | ''>('');

const checkStatus = async () => {
  loading.value = true;
  try {
    const res = await getStoreStatusApi();
    status.value = res.data.status;

    if (status.value === 'Approved') {
      authStore.updateSellerStatus(true);
      // 直接跳轉，不關閉 loading，達到完全無感
      router.replace('/seller');
    } else {
      // 關鍵：如果不是 Approved，一定要把 isSeller 設為 false，防止緩存錯誤
      authStore.updateSellerStatus(false);
      loading.value = false; // 只有在確定不是 Approved 時才顯示內容
    }
  } catch (error: any) {
    console.error('檢查賣場狀態失敗', error);
    ElMessage.error('無法取得賣場狀態');
    loading.value = false;
  }
};

onMounted(() => {
  checkStatus();
});
</script>

<style scoped lang="scss">
.my-store-container {
  padding: 20px;
}

.page-header {
  margin-bottom: 30px;
  .header-left {
    display: flex;
    align-items: center;
    gap: 15px;
    .title { margin: 0; font-size: 1.25rem; font-weight: 600; }
  }
}

.status-content {
  min-height: 450px;
  background: #fff;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.status-box {
  text-align: center;
  .status-tip {
    color: #8c8c8c;
    margin-bottom: 20px;
  }
}
</style>
