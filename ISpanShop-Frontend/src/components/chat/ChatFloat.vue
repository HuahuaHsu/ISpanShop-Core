<template>
  <div class="chat-float" v-if="authStore.isLoggedIn">
    <!-- 懸浮按鈕 -->
    <div class="chat-trigger" @click="toggleChat" :class="{ 'has-unread': totalUnread > 0 }">
      <el-badge :value="totalUnread" :hidden="totalUnread === 0" class="unread-badge">
        <el-icon :size="28"><ChatDotRound /></el-icon>
      </el-badge>
      <span class="trigger-text">好聊</span>
    </div>

    <!-- 蝦皮式雙欄聊天視窗 -->
    <transition name="el-zoom-in-bottom">
      <div class="chat-container" v-if="chatStore.isChatOpen">
        <!-- 左側聯絡人列表 -->
        <div class="chat-sidebar">
          <div class="sidebar-header">
            <span class="sidebar-title">好聊</span>
            <div class="sidebar-search">
              <el-input placeholder="搜尋名稱" prefix-icon="Search" size="small" />
            </div>
          </div>
          <div class="session-list">
          <div 
            v-for="session in sessions" 
            :key="session.otherUserId" 
            class="session-item"
            :class="{ active: chatStore.currentChatUser?.id === session.otherUserId }"
            @click="selectUser(session)"
          >
            <el-avatar :size="40" src="https://cube.elemecdn.com/3/7c/3ea6beec64369c2642b92c6726f1epng.png" />
            <div class="session-info">
              <div class="session-top">
                <span class="session-name">{{ session.otherUserName }}</span>
                <span class="session-time">{{ formatDate(session.sentAt) }}</span>
              </div>
              <div class="session-bottom">
                <span class="session-last-msg">{{ session.lastMessage }}</span>
                <el-badge :value="session.unreadCount" :hidden="session.unreadCount === 0" />
              </div>
            </div>
          </div>
          </div>
        </div>

        <!-- 右側對話區域 -->
        <div class="chat-main">
          <template v-if="chatStore.currentChatUser?.id">
            <div class="chat-header">
              <span class="chat-title">{{ chatStore.currentChatUser.name }}</span>
              <div class="header-actions">
                <el-icon class="close-btn" @click="chatStore.closeChat()"><Close /></el-icon>
              </div>
            </div>

            <div class="chat-body" ref="messageBox">
              <div v-for="(msg, index) in messages" :key="index" 
                   class="message-item" 
                   :class="{ 'is-me': msg.senderId === authStore.memberInfo.memberId }"
                   @contextmenu.prevent="showContextMenu($event, msg)">
                <div class="message-content">
                  <template v-if="msg.type === 1">
                    <el-image :src="msg.content" :preview-src-list="[msg.content]" class="chat-img" />
                  </template>
                  <template v-else-if="msg.type === 2">
                    <video :src="msg.content" controls class="chat-video"></video>
                  </template>
                  <template v-else-if="msg.type === 3">
                    <div class="file-card" @click="downloadFile(msg.content, '下載檔案')">
                      <el-icon :size="30"><Document /></el-icon>
                      <div class="file-info">
                        <div class="file-name">點擊下載檔案</div>
                        <div class="file-size">一般檔案</div>
                      </div>
                    </div>
                  </template>
                  <template v-else>
                    {{ msg.content }}
                  </template>
                </div>
                <div class="message-time">{{ formatTime(msg.sentAt) }}</div>
              </div>
            </div>

            <!-- 右鍵選單 -->
            <div v-if="contextMenu.show" 
                 class="custom-context-menu" 
                 :style="{ top: contextMenu.y + 'px', left: contextMenu.x + 'px' }">
              <div class="menu-item" v-if="contextMenu.msg?.type === 0" @click="copyText">複製文字</div>
            </div>

            <div class="chat-footer">
              <div class="footer-tools">
                <el-icon @click="triggerImageUpload" title="發送圖片"><Picture /></el-icon>
                <input type="file" ref="imageInput" style="display: none" accept="image/*" @change="handleImageUpload" />
                
                <el-icon @click="triggerVideoUpload" title="發送影片"><VideoCamera /></el-icon>
                <input type="file" ref="videoInput" style="display: none" accept="video/mp4,video/webm" @change="handleVideoUpload" />
                
                <el-icon @click="triggerFileUpload" title="發送檔案"><FolderOpened /></el-icon>
                <input type="file" ref="fileInput" style="display: none" 
                       accept=".pdf,.doc,.docx,.xls,.xlsx,.zip,.rar,.txt" @change="handleFileUpload" />
              </div>
              <div class="input-area">
                <el-input
                  v-model="inputMsg"
                  type="textarea"
                  :rows="2"
                  placeholder="輸入文字"
                  resize="none"
                  @keyup.enter.exact.prevent="handleSend"
                />
                <el-button type="primary" class="send-btn" :disabled="!inputMsg.trim()" @click="handleSend">
                  發送
                </el-button>
              </div>
            </div>
          </template>

          <div v-else class="welcome-view">
            <div class="welcome-content">
              <el-icon :size="80" color="#e2e8f0"><ChatLineSquare /></el-icon>
              <h3>歡迎使用好聊</h3>
              <p>開始與買賣家溝通吧！</p>
            </div>
            <el-icon class="close-btn-welcome" @click="chatStore.closeChat()"><Close /></el-icon>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, nextTick, computed } from 'vue';
import { 
  ChatDotRound, Close, ChatLineSquare, Search, 
  Picture, VideoCamera, FolderOpened, Document
} from '@element-plus/icons-vue';
import { useAuthStore } from '../../stores/auth';
import { useChatStore } from '../../stores/chat';
import { useChat } from '../../composables/useChat';
import { ElMessage } from 'element-plus';
import axios from 'axios';

const authStore = useAuthStore();
const chatStore = useChatStore();
const { messages, sessions, fetchSessions, fetchHistory, sendMessage } = useChat();

const inputMsg = ref('');
const messageBox = ref<HTMLElement | null>(null);
const imageInput = ref<HTMLInputElement | null>(null);
const videoInput = ref<HTMLInputElement | null>(null);
const fileInput = ref<HTMLInputElement | null>(null);

// 右鍵選單狀態
const contextMenu = ref({
  show: false,
  x: 0,
  y: 0,
  msg: null as any
});

const showContextMenu = (e: MouseEvent, msg: any) => {
  contextMenu.value = {
    show: true,
    x: e.clientX,
    y: e.clientY,
    msg: msg
  };
  const closeMenu = () => {
    contextMenu.value.show = false;
    document.removeEventListener('click', closeMenu);
  };
  setTimeout(() => document.addEventListener('click', closeMenu), 0);
};

const copyText = () => {
  if (contextMenu.value.msg?.content) {
    navigator.clipboard.writeText(contextMenu.value.msg.content);
    ElMessage.success('已複製到剪貼簿');
  }
};

const triggerImageUpload = () => imageInput.value?.click();
const triggerVideoUpload = () => videoInput.value?.click();
const triggerFileUpload = () => fileInput.value?.click();

const handleImageUpload = async (e: Event) => {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file || !chatStore.currentChatUser?.id) return;
  const formData = new FormData();
  formData.append('file', file);
  try {
    const res = await axios.post('https://localhost:7125/api/chat/upload', formData, {
      headers: { Authorization: `Bearer ${authStore.token}`, 'Content-Type': 'multipart/form-data' }
    });
    const fileUrl = 'https://localhost:7125' + res.data.url;
    await sendMessage(chatStore.currentChatUser!.id!, fileUrl, 1);
  } catch (err) { ElMessage.error('圖片上傳失敗'); }
  finally { (e.target as HTMLInputElement).value = ''; }
};

const handleVideoUpload = async (e: Event) => {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file || !chatStore.currentChatUser?.id) return;
  const formData = new FormData();
  formData.append('file', file);
  const loading = ElMessage({ message: '影片上傳中...', duration: 0 });
  try {
    const res = await axios.post('https://localhost:7125/api/chat/upload', formData, {
      headers: { Authorization: `Bearer ${authStore.token}`, 'Content-Type': 'multipart/form-data' }
    });
    const fileUrl = 'https://localhost:7125' + res.data.url;
    await sendMessage(chatStore.currentChatUser!.id!, fileUrl, 2);
    loading.close();
  } catch (err) { loading.close(); ElMessage.error('影片上傳失敗'); }
  finally { (e.target as HTMLInputElement).value = ''; }
};

const handleFileUpload = async (e: Event) => {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file || !chatStore.currentChatUser?.id) return;
  const formData = new FormData();
  formData.append('file', file);
  try {
    const res = await axios.post('https://localhost:7125/api/chat/upload', formData, {
      headers: { Authorization: `Bearer ${authStore.token}`, 'Content-Type': 'multipart/form-data' }
    });
    const fileUrl = 'https://localhost:7125' + res.data.url;
    await sendMessage(chatStore.currentChatUser!.id!, fileUrl, 3);
  } catch (err) { ElMessage.error('檔案上傳失敗'); }
  finally { (e.target as HTMLInputElement).value = ''; }
};

const downloadFile = (fileUrl: string, filename: string) => {
  const link = document.createElement('a');
  link.href = fileUrl;
  link.download = filename;
  link.target = '_blank';
  link.click();
};

const totalUnread = computed(() => {
  return sessions.value ? sessions.value.reduce((sum, s) => sum + (s.unreadCount || 0), 0) : 0;
});

const toggleChat = () => {
  chatStore.isChatOpen = !chatStore.isChatOpen;
  if (chatStore.isChatOpen) fetchSessions();
};

const selectUser = async (session: any) => {
  chatStore.openChatWithUser(session.otherUserId, session.otherUserName);
  await fetchHistory(session.otherUserId);
  scrollToBottom();
  fetchSessions();
};

const handleSend = async () => {
  if (!inputMsg.value.trim() || !chatStore.currentChatUser?.id) return;
  await sendMessage(chatStore.currentChatUser.id, inputMsg.value);
  inputMsg.value = '';
  scrollToBottom();
};

const scrollToBottom = async () => {
  await nextTick();
  if (messageBox.value) messageBox.value.scrollTop = messageBox.value.scrollHeight;
};

const formatDate = (dateStr?: string) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleDateString();
};

const formatTime = (dateStr?: string) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

watch(() => chatStore.isChatOpen, (newVal) => {
  if (newVal) {
    fetchSessions();
    if (chatStore.currentChatUser?.id) fetchHistory(chatStore.currentChatUser.id);
  }
});

watch(() => messages.value.length, () => scrollToBottom());
</script>

<style scoped>
.chat-float { position: fixed; right: 30px; bottom: 30px; z-index: 2000; }
.chat-trigger { width: 60px; height: 60px; background: #EE4D2D; border-radius: 50%; display: flex; flex-direction: column; align-items: center; justify-content: center; color: white; cursor: pointer; box-shadow: 0 4px 12px rgba(238, 77, 45, 0.4); }
.trigger-text { font-size: 12px; margin-top: -2px; }

.chat-container { position: absolute; right: 0; bottom: 80px; width: 800px; height: 600px; background: white; border-radius: 8px; box-shadow: 0 12px 32px rgba(0, 0, 0, 0.15); display: flex; overflow: hidden; border: 1px solid #e2e8f0; }
.chat-sidebar { width: 280px; border-right: 1px solid #f1f5f9; display: flex; flex-direction: column; background: white; }
.sidebar-header { padding: 15px; border-bottom: 1px solid #f1f5f9; }
.sidebar-title { font-size: 18px; font-weight: 600; color: #EE4D2D; display: block; margin-bottom: 10px; }
.session-list { flex: 1; overflow-y: auto; }
.session-item { display: flex; padding: 12px 15px; gap: 12px; cursor: pointer; transition: background 0.2s; }
.session-item:hover { background: #f8fafc; }
.session-item.active { background: #fff5f2; }
.session-info { flex: 1; min-width: 0; }
.session-top { display: flex; justify-content: space-between; margin-bottom: 4px; }
.session-name { font-weight: 500; font-size: 14px; }
.session-time { font-size: 11px; color: #94a3b8; }
.session-bottom { display: flex; justify-content: space-between; align-items: center; }
.session-last-msg { font-size: 12px; color: #64748b; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; flex: 1; }

.chat-main { flex: 1; display: flex; flex-direction: column; background: #f8fafc; position: relative; }
.chat-header { padding: 15px; background: white; border-bottom: 1px solid #f1f5f9; display: flex; justify-content: space-between; align-items: center; }
.chat-title { font-weight: 600; font-size: 16px; }
.close-btn { cursor: pointer; color: #94a3b8; font-size: 20px; }

.chat-body { flex: 1; padding: 20px; overflow-y: auto; display: flex; flex-direction: column; gap: 15px; }
.message-item { max-width: 85%; display: flex; flex-direction: column; align-items: flex-start; }
.message-item.is-me { align-self: flex-end; align-items: flex-end; }

.message-content { 
  padding: 8px 12px; 
  border-radius: 8px; 
  font-size: 14px; 
  background: white; 
  box-shadow: 0 1px 2px rgba(0,0,0,0.05); 
  border: 1px solid #e2e8f0; 
  position: relative; 
  word-break: break-all;
  display: inline-block; /* 讓寬度隨內容縮放 */
}
.is-me .message-content { background: #EE4D2D; color: white; border-color: #EE4D2D; }

.chat-img { max-width: 200px; max-height: 200px; border-radius: 4px; cursor: pointer; }
.chat-video { max-width: 280px; border-radius: 8px; background: black; }
.file-card { display: flex; align-items: center; gap: 12px; padding: 10px; background: #f8fafc; border-radius: 6px; border: 1px solid #e2e8f0; cursor: pointer; min-width: 180px; }
.is-me .file-card { background: rgba(255, 255, 255, 0.1); border-color: rgba(255, 255, 255, 0.2); color: white; }
.file-info { flex: 1; }
.file-name { font-size: 13px; font-weight: 500; }
.file-size { font-size: 11px; opacity: 0.7; }
.message-time { font-size: 10px; color: #94a3b8; margin-top: 4px; }

.custom-context-menu { position: fixed; background: white; border-radius: 4px; box-shadow: 0 2px 12px rgba(0,0,0,0.1); padding: 5px 0; z-index: 3000; min-width: 100px; border: 1px solid #e2e8f0; }
.menu-item { padding: 8px 16px; font-size: 13px; color: #334155; cursor: pointer; transition: background 0.2s; }
.menu-item:hover { background: #f1f5f9; }

.chat-footer { background: white; padding: 15px; border-top: 1px solid #f1f5f9; }
.footer-tools { display: flex; gap: 15px; margin-bottom: 10px; color: #64748b; font-size: 18px; }
.footer-tools .el-icon { cursor: pointer; }
.footer-tools .el-icon:hover { color: #EE4D2D; }
.input-area { display: flex; gap: 10px; align-items: flex-end; }
.send-btn { background: #EE4D2D; border: none; }
.welcome-view { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; color: #94a3b8; }
.welcome-content { text-align: center; }
.close-btn-welcome { position: absolute; top: 15px; right: 15px; cursor: pointer; font-size: 20px; }
</style>
