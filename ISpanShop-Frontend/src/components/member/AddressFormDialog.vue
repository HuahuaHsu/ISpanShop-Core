<template>
  <el-dialog
    v-model="visible"
    :title="isEdit ? '編輯地址' : '新增地址'"
    width="550px"
    @close="handleClose"
    destroy-on-close
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="100px"
      label-position="top"
    >
      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="收件人姓名" prop="recipientName">
            <el-input v-model="form.recipientName" placeholder="請輸入收件人姓名" />
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="收件人電話" prop="recipientPhone">
            <el-input v-model="form.recipientPhone" placeholder="請輸入手機號碼" />
          </el-form-item>
        </el-col>
      </el-row>

      <el-row :gutter="20">
        <el-col :span="12">
          <el-form-item label="縣市" prop="city">
            <el-select v-model="form.city" placeholder="選擇縣市" @change="form.region = ''" class="w-full">
              <el-option v-for="c in cities" :key="c" :label="c" :value="c" />
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :span="12">
          <el-form-item label="行政區" prop="region">
            <el-input v-model="form.region" placeholder="如：大安區" />
          </el-form-item>
        </el-col>
      </el-row>

      <el-form-item label="詳細地址" prop="street">
        <el-input
          v-model="form.street"
          type="textarea"
          :rows="2"
          placeholder="街道名稱、門牌號碼、樓層"
        />
      </el-form-item>

      <el-form-item prop="isDefault">
        <el-checkbox v-model="form.isDefault">設為預設地址</el-checkbox>
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="visible = false">取消</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">
          確定
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, watch, reactive, computed } from 'vue'
import type { FormInstance } from 'element-plus'
import type { AddressDto, UpdateAddressDto } from '@/types/member'

const props = defineProps<{
  modelValue: boolean
  editData?: AddressDto | null
  loading?: boolean
}>()

const emit = defineEmits(['update:modelValue', 'submit', 'close'])

const visible = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const isEdit = computed(() => !!props.editData)
const formRef = ref<FormInstance>()

const cities = [
  '台北市', '新北市', '桃園市', '台中市', '台南市', '高雄市',
  '基隆市', '新竹市', '嘉義市', '新竹縣', '苗栗縣', '彰化縣',
  '南投縣', '雲林縣', '嘉義縣', '屏東縣', '宜蘭縣', '花蓮縣',
  '台東縣', '澎湖縣', '金門縣', '連江縣'
]

const form = reactive<UpdateAddressDto>({
  id: 0,
  recipientName: '',
  recipientPhone: '',
  city: '',
  region: '',
  street: '',
  isDefault: false
})

const rules = {
  recipientName: [{ required: true, message: '請輸入收件人姓名', trigger: 'blur' }],
  recipientPhone: [
    { required: true, message: '請輸入手機號碼', trigger: 'blur' },
    { pattern: /^09\d{8}$/, message: '請輸入正確的手機格式 (09xxxxxxxx)', trigger: 'blur' }
  ],
  city: [{ required: true, message: '請選擇縣市', trigger: 'change' }],
  region: [{ required: true, message: '請輸入行政區', trigger: 'blur' }],
  street: [{ required: true, message: '請輸入詳細地址', trigger: 'blur' }]
}

const resetForm = () => {
  Object.assign(form, {
    id: 0,
    recipientName: '',
    recipientPhone: '',
    city: '',
    region: '',
    street: '',
    isDefault: false
  })
}

watch(() => props.editData, (newVal) => {
  if (newVal) {
    Object.assign(form, { ...newVal })
  } else {
    resetForm()
  }
}, { immediate: true })

const handleClose = () => {
  emit('close')
  if (formRef.value) formRef.value.resetFields()
}

const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate((valid) => {
    if (valid) {
      emit('submit', { ...form })
    }
  })
}

defineExpose({
  resetForm
})
</script>

<style scoped>
.w-full { width: 100%; }
</style>
