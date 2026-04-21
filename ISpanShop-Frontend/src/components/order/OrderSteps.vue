<template>
  <div class="order-steps-wrapper">
    <el-steps :active="activeStep" align-center finish-status="success">
      <el-step 
        v-for="(step, index) in orderSteps" 
        :key="index"
        :title="step.title" 
        :description="step.description"
      ></el-step>
    </el-steps>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  status: number | undefined;
  createdAt: string | null | undefined;
  paymentDate: string | null | undefined;
  completedAt: string | null | undefined;
}

const props = defineProps<Props>();

const formatDate = (dateStr?: string | null) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleString('zh-TW', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
};

// 根據訂單狀態動態生成進度條步驟
const orderSteps = computed(() => {
  const status = props.status;
  
  // 情況四：已取消
  if (status === 4) {
    return [
      { title: '訂單已成立', description: formatDate(props.createdAt) },
      { title: '已取消', description: formatDate(props.completedAt || props.paymentDate) }
    ];
  }
  
  // 情況三：退貨/退款中 或 已退款
  if (status === 5 || status === 6) {
    return [
      { title: '訂單已成立', description: formatDate(props.createdAt) },
      { title: '付款成功', description: formatDate(props.paymentDate) },
      { title: '退貨/款中' },
      { title: '已退款', description: formatDate(props.completedAt) }
    ];
  }
  
  // 情況二：運送中 (未完成前)
  if (status === 2) {
    return [
      { title: '訂單已成立', description: formatDate(props.createdAt) },
      { title: '付款成功', description: formatDate(props.paymentDate) },
      { title: '待出貨' },
      { title: '運送中' }
    ];
  }
  
  // 情況一：預設情況 (待付款、待出貨、已完成)
  return [
    { title: '訂單已成立', description: formatDate(props.createdAt) },
    { title: '付款成功', description: formatDate(props.paymentDate) },
    { title: '待出貨' },
    { title: '訂單已完成', description: formatDate(props.completedAt) }
  ];
});

// 計算當前活耀的步驟索引
const activeStep = computed(() => {
  const status = props.status;
  
  switch (status) {
    case 0: return 1; // 待付款 -> 停在「訂單已成立」之後
    case 1: return 2; // 待出貨 -> 「付款成功」已完成
    case 2: return 3; // 運送中 -> 「待出貨」已完成
    case 3: return 4; // 已完成 -> 全部完成
    case 4: return 1; // 已取消 -> 「訂單已成立」已完成
    case 5: return 2; // 退貨中 -> 「付款成功」已完成
    case 6: return 4; // 已退款 -> 全部完成
    default: return 0;
  }
});
</script>

<style scoped>
.order-steps-wrapper {
  padding: 20px 0;
}
</style>
