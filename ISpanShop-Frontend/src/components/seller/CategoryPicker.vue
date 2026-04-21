<template>
  <el-dialog
    v-model="dialogVisible"
    title="選擇分類"
    width="720px"
    :close-on-click-modal="false"
  >
    <!-- 搜尋框 -->
    <div class="search-box">
      <el-input
        v-model="searchKeyword"
        placeholder="請輸入至少 1 個字"
        clearable
        prefix-icon="Search"
      />
    </div>

    <!-- 動態層級級聯選擇 -->
    <div class="category-cascade">
      <div
        v-for="(col, level) in columns"
        :key="level"
        class="category-column"
      >
        <div class="column-header">{{ getColumnTitle(level) }}</div>
        <div class="category-list">
          <div
            v-for="cat in filteredColumn(col, level)"
            :key="cat.id"
            class="category-item"
            :class="{ active: selectedPath[level]?.id === cat.id }"
            @click="selectCategory(cat, level)"
          >
            {{ cat.name }}
            <el-icon v-if="hasChildren(cat.id)" class="arrow-right">
              <ArrowRight />
            </el-icon>
          </div>
          <el-empty
            v-if="filteredColumn(col, level).length === 0"
            description="無符合結果"
            :image-size="60"
          />
        </div>
      </div>

      <!-- 空狀態提示 -->
      <div v-if="columns.length === 0" class="category-column">
        <el-empty description="載入中..." :image-size="60" />
      </div>
    </div>

    <!-- 已選擇路徑 -->
    <div v-if="selectedPathString" class="selected-path">
      <span class="path-label">目前已選擇的:</span>
      <span class="path-value">{{ selectedPathString }}</span>
    </div>

    <template #footer>
      <el-button @click="handleCancel">取消</el-button>
      <el-button
        type="primary"
        :disabled="!canConfirm"
        @click="handleConfirm"
      >
        確認
      </el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { ArrowRight } from '@element-plus/icons-vue'
import { fetchMainCategories, fetchChildCategories } from '@/api/category'
import type { Category, SubCategory } from '@/types/category'

interface CategoryNode {
  id: number
  name: string
  parentId: number | null
}

const props = defineProps<{
  modelValue: boolean
  initialCategoryId?: number | null
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  confirm: [categoryId: number, categoryPath: string]
}>()

const dialogVisible = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
})

const searchKeyword = ref<string>('')
const categories = ref<CategoryNode[]>([])
const columns = ref<CategoryNode[][]>([])
const selectedPath = ref<CategoryNode[]>([])

const selectedPathString = computed<string>(() =>
  selectedPath.value.map((c) => c.name).join(' > ')
)

const canConfirm = computed<boolean>(() => {
  if (selectedPath.value.length === 0) return false
  const lastSelected = selectedPath.value[selectedPath.value.length - 1]
  return !hasChildren(lastSelected.id)
})

function getColumnTitle(level: number): string {
  const titles = ['大分類', '中分類', '小分類']
  return level < titles.length ? titles[level] : `子分類 ${level + 1}`
}

function filteredColumn(col: CategoryNode[], level: number): CategoryNode[] {
  if (!searchKeyword.value.trim() || level > 0) return col
  return col.filter((c) =>
    c.name.toLowerCase().includes(searchKeyword.value.toLowerCase())
  )
}

function hasChildren(categoryId: number): boolean {
  return categories.value.some((c) => c.parentId === categoryId)
}

async function selectCategory(category: CategoryNode, level: number): Promise<void> {
  selectedPath.value = selectedPath.value.slice(0, level)
  selectedPath.value.push(category)
  
  columns.value = columns.value.slice(0, level + 1)
  
  const children = categories.value.filter((c) => c.parentId === category.id)
  
  if (children.length === 0) {
    await loadChildren(category.id, level + 1)
  } else {
    columns.value.push(children)
  }
}

async function loadChildren(parentId: number, level: number): Promise<void> {
  try {
    const res = await fetchChildCategories(parentId)
    if (res.success && res.data.length > 0) {
      const newCategories = res.data.map((cat: SubCategory) => ({
        id: cat.id,
        name: cat.name,
        parentId,
      }))
      
      categories.value = [
        ...categories.value.filter((c) => c.parentId !== parentId),
        ...newCategories,
      ]
      
      columns.value.push(newCategories)
    }
  } catch (error) {
    console.error('載入子分類失敗', error)
  }
}

async function loadInitialCategories(): Promise<void> {
  try {
    const res = await fetchMainCategories()
    if (res.success) {
      categories.value = res.data.map((cat: Category) => ({
        id: cat.id,
        name: cat.name,
        parentId: null,
      }))
      
      const level1 = categories.value.filter((c) => c.parentId === null || c.parentId === 0)
      columns.value = [level1]
      
      console.log('已載入分類資料：', {
        總數: categories.value.length,
        第一層數量: level1.length,
        範例: categories.value.slice(0, 3),
      })
    }
  } catch (error) {
    ElMessage.error('載入分類失敗')
    console.error(error)
  }
}

function handleConfirm(): void {
  if (!canConfirm.value) {
    ElMessage.warning('請選擇完整的分類路徑（需選到最後一層）')
    return
  }
  
  const lastCategory = selectedPath.value[selectedPath.value.length - 1]
  emit('confirm', lastCategory.id, selectedPathString.value)
  emit('update:modelValue', false)
}

function handleCancel(): void {
  emit('update:modelValue', false)
}

function resetSelection(): void {
  selectedPath.value = []
  searchKeyword.value = ''
}

onMounted(async () => {
  await loadInitialCategories()
})

watch(dialogVisible, (visible) => {
  if (visible) {
    resetSelection()
  }
})
</script>

<style scoped>
.search-box {
  margin-bottom: 16px;
}

.category-cascade {
  display: flex;
  gap: 1px;
  background: #f0f0f0;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
  min-height: 320px;
}

.category-column {
  flex: 1;
  background: #fff;
  display: flex;
  flex-direction: column;
}

.column-header {
  padding: 12px 16px;
  background: #f7f8fa;
  border-bottom: 1px solid #e0e0e0;
  font-weight: 600;
  font-size: 14px;
  color: #333;
}

.category-list {
  flex: 1;
  overflow-y: auto;
  max-height: 380px;
}

.category-item {
  padding: 12px 16px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 14px;
  color: #333;
  transition: all 0.2s;
  border-bottom: 1px solid #f5f5f5;
}

.category-item:hover {
  background: #fff5f0;
  color: #ee4d2d;
}

.category-item.active {
  background: #fff5f0;
  color: #ee4d2d;
  font-weight: 600;
  border-left: 3px solid #ee4d2d;
}

.arrow-right {
  font-size: 14px;
  color: #999;
}

.category-item.active .arrow-right {
  color: #ee4d2d;
}

.selected-path {
  margin-top: 16px;
  padding: 12px;
  background: #f7f8fa;
  border-radius: 4px;
  font-size: 13px;
}

.path-label {
  color: #666;
  margin-right: 8px;
}

.path-value {
  color: #ee4d2d;
  font-weight: 600;
}
</style>
