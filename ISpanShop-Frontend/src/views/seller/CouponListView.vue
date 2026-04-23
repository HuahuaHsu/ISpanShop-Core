<template>
  <div class="coupon-list-page">
    <div class="page-header">
      <h1 class="page-title">我的優惠券</h1>
      <el-button type="primary" @click="openCreateDialog">
        <el-icon style="margin-right: 6px;"><Plus /></el-icon>新增優惠券
      </el-button>
    </div>

    <el-card v-loading="loading" shadow="never" class="table-card">
      <el-table :data="coupons" stripe style="width: 100%">
        <template #empty>
          <el-empty description="目前沒有優惠券，點擊右上角新增" />
        </template>
        <el-table-column prop="title" label="名稱" min-width="150" />
        <el-table-column prop="couponCode" label="代碼" width="120" />
        <el-table-column label="類型" width="100">
          <template #default="{ row }">
            <el-tag :type="row.couponType === 1 ? 'success' : 'warning'">
              {{ row.couponType === 1 ? '固定金額' : '百分比' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="折扣" width="100">
          <template #default="{ row }">
            {{ row.couponType === 1 ? `$${row.discountValue}` : `${row.discountValue}% off` }}
          </template>
        </el-table-column>
        <el-table-column label="門檻" width="100">
          <template #default="{ row }">
            ${{ row.minimumSpend }}
          </template>
        </el-table-column>
        <el-table-column label="數量" width="120">
          <template #default="{ row }">
            {{ row.usedQuantity }} / {{ row.totalQuantity }}
          </template>
        </el-table-column>
        <el-table-column label="效期" min-width="200">
          <template #default="{ row }">
            {{ formatDate(row.startTime) }} ~ {{ formatDate(row.endTime) }}
          </template>
        </el-table-column>
        <el-table-column label="狀態" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row)">
              {{ getStatusLabel(row) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="openEditDialog(row)">編輯</el-button>
            <el-button link type="danger" @click="handleDelete(row.id)">刪除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增/編輯彈窗 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '編輯優惠券' : '新增優惠券'"
      width="500px"
    >
      <el-form :model="formData" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item label="名稱" prop="title">
          <el-input v-model="formData.title" placeholder="如：開學季滿千折百" />
        </el-form-item>
        <el-form-item label="代碼" prop="couponCode">
          <el-input v-model="formData.couponCode" placeholder="如：SCHOOL2026" />
        </el-form-item>
        <el-form-item label="折扣類型" prop="couponType">
          <el-radio-group v-model="formData.couponType">
            <el-radio :value="1">固定金額</el-radio>
            <el-radio :value="2">百分比</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="formData.couponType === 1 ? '折扣金額' : '折扣比例'" prop="discountValue">
          <el-input-number v-model="formData.discountValue" :min="1" />
          <span v-if="formData.couponType === 2" class="hint"> % (例如 20 代表打 8 折)</span>
        </el-form-item>
        <el-form-item label="最低消費" prop="minimumSpend">
          <el-input-number v-model="formData.minimumSpend" :min="0" />
        </el-form-item>
        <el-form-item label="總發放量" prop="totalQuantity">
          <el-input-number v-model="formData.totalQuantity" :min="1" />
        </el-form-item>
        <el-form-item label="每人限領" prop="perUserLimit">
          <el-input-number v-model="formData.perUserLimit" :min="1" />
        </el-form-item>
        <el-form-item label="開始時間" prop="startTime">
          <el-date-picker v-model="formData.startTime" type="datetime" value-format="YYYY-MM-DDTHH:mm:ss" />
        </el-form-item>
        <el-form-item label="結束時間" prop="endTime">
          <el-date-picker v-model="formData.endTime" type="datetime" value-format="YYYY-MM-DDTHH:mm:ss" />
        </el-form-item>
        <el-form-item label="狀態" prop="status">
          <el-select v-model="formData.status">
            <el-option label="啟用" :value="1" />
            <el-option label="停用" :value="0" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">確認</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Plus } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { fetchSellerCoupons, createSellerCoupon, updateSellerCoupon, deleteSellerCoupon, type Coupon } from '@/api/coupon';

const loading = ref(false);
const coupons = ref<Coupon[]>([]);
const dialogVisible = ref(false);
const isEdit = ref(false);
const submitting = ref(false);
const formRef = ref();

// 輔助函式：格式化日期為 YYYY-MM-DDTHH:mm:ss
const getFormattedDate = (date: Date) => {
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  const hh = String(date.getHours()).padStart(2, '0');
  const min = String(date.getMinutes()).padStart(2, '0');
  const ss = String(date.getSeconds()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}T${hh}:${min}:${ss}`;
};

const getDefaultDates = () => {
  const now = new Date();
  const nextMonth = new Date();
  nextMonth.setMonth(now.getMonth() + 1);
  return {
    start: getFormattedDate(now),
    end: getFormattedDate(nextMonth)
  };
};

const defaultDates = getDefaultDates();

const formData = ref<Partial<Coupon>>({
  title: '',
  couponCode: '',
  couponType: 1,
  discountValue: 100,
  minimumSpend: 0,
  totalQuantity: 100,
  perUserLimit: 1,
  startTime: defaultDates.start,
  endTime: defaultDates.end,
  status: 1
});

const rules = {
  title: [{ required: true, message: '請輸入名稱', trigger: 'blur' }],
  couponCode: [{ required: true, message: '請輸入代碼', trigger: 'blur' }],
  startTime: [{ required: true, message: '請選擇開始時間', trigger: 'change' }],
  endTime: [{ required: true, message: '請選擇結束時間', trigger: 'change' }],
};

const loadCoupons = async () => {
  loading.value = true;
  try {
    const response = await fetchSellerCoupons();
    console.log('API Response:', response);
    
    // Axios 回傳的資料在 response.data
    const res = response.data;
    
    if (res && res.success) {
      // 支援多種回傳格式 (items 陣列或直接是 data 陣列)
      const data = res.data;
      coupons.value = (data as any)?.items || (Array.isArray(data) ? data : []);
      console.log('Coupons updated:', coupons.value);
    }
  } catch (error) {
    console.error('Failed to load coupons:', error);
    ElMessage.error('載入失敗');
  } finally {
    loading.value = false;
  }
};

const openCreateDialog = () => {
  isEdit.value = false;
  const dates = getDefaultDates();
  formData.value = {
    title: '',
    couponCode: '',
    couponType: 1,
    discountValue: 100,
    minimumSpend: 0,
    totalQuantity: 100,
    perUserLimit: 1,
    startTime: dates.start,
    endTime: dates.end,
    status: 1
  };
  dialogVisible.value = true;
};

const openEditDialog = (row: Coupon) => {
  isEdit.value = true;
  formData.value = { ...row };
  dialogVisible.value = true;
};

const handleSubmit = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid: boolean) => {
    if (!valid) return;
    submitting.value = true;
    try {
      if (isEdit.value) {
        await updateSellerCoupon(formData.value.id!, formData.value);
        ElMessage.success('更新成功');
      } else {
        await createSellerCoupon(formData.value);
        ElMessage.success('建立成功');
      }
      dialogVisible.value = false;
      await loadCoupons();
    } catch (error) {
      ElMessage.error('操作失敗');
    } finally {
      submitting.value = false;
    }
  });
};

const handleDelete = async (id: number) => {
  try {
    await ElMessageBox.confirm('確定要刪除這張優惠券嗎？', '提示', { type: 'warning' });
    await deleteSellerCoupon(id);
    ElMessage.success('已刪除');
    await loadCoupons();
  } catch (error) {
    // cancel
  }
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '-';
  return dateStr.replace('T', ' ').substring(0, 16);
};

const getStatusLabel = (row: Coupon) => {
  if (row.status === 0) return '停用';
  const now = new Date();
  if (new Date(row.endTime) < now) return '已過期';
  if (new Date(row.startTime) > now) return '未開始';
  return '使用中';
};

const getStatusType = (row: Coupon) => {
  if (row.status === 0) return 'info';
  const now = new Date();
  if (new Date(row.endTime) < now) return 'danger';
  if (new Date(row.startTime) > now) return 'warning';
  return 'success';
};

onMounted(loadCoupons);
</script>

<style scoped>
.coupon-list-page {
  padding: 24px;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}
.page-title {
  font-size: 24px;
  font-weight: bold;
  margin: 0;
}
.table-card {
  border-radius: 8px;
}
.hint {
  font-size: 12px;
  color: #999;
  margin-left: 8px;
}
</style>
