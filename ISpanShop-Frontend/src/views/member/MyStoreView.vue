<template>
  <div class="my-store-container">
    <div class="page-header">
      <div class="header-left">
        <el-button @click="router.back()" circle icon="ArrowLeft" />
        <h2 class="title">賣場狀態檢查</h2>
      </div>
    </div>

    <div v-loading="loading" class="status-content">
      <!-- 1. 尚未申請：留在本頁顯示申請引導 -->
      <div v-if="status === 'NotApplied'" class="status-box">
        <el-empty description="您尚未擁有賣場">
          <el-button type="primary" @click="router.push('/member/seller-apply')">
            立即申請成為賣家
          </el-button>
        </el-empty>
      </div>

      <!-- 2. 審核中：留在本頁顯示提示 -->
      <div v-else-if="status === 'Pending'" class="status-box">
        <el-empty image="https://gw.alipayobjects.com/zos/antfincdn/ZHrcdLPrvN/empty.svg" description="賣場申請審核中">
          <template #extra>
            <p class="status-tip">管理員正在審核您的申請，請耐心等候。</p>
            <el-button @click="checkStatus">重新整理狀態</el-button>
          </template>
        </el-empty>
      </div>

      <!-- 3. 審核被駁回：留在本頁顯示駁回訊息 -->
      <div v-else-if="status === 'Rejected'" class="status-box">
        <el-result icon="error" title="申請被駁回" sub-title="很抱歉，您的賣場申請未通過審核。">
          <template #extra>
            <el-button type="primary" @click="router.push('/member/seller-apply')">查看詳情並重新申請</el-button>
          </template>
        </el-result>
      </div>

      <!-- 4. 審核通過：顯示轉場並立即跳轉 -->
      <div v-else-if="status === 'Approved'" class="status-box">
        <el-result icon="success" title="審核已通過" sub-title="正在為您導向賣家中心...">
        </el-result>
      </div>
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
const loading = ref(false);
const status = ref<StoreStatus | ''>('');

const checkStatus = async () => {
  loading.value = true;
  try {
    const res = await getStoreStatusApi();
    status.value = res.data.status;

    if (status.value === 'Approved') {
      // 同步更新前端 AuthStore 身分，確保路由守衛放行
      authStore.updateSellerStatus(true);
      
      // 延時一小段時間讓使用者看清狀態，然後跳轉至正式賣家中心 (/seller)
      setTimeout(() => {
        router.replace('/seller');
      }, 800);
    } else {
      // 若未通過，確保身分為 false (同步資料庫最新狀態)
      authStore.updateSellerStatus(false);
    }
  } catch (error: any) {
    console.error('檢查賣場狀態失敗', error);
    ElMessage.error('無法取得賣場狀態');
  } finally {
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
