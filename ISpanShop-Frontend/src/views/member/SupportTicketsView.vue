<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { getMyTickets, createTicket } from '@/api/support';
import { SUPPORT_CATEGORIES, TICKET_STATUS, type SupportTicket } from '@/types/support';
import { ElMessage, type FormInstance, type FormRules } from 'element-plus';
import { ChatDotSquare, Plus, Refresh } from '@element-plus/icons-vue';

const route = useRoute();
// 列表資料
const tickets = ref<SupportTicket[]>([]);
const loading = ref(false);

// 表單相關
const dialogVisible = ref(false);
const formRef = ref<FormInstance>();
const submitLoading = ref(false);
const formModel = ref<Partial<SupportTicket>>({
  category: 0,
  subject: '',
  description: '',
  orderId: null
});

const rules: FormRules = {
  subject: [{ required: true, message: '請輸入主旨', trigger: 'blur' }],
  description: [{ required: true, message: '請輸入內容描述', trigger: 'blur' }],
  category: [{ required: true, message: '請選擇類別', trigger: 'change' }]
};

// 取得資料
const fetchTickets = async () => {
  loading.value = true;
  try {
    const res = await getMyTickets();
    // Axios 回傳的是物件，真正的資料在 res.data
    const responseData = (res as any).data || res;
    
    if (Array.isArray(responseData)) {
      // 後端回傳可能是大寫開頭 (C#) 或小寫開頭 (JSON)，統一轉為前端使用的小寫
      tickets.value = responseData.map((t: any) => ({
        id: t.id ?? t.Id,
        userId: t.userId ?? t.UserId,
        category: t.category ?? t.Category,
        subject: t.subject ?? t.Subject,
        description: t.description ?? t.Description,
        status: t.status ?? t.Status ?? 0,
        adminReply: t.adminReply ?? t.AdminReply,
        createdAt: t.createdAt ?? t.CreatedAt,
        resolvedAt: t.resolvedAt ?? t.ResolvedAt,
        orderId: t.orderId ?? t.OrderId
      }));
      
      // 確保最新的在最上面
      tickets.value.sort((a, b) => {
        const dateA = new Date(a.createdAt || 0).getTime();
        const dateB = new Date(b.createdAt || 0).getTime();
        return dateB - dateA;
      });
    }
  } catch (error) {
    console.error('獲取工單列表失敗', error);
  } finally {
    loading.value = false;
  }
};

// 提交表單
const handleSubmit = async () => {
  if (!formRef.value) return;
  
  await formRef.value.validate(async (valid) => {
    if (valid) {
      submitLoading.value = true;
      try {
        const payload = {
          userId: 0, // 後端會自動覆蓋
          userName: "",
          category: formModel.value.category,
          subject: formModel.value.subject,
          description: formModel.value.description,
          orderId: formModel.value.orderId || null,
          attachmentUrl: "",
          status: 0,
          adminReply: "",
          createdAt: new Date().toISOString()
        };

        await createTicket(payload);
        ElMessage.success('提交成功，我們將儘速處理您的問題');
        dialogVisible.value = false;
        // 重置表單並重新整理列表
        formModel.value = { category: 0, subject: '', description: '', orderId: null };
        fetchTickets();
      } catch (error: any) {
        console.error('提交申訴失敗:', error);
        
        let errorMsg = '提交失敗，請稍後再試';
        const responseData = error.response?.data;
        
        if (responseData) {
          if (responseData.errors) {
            // 取得 ASP.NET Core 的驗證錯誤
            const firstErrorKey = Object.keys(responseData.errors)[0];
            const firstErrorVal = responseData.errors[firstErrorKey];
            errorMsg = `${firstErrorKey}: ${Array.isArray(firstErrorVal) ? firstErrorVal[0] : firstErrorVal}`;
          } else if (responseData.message) {
            errorMsg = responseData.message;
          } else if (responseData.title) {
            errorMsg = responseData.title;
          }
        }
        
        ElMessage.error({
          message: errorMsg,
          duration: 5000,
          showClose: true
        });
      } finally {
        submitLoading.value = false;
      }
    }
  });
};

const getStatusInfo = (status: number) => {
  return TICKET_STATUS[status as keyof typeof TICKET_STATUS] || { label: '未知', type: 'info' };
};

const getCategoryLabel = (category: number) => {
  return SUPPORT_CATEGORIES.find(c => c.value === category)?.label || '其他';
};

const formatDate = (dateStr?: string) => {
  if (!dateStr) return '-';
  return new Date(dateStr).toLocaleString('zh-TW');
};

onMounted(() => {
  fetchTickets();
  
  // 處理從訂單詳情跳轉過來的申訴
  if (route.query.orderId) {
    formModel.value.orderId = Number(route.query.orderId);
    formModel.value.category = 0; // 預設為訂單問題
    formModel.value.subject = `關於訂單 #${route.query.orderId} 的申訴`;
    dialogVisible.value = true;
  }
});
</script>

<template>
  <div class="support-page">
    <div class="header">
      <div class="title-section">
        <el-icon class="title-icon"><ChatDotSquare /></el-icon>
        <h2>客戶服務中心</h2>
      </div>
      <div class="action-section">
        <el-button @click="fetchTickets" :icon="Refresh" circle />
        <el-button type="primary" :icon="Plus" @click="dialogVisible = true">
          新的諮詢
        </el-button>
      </div>
    </div>

    <el-card shadow="never" class="list-card">
      <el-table :data="tickets" v-loading="loading" style="width: 100%">
        <el-table-column type="expand">
          <template #default="{ row }">
            <div class="ticket-detail">
              <div class="detail-item">
                <span class="label">問題描述：</span>
                <p class="content">{{ row.description }}</p>
              </div>
              <div class="detail-item admin-reply" v-if="row.adminReply">
                <span class="label">客服回覆：</span>
                <p class="content">{{ row.adminReply }}</p>
                <div class="reply-meta">回覆時間：{{ formatDate(row.resolvedAt) }}</div>
              </div>
              <div class="detail-item no-reply" v-else>
                <el-alert title="客服人員正在處理中，請耐心等候。" type="info" :closable="false" show-icon />
              </div>
            </div>
          </template>
        </el-table-column>

        <el-table-column label="編號" prop="id" width="80" />
        
        <el-table-column label="類別" width="120">
          <template #default="{ row }">
            <el-tag effect="plain">{{ getCategoryLabel(row.category) }}</el-tag>
          </template>
        </el-table-column>

        <el-table-column label="主旨" prop="subject" min-width="200" show-overflow-tooltip />

        <el-table-column label="狀態" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusInfo(row.status).type">
              {{ getStatusInfo(row.status).label }}
            </el-tag>
          </template>
        </el-table-column>

        <el-table-column label="建立時間" width="180">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
      </el-table>

      <template v-if="tickets.length === 0 && !loading">
        <div class="empty-state">
          <p>目前沒有諮詢紀錄</p>
          <el-button type="primary" link @click="dialogVisible = true">點擊此處開始您的第一個諮詢</el-button>
        </div>
      </template>
    </el-card>

    <!-- 新增工單彈窗 -->
    <el-dialog
      v-model="dialogVisible"
      title="新的諮詢"
      width="500px"
      destroy-on-close
    >
      <el-form
        ref="formRef"
        :model="formModel"
        :rules="rules"
        label-position="top"
      >
        <el-form-item label="問題類別" prop="category">
          <el-select v-model="formModel.category" placeholder="請選擇類別" style="width: 100%">
            <el-option
              v-for="item in SUPPORT_CATEGORIES"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="相關訂單 ID (選填)" prop="orderId">
          <el-input-number v-model="formModel.orderId" :min="1" placeholder="例如: 1001" style="width: 100%" />
        </el-form-item>

        <el-form-item label="主旨" prop="subject">
          <el-input v-model="formModel.subject" placeholder="請簡述您的問題" />
        </el-form-item>

        <el-form-item label="詳細描述" prop="description">
          <el-input
            v-model="formModel.description"
            type="textarea"
            :rows="4"
            placeholder="請提供更多細節，以便我們為您提供協助"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogVisible = false">取消</el-button>
          <el-button type="primary" @click="handleSubmit" :loading="submitLoading">
            提交申請
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped lang="scss">
.support-page {
  padding: 20px;
  max-width: 1000px;
  margin: 0 auto;

  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;

    .title-section {
      display: flex;
      align-items: center;
      gap: 12px;
      h2 {
        margin: 0;
        font-size: 22px;
        color: #303133;
      }
      .title-icon {
        font-size: 24px;
        color: #409eff;
      }
    }
  }

  .list-card {
    border-radius: 8px;
  }

  .ticket-detail {
    padding: 20px 40px;
    background-color: #f8f9fa;
    border-radius: 4px;

    .detail-item {
      margin-bottom: 16px;
      &:last-child { margin-bottom: 0; }
      
      .label {
        font-weight: bold;
        color: #606266;
        display: block;
        margin-bottom: 8px;
      }
      .content {
        margin: 0;
        line-height: 1.6;
        color: #303133;
        white-space: pre-wrap;
      }
    }

    .admin-reply {
      margin-top: 16px;
      padding: 16px;
      background-color: #ecf5ff;
      border-left: 4px solid #409eff;
      .reply-meta {
        margin-top: 12px;
        font-size: 12px;
        color: #909399;
        text-align: right;
      }
    }
  }

  .empty-state {
    padding: 60px 0;
    text-align: center;
    color: #909399;
  }
}
</style>
