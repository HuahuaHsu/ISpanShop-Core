<script setup lang="ts">
interface Props {
  discountAmount?: number | null
  levelDiscount?: number | null
  pointDiscount?: number | null
  promotionDiscount?: number | null
  horizontal?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  horizontal: false
})
</script>

<template>
  <div class="discount-tags" :class="{ 'is-horizontal': horizontal }">
    <el-tag 
      v-if="discountAmount && discountAmount > 0" 
      size="small" 
      type="danger" 
      effect="plain" 
      class="tag-item"
    >
      優惠券折抵
    </el-tag>
    <el-tag 
      v-if="levelDiscount && levelDiscount > 0" 
      size="small" 
      type="warning" 
      effect="plain" 
      class="tag-item"
    >
      會員折扣
    </el-tag>
    <el-tag 
      v-if="pointDiscount && pointDiscount > 0" 
      size="small" 
      type="success" 
      effect="plain" 
      class="tag-item"
    >
      點數折抵
    </el-tag>
    <el-tag 
      v-if="promotionDiscount && promotionDiscount > 0" 
      size="small" 
      type="primary" 
      effect="plain" 
      class="tag-item"
    >
      活動促銷
    </el-tag>
  </div>
</template>

<style scoped>
.discount-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}
.discount-tags.is-horizontal {
  display: inline-flex;
  vertical-align: middle;
  margin-left: 8px;
}
.tag-item {
  border-radius: 4px;
  padding: 0 8px;
  font-size: 11px;
  height: 20px;
  line-height: 18px;
}
</style>
