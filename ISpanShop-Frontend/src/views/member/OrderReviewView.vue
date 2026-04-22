<template>
  <div class="review-page">
    <div class="review-container" v-loading="loading">
      <div class="review-header">
        <el-button link @click="router.back()">
          <el-icon><ArrowLeft /></el-icon> 返回
        </el-button>
        <h2>商品評價</h2>
      </div>

      <el-card v-if="order" class="order-info-card" shadow="never">
        <div class="order-meta">
          <span>訂單編號：{{ order.orderNumber }}</span>
          <el-divider direction="vertical" />
          <span>商家：{{ order.storeName }}</span>
        </div>
        
        <div class="product-list">
          <div v-for="item in order.items" :key="item.id" class="product-item">
            <el-image :src="item.coverImage || '/placeholder.png'" class="product-img" fit="cover" />
            <div class="product-detail">
              <div class="product-name">{{ item.productName }}</div>
              <div class="product-variant">{{ item.variantName }}</div>
            </div>
          </div>
        </div>
      </el-card>

      <el-card class="review-form-card" shadow="never">
        <el-form :model="form" :rules="rules" ref="formRef" label-position="top">
          <el-form-item label="滿意度評分" prop="rating">
            <el-rate
              v-model="form.rating"
              :texts="['極差', '失望', '普通', '滿意', '非常滿意']"
              show-text
              size="large"
            />
          </el-form-item>

          <el-form-item label="評價內容" prop="comment">
            <el-input
              v-model="form.comment"
              type="textarea"
              :rows="5"
              placeholder="分享您的使用體驗，幫助其他買家做出更好的選擇吧！"
              maxlength="500"
              show-word-limit
            />
          </el-form-item>

          <el-form-item label="上傳圖片 (選填)">
            <el-upload
              action="/api/upload"
              list-type="picture-card"
              :on-preview="handlePictureCardPreview"
              :on-remove="handleRemove"
              :on-success="handleUploadSuccess"
              :before-upload="beforeUpload"
              multiple
            >
              <el-icon><Plus /></el-icon>
            </el-upload>
          </el-form-item>

          <div class="form-actions">
            <el-button @click="router.back()">取消</el-button>
            <el-button type="primary" @click="submitReview" :loading="submitting">提交評價</el-button>
          </div>
        </el-form>
      </el-card>

      <el-dialog v-model="dialogVisible">
        <img w-full :src="dialogImageUrl" alt="Preview Image" style="width: 100%" />
      </el-dialog>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, Plus } from '@element-plus/icons-vue';
import { getOrderDetailApi } from '@/api/order';
import type { OrderDetail } from '@/types/order';
import { ElMessage, type FormInstance, type FormRules } from 'element-plus';
import axios from '@/api/axios';

const route = useRoute();
const router = useRouter();
const orderId = Number(route.params.id);

const loading = ref(false);
const submitting = ref(false);
const order = ref<OrderDetail | null>(null);
const formRef = ref<FormInstance>();

const form = ref({
  rating: 5,
  comment: '',
  imageUrls: [] as string[]
});

const rules: FormRules = {
  rating: [{ required: true, message: '請進行評分', trigger: 'change' }],
  comment: [{ required: true, message: '請輸入評價內容', trigger: 'blur' }, { min: 5, message: '內容至少 5 個字', trigger: 'blur' }]
};

const dialogImageUrl = ref('');
const dialogVisible = ref(false);

const fetchOrderDetail = async () => {
  if (isNaN(orderId)) return;
  loading.value = true;
  try {
    const res = await getOrderDetailApi(orderId);
    order.value = res.data;
  } catch (error) {
    ElMessage.error('獲取訂單資訊失敗');
  } finally {
    loading.value = false;
  }
};

const handleRemove = (uploadFile: any) => {
  const url = uploadFile.response?.url || uploadFile.url;
  form.value.imageUrls = form.value.imageUrls.filter(u => u !== url);
};

const handlePictureCardPreview = (uploadFile: any) => {
  dialogImageUrl.value = uploadFile.url!;
  dialogVisible.value = true;
};

const handleUploadSuccess = (response: any) => {
  if (response && response.url) {
    form.value.imageUrls.push(response.url);
  }
};

const beforeUpload = (file: any) => {
  const isJPGorPNG = file.type === 'image/jpeg' || file.type === 'image/png';
  if (!isJPGorPNG) {
    ElMessage.error('只能上傳 JPG/PNG 格式的圖片');
    return false;
  }
  const isLt2M = file.size / 1024 / 1024 < 2;
  if (!isLt2M) {
    ElMessage.error('圖片大小不能超過 2MB');
    return false;
  }
  return true;
};

const submitReview = async () => {
  if (!formRef.value) return;
  await formRef.value.validate(async (valid) => {
    if (valid) {
      submitting.value = true;
      try {
        await axios.post(`/api/front/orders/${orderId}/review`, {
          orderId: orderId,
          rating: form.value.rating,
          comment: form.value.comment,
          imageUrls: form.value.imageUrls,
          createdAt: new Date().toISOString()
        });
        ElMessage.success('評價提交成功！感謝您的分享');
        router.push('/member/orders');
      } catch (error) {
        ElMessage.error('提交失敗，請稍後再試');
      } finally {
        submitting.value = false;
      }
    }
  });
};

onMounted(fetchOrderDetail);
</script>

<style scoped lang="scss">
.review-page {
  background-color: #f5f5f5;
  min-height: calc(100vh - 200px);
  padding: 20px 0;
}

.review-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 0 20px;
}

.review-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 20px;
  
  h2 {
    margin: 0;
    font-size: 20px;
    font-weight: 500;
  }
}

.order-info-card {
  margin-bottom: 20px;
  
  .order-meta {
    font-size: 14px;
    color: #666;
    margin-bottom: 15px;
    padding-bottom: 10px;
    border-bottom: 1px solid #eee;
  }
  
  .product-list {
    .product-item {
      display: flex;
      gap: 15px;
      margin-bottom: 15px;
      &:last-child { margin-bottom: 0; }
      
      .product-img {
        width: 60px;
        height: 60px;
        border-radius: 4px;
        border: 1px solid #eee;
      }
      
      .product-detail {
        .product-name {
          font-size: 14px;
          color: #333;
          margin-bottom: 4px;
        }
        .product-variant {
          font-size: 12px;
          color: #999;
        }
      }
    }
  }
}

.review-form-card {
  .form-actions {
    margin-top: 30px;
    display: flex;
    justify-content: flex-end;
    gap: 12px;
  }
}

:deep(.el-rate__text) {
  font-size: 14px;
  margin-left: 10px;
}
</style>
