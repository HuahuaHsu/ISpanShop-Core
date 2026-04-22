import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { 
  getAddressList, 
  createAddress, 
  updateAddress, 
  deleteAddress, 
  setDefaultAddress 
} from '@/api/member'
import type { AddressDto, CreateAddressDto, UpdateAddressDto } from '@/types/member'
import { ElMessage } from 'element-plus'

export const useAddressStore = defineStore('address', () => {
  const addresses = ref<AddressDto[]>([])
  const loading = ref(false)

  // Getters
  const defaultAddress = computed(() => {
    // 優先尋找明確標記為預設的地址
    const found = addresses.value.find(addr => addr.isDefault === true)
    return found || addresses.value[0] || null
  })

  // Actions
  const fetchAddresses = async () => {
    loading.value = true
    try {
      const res = await getAddressList()
      addresses.value = res.data
    } catch (error) {
      ElMessage.error('獲取地址失敗')
    } finally {
      loading.value = false
    }
  }

  const addAddress = async (form: CreateAddressDto) => {
    try {
      await createAddress(form)
      await fetchAddresses()
      ElMessage.success('新增成功')
      return true
    } catch (error: any) {
      ElMessage.error(error.response?.data?.message || '新增失敗')
      return false
    }
  }

  const editAddress = async (id: number, form: UpdateAddressDto) => {
    try {
      await updateAddress(id, form)
      await fetchAddresses()
      ElMessage.success('更新成功')
      return true
    } catch (error: any) {
      ElMessage.error(error.response?.data?.message || '更新失敗')
      return false
    }
  }

  const removeAddress = async (id: number) => {
    try {
      await deleteAddress(id)
      await fetchAddresses()
      ElMessage.success('刪除成功')
      return true
    } catch (error) {
      ElMessage.error('刪除失敗')
      return false
    }
  }

  const setAsDefault = async (id: number) => {
    try {
      await setDefaultAddress(id)
      await fetchAddresses()
      ElMessage.success('設定預設成功')
      return true
    } catch (error) {
      ElMessage.error('設定失敗')
      return false
    }
  }

  return {
    addresses,
    loading,
    defaultAddress,
    fetchAddresses,
    addAddress,
    editAddress,
    removeAddress,
    setAsDefault
  }
})
