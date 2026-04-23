<template>
  <div class="member-level-container" v-loading="loading">
    <!-- 上半部：個人等級概況 -->
    <el-card class="status-card" shadow="hover" v-if="!loading">
      <div class="user-level-info">
        <div class="level-badge-section">
          <div class="level-icon" :style="{ backgroundColor: currentLevelStyles.color }">
            <el-icon :size="40"><Trophy /></el-icon>
          </div>
          <div class="level-names">
            <span class="current-label">目前等級</span>
            <h2 class="level-name">{{ currentLevel.levelName }}</h2>
          </div>
        </div>
        
        <div class="stats-grid">
          <div class="stat-item">
            <span class="stat-label">累積消費金額</span>
            <span class="stat-value">NT$ {{ formatNumber(realTotalSpending) }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">計算區間</span>
            <span class="stat-value text-small">{{ calculationPeriod }}</span>
          </div>
        </div>
      </div>

      <div class="progress-section">
        <div class="progress-header">
          <span>等級進度</span>
          <span v-if="nextLevel" class="next-level-tip">
            再消費 <strong>NT$ {{ formatNumber(neededForNext) }}</strong> 即可升級至 <strong>{{ nextLevel.levelName }}</strong>
          </span>
          <span v-else class="next-level-tip">您已達到最高等級！</span>
        </div>
        <el-progress 
          :percentage="progressPercentage" 
          :stroke-width="16" 
          :format="progressFormat"
          :color="currentLevelStyles.color"
        />
        <div class="progress-footer">
          <span>NT$ {{ formatNumber(realTotalSpending) }}</span>
          <span v-if="nextLevel">NT$ {{ formatNumber(Number(nextLevel.minSpending)) }}</span>
        </div>
      </div>
      
      <div class="update-info" v-if="statsInfo.updatedAt">
        最後更新時間：{{ statsInfo.updatedAt }} (數據每 24 小時同步一次)
      </div>
    </el-card>

    <!-- 下半部：等級說明與權益 -->
    <el-card class="rules-card" shadow="never">
      <template #header>
        <div class="card-header">
          <el-icon><InfoFilled /></el-icon>
          <span>會員等級說明</span>
        </div>
      </template>
      
      <el-table :data="levelRules" style="width: 100%" border stripe>
        <el-table-column prop="levelName" label="等級名稱" width="150" align="center">
          <template #default="scope">
            <span class="custom-level-tag" :style="getLevelTagStyle(scope.row)">
              {{ scope.row.levelName }}
            </span>
          </template>
        </el-table-column>
        <el-table-column prop="minSpending" label="升級門檻 (累積消費)" align="right">
          <template #default="scope">
            NT$ {{ formatNumber(Number(scope.row.minSpending)) }}
          </template>
        </el-table-column>
        <el-table-column prop="discountRate" label="專屬權益" align="center">
          <template #default="scope">
            <!-- 恢復邏輯：只有折扣率小於 1 的才顯示折扣 -->
            <span v-if="Number(scope.row.discountRate) < 1" class="highlight-text">
              {{ (Number(scope.row.discountRate) * 10).toFixed(1) }} 折優惠
            </span>
            <span v-else>—</span>
          </template>
        </el-table-column>
        <el-table-column label="有效期" align="center">
          <template #default>
            永久有效
          </template>
        </el-table-column>
      </el-table>

      <div class="rules-footer">
        <h3>等級計算規則：</h3>
        <ul>
          <li>系統將根據您在過去 12 個月內的「已完成」訂單總額進行計算。</li>
          <li>達成升級門檻後，系統將自動更新您的會員等級。</li>
          <li>若發生退貨導致累積金額低於門檻，系統將保留調整等級之權利。</li>
        </ul>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Trophy, InfoFilled } from '@element-plus/icons-vue'
import { getLevelInfo } from '@/api/member'
import { ElMessage } from 'element-plus'

// 修改 Interface 以符合 API 回傳的 camelCase
interface MembershipLevel {
  id: number;
  levelName: string;
  minSpending: number | string;
  discountRate: number | string;
  color?: string;
}

const loading = ref(true)
const realTotalSpending = ref(0)
const levelRules = ref<MembershipLevel[]>([])

// 模擬等級配色 (對應 ID)
const levelColors: Record<number, string> = {
  1: '#EE4D2D', // 一般 (品牌橘)
  2: '#64748b', // 銀卡 (灰藍)
  3: '#f59e0b'  // 金卡 (琥珀金)
}

const statsInfo = ref({
  startDate: '',
  endDate: '',
  updatedAt: ''
})

const fetchLevelData = async () => {
  try {
    loading.value = true
    const response = await getLevelInfo()
    const data = response.data
    
    realTotalSpending.value = data.totalSpending
    // 修正點：API 回傳的是 id 而非 Id
    levelRules.value = data.levels.map((l: any) => ({
      ...l,
      color: levelColors[l.id] || '#94a3b8'
    }))
    
    const now = new Date()
    const oneYearAgo = new Date()
    oneYearAgo.setFullYear(now.getFullYear() - 1)
    const formatDate = (date: Date) => date.toISOString().split('T')[0]
    
    statsInfo.value = {
      startDate: formatDate(oneYearAgo),
      endDate: formatDate(now),
      updatedAt: new Date().toLocaleString()
    }
  } catch (error) {
    console.error('獲取等級資訊失敗:', error)
    ElMessage.error('無法取得會員等級數據')
  } finally {
    loading.value = false
  }
}

const calculationPeriod = computed(() => {
  return statsInfo.value.startDate ? `${statsInfo.value.startDate} ～ ${statsInfo.value.endDate}` : '載入中...'
})

const currentLevel = computed(() => {
  if (levelRules.value.length === 0) return { levelName: '載入中...', color: '#94a3b8' }
  const spending = Number(realTotalSpending.value)
  const sorted = [...levelRules.value].sort((a, b) => Number(b.minSpending) - Number(a.minSpending))
  return sorted.find(l => spending >= Number(l.minSpending)) || levelRules.value[0]
})

const nextLevel = computed(() => {
  if (levelRules.value.length === 0) return null
  const spending = Number(realTotalSpending.value)
  const sorted = [...levelRules.value].sort((a, b) => Number(a.minSpending) - Number(b.minSpending))
  return sorted.find(l => Number(l.minSpending) > spending)
})

const neededForNext = computed(() => {
  if (!nextLevel.value) return 0
  return Number(nextLevel.value.minSpending) - realTotalSpending.value
})

const progressPercentage = computed(() => {
  if (!nextLevel.value || levelRules.value.length === 0) return 100
  const spending = Number(realTotalSpending.value)
  const currentBase = Number(currentLevel.value.minSpending)
  const nextTarget = Number(nextLevel.value.minSpending)
  
  const progress = ((spending - currentBase) / (nextTarget - currentBase)) * 100
  return Math.min(Math.max(progress, 0), 100)
})

const currentLevelStyles = computed(() => {
  return {
    color: currentLevel.value.color || '#94a3b8'
  }
})

const formatNumber = (num: number) => {
  return Number(num).toLocaleString()
}

const progressFormat = () => {
  return nextLevel.value ? `${progressPercentage.value.toFixed(0)}%` : 'MAX'
}

const getLevelTagStyle = (row: MembershipLevel) => {
  const baseColor = row.color || '#94a3b8'
  return {
    backgroundColor: baseColor + '15',
    color: baseColor,
    border: `1px solid ${baseColor}30`,
    padding: '4px 12px',
    borderRadius: '6px',
    fontSize: '13px',
    fontWeight: '600'
  }
}

onMounted(() => {
  fetchLevelData()
})
</script>

<style scoped>
.member-level-container {
  max-width: 900px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.status-card {
  background: white;
  border-radius: 16px;
  border: 1px solid #f1f5f9;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.user-level-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.level-badge-section {
  display: flex;
  align-items: center;
  gap: 20px;
}

.level-icon {
  color: white;
  width: 72px;
  height: 72px;
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.level-names {
  display: flex;
  flex-direction: column;
}

.current-label {
  font-size: 13px;
  color: #94a3b8;
  margin-bottom: 2px;
}

.level-name {
  font-size: 26px;
  font-weight: 800;
  color: #1e293b;
  margin: 0;
}

.stats-grid {
  display: flex;
  gap: 40px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.stat-label {
  font-size: 13px;
  color: #94a3b8;
  margin-bottom: 6px;
}

.stat-value {
  font-size: 20px;
  font-weight: 700;
  color: #1e293b;
}

.text-small {
  font-size: 13px;
}

.progress-section {
  padding: 10px 0;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  font-size: 14px;
  color: #64748b;
}

.next-level-tip strong {
  color: #ee4d2d;
}

.progress-footer {
  display: flex;
  justify-content: space-between;
  margin-top: 8px;
  font-size: 12px;
  color: #94a3b8;
  font-weight: 600;
}

.update-info {
  margin-top: 20px;
  font-size: 12px;
  color: #94a3b8;
  text-align: right;
  font-style: italic;
}

.rules-card {
  border-radius: 16px;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 17px;
  font-weight: 700;
  color: #1e293b;
}

.custom-level-tag {
  display: inline-block;
  white-space: nowrap;
}

.highlight-text {
  color: #ee4d2d;
  font-weight: 700;
}

.rules-footer {
  margin-top: 30px;
  background-color: #f8fafc;
  padding: 20px;
  border-radius: 12px;
}

.rules-footer h3 {
  font-size: 15px;
  color: #1e293b;
  margin-top: 0;
  margin-bottom: 10px;
}

.rules-footer ul {
  margin: 0;
  padding-left: 20px;
  color: #64748b;
  font-size: 13px;
  line-height: 1.8;
}

:deep(.el-progress-bar__outer) {
  background-color: #f1f5f9;
}
</style>
