<template>
  <div class="refund-page">
    <div class="refund-container" v-loading="loading">
      <div class="page-header">
        <el-button link @click="router.back()">
          <el-icon><ArrowLeft /></el-icon> 返回
        </el-button>
        <h2 class="title">申請退貨/退款</h2>
      </div>

      <!-- 1. 商品選擇 (部分退款邏輯) -->
      <el-card class="section-card" shadow="never">
        <template #header>
          <div class="card-header">
            <span>選擇退貨商品</span>
            <el-checkbox v-model="checkAll" :indeterminate="isIndeterminate" @change="handleCheckAllChange">全選</el-checkbox>
          </div>
        </template>
        
        <div class="product-list">
          <div v-for="item in order?.items" :key="item.id" class="product-item">
            <el-checkbox v-model="selectedItems" :label="item.id" @change="handleItemChange">
              <div class="item-content">
                <el-image :src="item.coverImage" class="item-img" fit="cover" />
                <div class="item-info">
                  <div class="name">{{ item.productName }}</div>
                  <div class="variant">{{ item.variantName }}</div>
                  <div class="price-qty">
                    <span class="price">${{ formatPrice(item.price) }}</span>
                    <span class="qty">x{{ item.quantity }}</span>
                  </div>
                </div>
              </div>
            </el-checkbox>
            <!-- 選擇數量 -->
            <div class="qty-selector" v-if="selectedItems.includes(item.id)">
              <span class="label">退貨數量:</span>
              <el-input-number 
                v-model="returnQuantities[item.id]" 
                :min="1" 
                :max="item.quantity" 
                size="small" 
              />
            </div>
          </div>
        </div>
      </el-card>

      <!-- 2. 退貨/款設定 -->
      <el-card class="section-card" shadow="never">
        <el-form :model="form" label-position="top">
          <div v-if="hasDiscounts" class="discount-info-box">
            <div class="discount-title">本訂單折抵紀錄</div>
            <div class="discount-row" v-if="order?.pointDiscount">
              <span class="label">點數折抵:</span>
              <span class="value">-${{ formatPrice(order.pointDiscount) }}</span>
            </div>
            <div class="discount-row" v-if="order?.discountAmount">
              <span class="label">優惠券折抵 <span v-if="order.couponTitle" class="text-xs">({{ order.couponTitle }})</span>:</span>
              <span class="value">-${{ formatPrice(order.discountAmount) }}</span>
            </div>
            <div class="divider"></div>
            <div class="discount-row refund-total">
              <span class="label fw-bold">預計退款金額:</span>
              <span class="value fw-bold fs-5 text-danger">${{ formatPrice(totalRefundAmount) }}</span>
            </div>
            <div class="text-xs text-gray-400 mt-2">
              註：全額退貨將退還最終實付金額。部分退款將依商品金額比例扣除折抵。
            </div>
          </div>

          <el-form-item label="退款類型" required>
            <el-select v-model="form.type" placeholder="請選擇退款類型" class="w-full">
              <el-option label="退貨退款" value="ReturnAndRefund" />
              <el-option label="僅退款 (未收到貨或已與賣家達成協議)" value="RefundOnly" />
            </el-select>
          </el-form-item>

          <el-form-item label="退款原因" required>
            <el-select v-model="form.reasonCategory" placeholder="請選擇退款原因" class="w-full">
              <el-option label="商品瑕疵/損壞" value="商品瑕疵" />
              <el-option label="寄錯商品/尺寸/顏色" value="寄錯商品" />
              <el-option label="商品描述不符" value="描述不符" />
              <el-option label="少寄商品" value="少寄商品" />
              <el-option label="我不想要了/買錯了" value="買錯了" />
            </el-select>
          </el-form-item>

          <el-form-item label="補充說明">
            <el-input 
              v-model="form.reasonDescription" 
              type="textarea" 
              rows="3" 
              placeholder="請詳細描述商品問題，有助於賣家更快處理您的申請"
            />
          </el-form-item>

          <el-form-item label="上傳憑證 (最多 3 張)">
            <el-upload
              action="#"
              list-type="picture-card"
              :auto-upload="false"
              :limit="3"
              :on-change="handleUploadChange"
              :on-remove="handleRemove"
              :file-list="form.images"
            >
              <el-icon><Plus /></el-icon>
            </el-upload>
          </el-form-item>
        </el-form>
      </el-card>

      <!-- 3. 底部結算與送出 -->
      <div class="refund-footer">
        <div class="amount-summary">
          <span class="label">預計退款金額:</span>
          <span class="value">${{ formatPrice(totalRefundAmount) }}</span>
        </div>
        <el-button type="primary" size="large" class="submit-btn" :disabled="!isFormValid" @click="handleSubmit">
          送出申請
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, reactive } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, Plus } from '@element-plus/icons-vue';
import { getOrderDetailApi, requestRefundApi, uploadImagesApi } from '@/api/order';
import type { OrderDetail } from '@/types/order';
import { ElMessage } from 'element-plus';

const route = useRoute();
const router = useRouter();
const loading = ref(false);
const order = ref<OrderDetail | null>(null);

// ── 商品選擇狀態 ──
const selectedItems = ref<number[]>([]);
const returnQuantities = reactive<Record<number, number>>({});
const checkAll = ref(false);
const isIndeterminate = ref(false);

// ── 表單狀態 ──
const form = reactive({
  type: '',
  reasonCategory: '',
  reasonDescription: '',
  images: [] as any[]
});

const fetchOrder = async () => {
  const id = Number(route.params.id);
  loading.value = true;
  try {
    const res = await getOrderDetailApi(id);
    order.value = res.data;
    // 初始化退貨數量與全選狀態
    order.value.items.forEach(item => {
      returnQuantities[item.id] = item.quantity;
    });
    
    // 預設全選所有商品
    checkAll.value = true;
    selectedItems.value = order.value.items.map(i => i.id);
    isIndeterminate.value = false;
  } catch (error) {
    ElMessage.error('獲取訂單資訊失敗');
  } finally {
    loading.value = false;
  }
};

const handleCheckAllChange = (val: any) => {
  selectedItems.value = val ? order.value?.items.map(i => i.id) || [] : [];
  isIndeterminate.value = false;
};

const handleItemChange = () => {
  const count = selectedItems.value.length;
  const total = order.value?.items.length || 0;
  checkAll.value = count === total;
  isIndeterminate.value = count > 0 && count < total;
};

const hasDiscounts = computed(() => {
  return (order.value?.pointDiscount || 0) > 0 || (order.value?.discountAmount || 0) > 0;
});

const totalRefundAmount = computed(() => {
  if (!order.value) return 0;

  // 計算所選商品的原價總和
  const itemsOriginalTotal = selectedItems.value.reduce((sum, id) => {
    const item = order.value?.items.find(i => i.id === id);
    if (item) {
      return sum + (item.price * (returnQuantities[id] || 0));
    }
    return sum;
  }, 0);

  // 如果全選且所有數量皆等於購買數量，直接退 finalAmount (包含運費、扣除折抵等最終實付金額)
  const isFullReturn = order.value.items.length === selectedItems.value.length && 
      selectedItems.value.every(id => returnQuantities[id] === order.value?.items.find(i => i.id === id)?.quantity);

  if (isFullReturn) {
    return order.value.finalAmount;
  }

  // 若為部分退貨，按比例分配折抵金額
  const orderTotal = order.value.totalAmount; // 商品總原價
  if (orderTotal === 0) return 0;

  const ratio = itemsOriginalTotal / orderTotal;
  
  const totalDiscount = (order.value.pointDiscount || 0) + (order.value.discountAmount || 0);
  const proportionDiscount = Math.round(totalDiscount * ratio);
  
  const refundAmount = itemsOriginalTotal - proportionDiscount;
  
  return refundAmount > 0 ? refundAmount : 0;
});

const isFormValid = computed(() => {
  return selectedItems.value.length > 0 && form.type && form.reasonCategory;
});

const handleUploadChange = (file: any, fileList: any[]) => {
  form.images = fileList;
};

const handleRemove = (file: any, fileList: any[]) => {
  form.images = fileList;
};

const handleSubmit = async () => {
  const id = Number(route.params.id);
  loading.value = true;
  try {
    let uploadedUrls: string[] = [];

    // 1. 如果有圖片，先上傳
    if (form.images.length > 0) {
      const formData = new FormData();
      form.images.forEach(img => {
        if (img.raw) {
          formData.append('files', img.raw);
        }
      });
      
      const res = await uploadImagesApi(formData);
      uploadedUrls = res.data.urls;
    }

    // 2. 送出退貨申請
    const typeLabel = form.type === 'ReturnAndRefund' ? '退貨退款' : (form.type === 'RefundOnly' ? '僅退款' : form.type);
    const payload = {
      reasonCategory: `[${typeLabel}] ${form.reasonCategory}`,
      reasonDescription: form.reasonDescription,
      items: selectedItems.value.map(id => ({
        orderDetailId: id,
        quantity: returnQuantities[id]
      })),
      imageUrls: uploadedUrls
    };
    
    await requestRefundApi(id, payload);
    ElMessage.success('退貨申請已送出');
    router.push('/member/orders');
  } catch (error: any) {
    console.error('Submit error:', error);
    ElMessage.error(error.response?.data?.message || '申請失敗');
  } finally {
    loading.value = false;
  }
};

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('zh-TW').format(price);
};

onMounted(fetchOrder);
</script>

<style scoped lang="scss">
.refund-page {
  background-color: #f5f5f5;
  min-height: calc(100vh - 100px);
  padding: 20px 0;
}

.refund-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 15px;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 20px;
  .title { margin: 0; font-size: 20px; }
}

.section-card {
  margin-bottom: 15px;
  border-radius: 4px;
  
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
}

.product-list {
  padding: 10px 0;
}

.product-item {
  padding: 15px 0;
  border-bottom: 1px solid #f0f0f0;
  &:last-child { border-bottom: none; }

  :deep(.el-checkbox) {
    height: auto;
    width: 100%;
    display: flex;
    align-items: center;
    
    .el-checkbox__label {
      flex: 1;
      padding-left: 0;
    }
  }

  .item-content {
    display: flex;
    gap: 12px;
    margin-left: 10px;

    .item-img {
      width: 60px;
      height: 60px;
      border-radius: 4px;
      flex-shrink: 0;
      border: 1px solid #eee;
    }

    .item-info {
      flex: 1;
      .name { font-size: 14px; color: #333; margin-bottom: 4px; line-height: 1.4; white-space: normal; }
      .variant { font-size: 12px; color: #999; margin-bottom: 8px; }
      .price-qty {
        display: flex;
        gap: 15px;
        .price { color: #ee4d2d; font-weight: 500; }
        .qty { color: #999; }
      }
    }
  }

  .qty-selector {
    margin-top: 10px;
    margin-left: 95px;
    display: flex;
    align-items: center;
    gap: 15px;
    .label { font-size: 13px; color: #666; }
  }
}

.w-full { width: 100%; }

.discount-info-box {
  background-color: #fafafa;
  border: 1px dashed #e4e4e4;
  padding: 15px;
  border-radius: 4px;
  margin-bottom: 20px;

  .discount-title {
    font-size: 14px;
    font-weight: bold;
    color: #666;
    margin-bottom: 10px;
  }

  .discount-row {
    display: flex;
    justify-content: space-between;
    margin-bottom: 5px;
    font-size: 14px;

    .label { color: #666; }
    .value { color: #ee4d2d; }
  }

  .text-xs { font-size: 12px; }
  .text-gray-400 { color: #999; }
  .mt-2 { margin-top: 10px; }

  .divider {
    height: 1px;
    background-color: #e4e4e4;
    margin: 12px 0;
  }

  .refund-total {
    margin-top: 10px;
    
    .label { 
      font-weight: 600; 
      color: #333; 
    }
    .value { 
      font-weight: bold; 
      font-size: 18px; 
    }
  }
}

.refund-footer {
  margin-top: 30px;
  background: #fff;
  padding: 20px;
  border-radius: 4px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 -2px 10px rgba(0,0,0,0.05);

  .amount-summary {
    .label { font-size: 16px; color: #333; margin-right: 10px; }
    .value { font-size: 24px; color: #ee4d2d; font-weight: bold; }
  }

  .submit-btn {
    min-width: 150px;
  }
}
</style>
