<template>
  <div class="address-view">
    <div class="header">
      <h2>我的收件地址</h2>
      <el-button type="primary" :icon="Plus" @click="handleAdd">新增地址</el-button>
    </div>

    <el-skeleton :loading="addressStore.loading" animated :count="3">
      <template #default>
        <div v-if="addressStore.addresses.length === 0" class="empty-state">
          <el-empty description="目前沒有收件地址" />
        </div>
        
        <div v-else class="address-grid">
          <el-row :gutter="20">
            <el-col 
              v-for="addr in addressStore.addresses" 
              :key="addr.id" 
              :xs="24" :sm="12" :lg="8"
            >
              <AddressCard 
                :address="addr" 
                @edit="handleEdit"
                @delete="addressStore.removeAddress"
                @set-default="addressStore.setAsDefault"
              />
            </el-col>
          </el-row>
        </div>
      </template>
    </el-skeleton>

    <!-- 地址表單彈窗 -->
    <AddressFormDialog
      v-model="dialogVisible"
      :edit-data="editingAddress"
      :loading="submitting"
      @submit="handleSubmit"
      @close="editingAddress = null"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Plus } from '@element-plus/icons-vue'
import AddressCard from '@/components/member/AddressCard.vue'
import AddressFormDialog from '@/components/member/AddressFormDialog.vue'
import { useAddressStore } from '@/stores/address'
import type { AddressDto, UpdateAddressDto } from '@/types/member'

const addressStore = useAddressStore()
const dialogVisible = ref(false)
const editingAddress = ref<AddressDto | null>(null)
const submitting = ref(false)

const handleAdd = () => {
  editingAddress.value = null
  dialogVisible.value = true
}

const handleEdit = (addr: AddressDto) => {
  editingAddress.value = addr
  dialogVisible.value = true
}

const handleSubmit = async (form: UpdateAddressDto) => {
  submitting.value = true
  let success = false
  if (editingAddress.value) {
    success = await addressStore.editAddress(form.id, form)
  } else {
    success = await addressStore.addAddress(form)
  }
  
  if (success) {
    dialogVisible.value = false
  }
  submitting.value = false
}

onMounted(() => {
  addressStore.fetchAddresses()
})
</script>

<style scoped>
.address-view {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.header h2 {
  margin: 0;
  font-size: 22px;
  color: #1e293b;
}

.empty-state {
  padding: 80px 0;
  background: #f8fafc;
  border-radius: 12px;
}

@media (max-width: 640px) {
  .header {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }
}
</style>
