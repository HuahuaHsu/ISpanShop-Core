<template>
  <div class="filter-sidebar">

    <!-- ① 目前分類 -->
    <div class="sb-block">
      <div class="sb-title">目前分類</div>
      <div class="sb-current-cat">
        <span class="sb-cat-name">{{ categoryName }}</span>
        <el-button link type="danger" size="small" @click="$emit('clear')">
          <el-icon><Close /></el-icon> 清除
        </el-button>
      </div>
    </div>

    <!-- ② 子分類 -->
    <div v-if="subLoading || subCategories.length > 0" class="sb-block">
      <div class="sb-title">子分類</div>
      <div v-if="subLoading" class="skel-list">
        <el-skeleton v-for="n in 4" :key="n" animated>
          <template #template>
            <el-skeleton-item variant="p" style="width: 72%; margin-bottom: 10px;" />
          </template>
        </el-skeleton>
      </div>
      <el-radio-group
        v-else
        v-model="subCatModel"
        class="sb-radio-group"
        @change="onSubChange"
      >
        <el-radio :value="-1" class="sb-radio">全部</el-radio>
        <el-radio
          v-for="sub in subCategories"
          :key="sub.id"
          :value="sub.id"
          class="sb-radio"
        >{{ sub.name }}</el-radio>
      </el-radio-group>
    </div>

    <!-- ③ 品牌 -->
    <div v-if="brandLoading || brands.length > 0" class="sb-block">
      <div class="sb-title">品牌</div>
      <div v-if="brandLoading" class="skel-list">
        <el-skeleton v-for="n in 5" :key="n" animated>
          <template #template>
            <el-skeleton-item variant="p" style="width: 80%; margin-bottom: 10px;" />
          </template>
        </el-skeleton>
      </div>
      <template v-else>
        <el-input
          :model-value="brandKeyword"
          placeholder="搜尋品牌"
          size="small"
          clearable
          class="sb-brand-search"
          @update:model-value="$emit('update:brandKeyword', $event as string)"
        />
        <el-checkbox-group
          :model-value="selectedBrandIds"
          class="sb-checkbox-group"
          @update:model-value="onBrandChange"
        >
          <el-checkbox
            v-for="brand in visibleBrands"
            :key="brand.id"
            :value="brand.id"
            class="sb-checkbox"
          >
            {{ brand.name }}<span class="brand-count"> ({{ brand.productCount }})</span>
          </el-checkbox>
        </el-checkbox-group>
        <el-button
          v-if="filteredBrands.length > BRAND_LIMIT"
          text
          size="small"
          class="sb-more-btn"
          @click="toggleBrandExpand"
        >
          {{ isBrandExpanded ? '收起' : `顯示更多 (${filteredBrands.length - BRAND_LIMIT})` }}
        </el-button>
      </template>
    </div>

    <!-- ④ 已套用篩選 -->
    <div v-if="appliedFilters.length > 0" class="sb-block">
      <div class="sb-title">已套用篩選</div>
      <div class="applied-tags">
        <el-tag
          v-for="f in appliedFilters"
          :key="f.key"
          closable
          size="small"
          type="info"
          class="applied-tag"
          @close="removeFilter(f)"
        >{{ f.label }}</el-tag>
      </div>
      <el-button text type="danger" size="small" class="sb-clear-btn" @click="clearAll">
        清除全部
      </el-button>
    </div>

  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Close } from '@element-plus/icons-vue'
import type { SubCategory } from '@/types/category'
import type { Brand } from '@/types/brand'

const BRAND_LIMIT = 10

interface AppliedFilter {
  key: string
  label: string
  type: 'sub' | 'brand'
  id: number
}

const props = defineProps<{
  categoryName: string
  subCategories: SubCategory[]
  brands: Brand[]
  subLoading: boolean
  brandLoading: boolean
  selectedSubCategoryId: number | null
  selectedBrandIds: number[]
  brandKeyword: string
  isBrandExpanded: boolean
}>()

const emit = defineEmits<{
  clear: []
  'filter-change': []
  'update:selectedSubCategoryId': [value: number | null]
  'update:selectedBrandIds': [value: number[]]
  'update:brandKeyword': [value: string]
  'update:isBrandExpanded': [value: boolean]
}>()

// null → -1 作為 el-radio-group 的「全部」sentinel
const subCatModel = computed<number>({
  get: () => props.selectedSubCategoryId ?? -1,
  set: (val: number) => {
    emit('update:selectedSubCategoryId', val === -1 ? null : val)
  },
})

function onSubChange(): void {
  emit('filter-change')
}

function onBrandChange(val: (string | number | boolean)[]): void {
  const ids = val.filter((v): v is number => typeof v === 'number')
  emit('update:selectedBrandIds', ids)
  emit('filter-change')
}

function toggleBrandExpand(): void {
  emit('update:isBrandExpanded', !props.isBrandExpanded)
}

const filteredBrands = computed<Brand[]>(() => {
  const kw = props.brandKeyword.trim().toLowerCase()
  if (!kw) return props.brands
  return props.brands.filter(b => b.name.toLowerCase().includes(kw))
})

const visibleBrands = computed<Brand[]>(() =>
  props.isBrandExpanded
    ? filteredBrands.value
    : filteredBrands.value.slice(0, BRAND_LIMIT),
)

const appliedFilters = computed<AppliedFilter[]>(() => {
  const result: AppliedFilter[] = []
  if (props.selectedSubCategoryId !== null) {
    const sub = props.subCategories.find(s => s.id === props.selectedSubCategoryId)
    if (sub) {
      result.push({ key: `sub-${sub.id}`, label: sub.name, type: 'sub', id: sub.id })
    }
  }
  for (const bId of props.selectedBrandIds) {
    const brand = props.brands.find(b => b.id === bId)
    if (brand) {
      result.push({ key: `brand-${brand.id}`, label: brand.name, type: 'brand', id: brand.id })
    }
  }
  return result
})

function removeFilter(f: AppliedFilter): void {
  if (f.type === 'sub') {
    emit('update:selectedSubCategoryId', null)
  } else {
    emit('update:selectedBrandIds', props.selectedBrandIds.filter(id => id !== f.id))
  }
  emit('filter-change')
}

function clearAll(): void {
  emit('update:selectedSubCategoryId', null)
  emit('update:selectedBrandIds', [])
  emit('filter-change')
}
</script>

<style scoped>
.filter-sidebar {
  background: white;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.sb-block {
  padding: 16px;
  border-bottom: 1px solid #f1f5f9;
}
.sb-block:last-child {
  border-bottom: none;
}

.sb-title {
  font-size: 13px;
  font-weight: 700;
  color: #475569;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 12px;
}

/* 目前分類 */
.sb-current-cat {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}
.sb-cat-name {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
}

/* 子分類 radio */
.sb-radio-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.sb-radio {
  height: auto;
  line-height: 1.4;
  margin-right: 0;
}
:deep(.sb-radio .el-radio__label) {
  font-size: 14px;
  color: #334155;
}

/* 品牌 */
.sb-brand-search {
  margin-bottom: 10px;
}
.sb-checkbox-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}
.sb-checkbox {
  height: auto;
  line-height: 1.4;
  margin-right: 0;
  align-items: flex-start;
}
:deep(.sb-checkbox .el-checkbox__label) {
  font-size: 14px;
  color: #334155;
  white-space: normal;
  line-height: 1.4;
}
.brand-count {
  font-size: 11px;
  color: #94a3b8;
}
.sb-more-btn {
  margin-top: 6px;
  padding: 0;
  font-size: 12px;
}

/* 已套用篩選 */
.applied-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 8px;
}
.applied-tag {
  max-width: 160px;
  overflow: hidden;
  text-overflow: ellipsis;
}
.sb-clear-btn {
  padding: 0;
  font-size: 12px;
}

/* 骨架屏 */
.skel-list {
  display: flex;
  flex-direction: column;
}
</style>
