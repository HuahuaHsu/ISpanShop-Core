<template>
  <el-card 
    class="address-card" 
    :class="{ 
      'is-default': !selectable && address.isDefault,
      'is-selected': selected,
      'is-selectable': selectable
    }"
    shadow="hover"
    @click="handleCardClick"
  >
    <div class="card-header">
      <div class="header-left">
        <el-radio v-if="selectable" :model-value="selected ? address.id : null" :label="address.id">
          <span class="recipient">{{ address.recipientName }}</span>
        </el-radio>
        <span v-else class="recipient">{{ address.recipientName }}</span>
        <el-tag v-if="address.isDefault" size="small" type="danger" effect="plain" class="ml-2">預設</el-tag>
      </div>
    </div>
    
    <div class="card-body">
      <p class="phone"><el-icon><Phone /></el-icon> {{ address.recipientPhone }}</p>
      <p class="location">
        <el-icon><Location /></el-icon>
        {{ address.city }}{{ address.region }}{{ address.street }}
      </p>
    </div>
    
    <div v-if="!selectable" class="card-footer">
      <el-button link type="primary" @click.stop="$emit('edit', address)">編輯</el-button>
      <el-popconfirm title="確定要刪除此地址嗎？" @confirm="$emit('delete', address.id)">
        <template #reference>
          <el-button link type="danger" @click.stop>刪除</el-button>
        </template>
      </el-popconfirm>
      <el-button 
        v-if="!address.isDefault" 
        link 
        type="info" 
        @click.stop="$emit('set-default', address.id)"
      >
        設為預設
      </el-button>
    </div>
  </el-card>
</template>

<script setup lang="ts">
import { Phone, Location } from '@element-plus/icons-vue'
import type { AddressDto } from '@/types/member'

const props = defineProps<{
  address: AddressDto
  selectable?: boolean
  selected?: boolean
}>()

const emit = defineEmits(['edit', 'delete', 'set-default', 'select'])

const handleCardClick = () => {
  if (props.selectable) {
    emit('select', props.address)
  }
}
</script>

<style scoped>
.address-card {
  margin-bottom: 20px;
  border-radius: 8px;
  transition: all 0.3s;
  cursor: default;
}

.is-selectable {
  cursor: pointer;
}

.address-card.is-default {
  border: 1px solid #ffccc7;
  background-color: #fff1f0;
}

.address-card.is-selected {
  border: 2px solid #ee4d2d;
  background-color: #fff5f2;
}

.card-header {
  margin-bottom: 12px;
}

.header-left {
  display: flex;
  align-items: center;
}

.recipient {
  font-weight: bold;
  font-size: 16px;
  color: #303133;
}

.card-body p {
  margin: 8px 0;
  font-size: 14px;
  color: #606266;
  display: flex;
  align-items: center;
  gap: 8px;
}

.card-footer {
  margin-top: 15px;
  padding-top: 10px;
  border-top: 1px solid #ebeef5;
  display: flex;
  justify-content: flex-end;
}

.ml-2 { margin-left: 8px; }

:deep(.el-radio__label) {
  padding-left: 8px;
}
</style>
