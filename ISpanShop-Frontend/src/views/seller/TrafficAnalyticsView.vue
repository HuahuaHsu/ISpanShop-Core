<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getTrafficAnalyticsApi } from '@/api/seller'
import { ElMessage } from 'element-plus'
import { View, ShoppingCart, PieChart, TrendCharts, QuestionFilled } from '@element-plus/icons-vue'

const router = useRouter()

interface TrafficSummary {
  totalViews: number
  topItemsTrafficShare: number
  avgConversionRate: number
}

interface TopProduct {
  productId: number
  productName: string
  productImage: string
  viewCount: number
  totalSales: number
  conversionRate: number
}

interface CategoryTraffic {
  categoryName: string
  viewCount: number
  percentage: number
}

const loading = ref(true)
const summary = ref<TrafficSummary>({ totalViews: 0, topItemsTrafficShare: 0, avgConversionRate: 0 })
const topProducts = ref<TopProduct[]>([])
const categoryData = ref<CategoryTraffic[]>([])

const fetchData = async () => {
  try {
    loading.value = true
    const res = await getTrafficAnalyticsApi()
    summary.value = res.data.summary
    topProducts.value = res.data.topProducts
    categoryData.value = res.data.categoryData
  } catch (error) {
    console.error('獲取流量分析失敗:', error)
    ElMessage.error('獲取數據失敗')
  } finally {
    loading.value = false
  }
}

function formatNumber(num: number): string {
  return num.toLocaleString()
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <div class="analytics-container">
    <div class="page-header">
      <h2 class="title">流量分析</h2>
      <p class="subtitle">分析賣場商品的瀏覽趨勢與轉換成效</p>
    </div>

    <!-- KPI 卡片 -->
    <el-row :gutter="20" class="mb-6">
      <el-col :span="8">
        <el-card shadow="hover" class="kpi-card">
          <div class="kpi-content">
            <div class="kpi-icon views">
              <el-icon><View /></el-icon>
            </div>
            <div class="kpi-info">
              <div class="kpi-label">全店總瀏覽量</div>
              <div class="kpi-value">{{ formatNumber(summary.totalViews) }}</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover" class="kpi-card">
          <div class="kpi-content">
            <div class="kpi-icon concentration">
              <el-icon><TrendCharts /></el-icon>
            </div>
            <div class="kpi-info">
              <div class="kpi-label">
                流量集中度 (前三名佔比)
                <el-tooltip content="計算前三名熱門商品佔全店總流量的比例。若比例過高，代表流量過於依賴少數商品。" placement="top">
                  <el-icon class="info-icon"><QuestionFilled /></el-icon>
                </el-tooltip>
              </div>
              <div class="kpi-value">{{ summary.topItemsTrafficShare }}%</div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover" class="kpi-card">
          <div class="kpi-content">
            <div class="kpi-icon conversion">
              <el-icon><ShoppingCart /></el-icon>
            </div>
            <div class="kpi-info">
              <div class="kpi-label">
                平均全店轉換率
                <el-tooltip content="全店總銷量 / 總瀏覽次數。反映賣場商品的吸引力與下單意願。" placement="top">
                  <el-icon class="info-icon"><QuestionFilled /></el-icon>
                </el-tooltip>
              </div>
              <div class="kpi-value">{{ summary.avgConversionRate }}%</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <!-- 商品流量排行榜 -->
      <el-col :span="16">
        <el-card shadow="never" header="商品流量 Top 10" class="rank-card">
          <el-table :data="topProducts" stripe v-loading="loading">
            <el-table-column label="排行" width="60" align="center">
              <template #default="scope">
                <span :class="['rank-num', { 'top-three': scope.$index < 3 }]">{{ scope.$index + 1 }}</span>
              </template>
            </el-table-column>
            <el-table-column label="商品資訊" min-width="250">
              <template #default="{ row }">
                <div class="product-info clickable" @click="router.push(`/product/${row.productId}`)">
                  <el-image :src="row.productImage" fit="cover" class="product-img">
                    <template #error>
                      <div class="image-placeholder">🖼️</div>
                    </template>
                  </el-image>
                  <span class="product-name">{{ row.productName }}</span>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="viewCount" label="瀏覽次數" width="120" sortable align="right">
              <template #default="{ row }">
                {{ formatNumber(row.viewCount) }}
              </template>
            </el-table-column>
            <el-table-column prop="conversionRate" label="轉換率" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="row.conversionRate > summary.avgConversionRate ? 'success' : 'info'" size="small">
                  {{ row.conversionRate }}%
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>

      <!-- 分類分佈 -->
      <el-col :span="8">
        <el-card shadow="never" header="分類流量佔比" class="chart-card">
          <div class="category-list" v-loading="loading">
            <div v-if="categoryData.length === 0" class="empty-text">尚無數據</div>
            <div v-for="cat in categoryData" :key="cat.categoryName" class="category-item">
              <div class="cat-header">
                <span class="cat-name">{{ cat.categoryName }}</span>
                <span class="cat-percent">{{ cat.percentage }}%</span>
              </div>
              <el-progress 
                :percentage="cat.percentage" 
                :show-text="false"
                :stroke-width="10"
                :color="cat.percentage > 30 ? '#409EFF' : '#95d475'"
              />
              <div class="cat-footer">
                瀏覽數: {{ formatNumber(cat.viewCount) }}
              </div>
            </div>
          </div>
        </el-card>

        <el-card shadow="never" class="insight-card mt-4">
          <template #header>
            <div class="insight-header">
              <el-icon><PieChart /></el-icon>
              <span>數據洞察</span>
            </div>
          </template>
          <div class="insight-content">
            <p v-if="summary.topItemsTrafficShare > 60">
              ⚠️ 您的流量過於集中在少數商品，建議增加其他商品的曝光路徑。
            </p>
            <p v-else>
              ✅ 您的流量分佈相對健康。
            </p>
            <p v-if="summary.avgConversionRate < 2">
              💡 店內整體轉換率偏低，建議檢查商品描述或定價策略。
            </p>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.analytics-container {
  padding: 24px;
}
.page-header {
  margin-bottom: 24px;
}
.title {
  font-size: 24px;
  font-weight: 600;
  margin: 0;
  color: #303133;
}
.subtitle {
  font-size: 14px;
  color: #909399;
  margin-top: 8px;
}
.mb-6 { margin-bottom: 24px; }
.mt-4 { margin-top: 16px; }

.kpi-card {
  border-radius: 12px;
  transition: all 0.3s;
}
.kpi-content {
  display: flex;
  align-items: center;
  gap: 16px;
}
.kpi-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
}
.kpi-icon.views { background: #ecf5ff; color: #409eff; }
.kpi-icon.concentration { background: #fdf6ec; color: #e6a23c; }
.kpi-icon.conversion { background: #f0f9eb; color: #67c23a; }

.kpi-label { 
  font-size: 14px; 
  color: #606266; 
  margin-bottom: 4px; 
  display: flex;
  align-items: center;
  gap: 4px;
}
.info-icon {
  font-size: 14px;
  color: #c0c4cc;
  cursor: help;
}
.kpi-value { font-size: 24px; font-weight: 700; color: #303133; }

.rank-card :deep(.el-card__header) { font-weight: 600; }
.rank-num {
  width: 24px;
  height: 24px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  background: #f5f7fa;
  font-size: 12px;
  font-weight: 700;
  color: #909399;
}
.rank-num.top-three {
  background: #EE4D2D;
  color: white;
}

.product-info {
  display: flex;
  align-items: center;
  gap: 12px;
}
.product-info.clickable {
  cursor: pointer;
  transition: opacity 0.2s;
}
.product-info.clickable:hover {
  opacity: 0.8;
}
.product-img {
  width: 40px;
  height: 40px;
  border-radius: 4px;
  background: #f5f7fa;
}
.product-name {
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.category-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}
.category-item { margin-bottom: 4px; }
.cat-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 14px;
}
.cat-name { font-weight: 500; color: #303133; }
.cat-percent { color: #409eff; font-weight: 600; }
.cat-footer {
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
  text-align: right;
}

.insight-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #e6a23c;
}
.insight-content {
  font-size: 13px;
  color: #606266;
  line-height: 1.6;
}
.insight-content p { margin: 0 0 8px; }

.empty-text {
  padding: 40px 0;
  text-align: center;
  color: #c0c4cc;
}
</style>
