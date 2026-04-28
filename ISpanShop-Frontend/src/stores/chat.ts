import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import * as signalR from '@microsoft/signalr';
import axios from 'axios';
import { useAuthStore } from './auth';
import { ElMessage } from 'element-plus';
import type { ChatMessage } from '../types/chat';

export const useChatStore = defineStore('chat', () => {
  const authStore = useAuthStore();
  
  // --- State ---
  const currentChatUser = ref<{
    id: number | null;
    name: string;
    avatar?: string;
  }>({
    id: null,
    name: ''
  });

  const isChatOpen = ref(false);
  const hiddenUserIds = ref<Set<number>>(new Set());
  
  // 新增：全域訊息與會話狀態
  const messages = ref<ChatMessage[]>([]);
  const sessions = ref<any[]>([]);
  const connection = ref<signalR.HubConnection | null>(null);
  const isConnected = ref(false);
  // 新增：強制刷新用的時間戳
  const lastMessageAt = ref(Date.now());

  // --- Getters ---
  const totalUnread = computed(() => {
    return sessions.value ? sessions.value.reduce((sum, s) => sum + (s.unreadCount || 0), 0) : 0;
  });

  // --- Actions ---
  
  /** 初始化 SignalR */
  async function initConnection() {
    if (!authStore.token || connection.value) return;

    const hubUrl = 'https://localhost:7125/chatHub';

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => authStore.token!,
      })
      .withAutomaticReconnect()
      .build();

    connection.value.on('ReceiveMessage', (senderId: any, content: string, type: number) => {
      // 確保 ID 都是數字型態
      const sId = Number(senderId);
      const myId = Number(authStore.memberInfo.memberId);
      const currentId = currentChatUser.value.id ? Number(currentChatUser.value.id) : null;

      console.log('%c[SignalR] Message Arrived!', 'color: white; background: #EE4D2D; padding: 2px 5px;', { 
        sId, 
        content, 
        currentId
      });

      // 1. 解除隱藏
      if (hiddenUserIds.value.has(sId)) {
        hiddenUserIds.value.delete(sId);
      }

      // 2. 判斷是否為當前對話
      const isCurrentChat = currentId !== null && currentId === sId;
      const isMe = myId === sId;

      console.log(`[ChatStore] Reactivity Check: isCurrent=${isCurrentChat}, isMe=${isMe}`);

      // 3. 建立新訊息
      const newMessage = {
        id: Date.now() + Math.random(),
        senderId: sId,
        receiverId: isMe ? (currentId || 0) : myId,
        content,
        type,
        isRead: isCurrentChat,
        sentAt: new Date().toISOString()
      };

      // 4. 更新訊息列表 (即使是當前對話，也要強制觸發)
      if (isCurrentChat || isMe) {
        // 去重判斷 (針對自己發送的訊息)
        const lastMsg = messages.value[messages.value.length - 1];
        if (isMe && lastMsg && lastMsg.senderId === myId && lastMsg.content === content) {
           console.log('[ChatStore] Detected self-sent duplicate, skipping.');
        } else {
           // 暴力法：直接賦值新陣列並更新時間戳
           messages.value = [...messages.value, newMessage];
           lastMessageAt.value = Date.now();
           console.log('[ChatStore] Message List Updated. Count:', messages.value.length);
        }
        
        if (isCurrentChat && !isMe) {
          markAsRead(sId);
        }
      } else {
        // 非當前對話，彈出通知
        ElMessage({
          message: `收到新訊息: ${content.substring(0, 15)}...`,
          type: 'info',
          plain: true,
          duration: 2000
        });
      }

      // 5. 刷新會話列表
      fetchSessions();
    });

    try {
      await connection.value.start();
      isConnected.value = true;
      console.log('SignalR Store Connected.');
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
    }
  }

  async function markAsRead(senderId: number) {
    if (!authStore.token) return;
    try {
      // 呼叫 API 標記已讀 (這裡我們沿用 fetchHistory，因為它內部會標記已讀)
      // 或者您可以新增一個專門的 markAsRead API
      await axios.post(`https://localhost:7125/api/chat/read/${senderId}`, {}, {
        headers: { Authorization: `Bearer ${authStore.token}` }
      });
    } catch (err) {
      console.warn('Mark as read failed (might not have endpoint yet):', err);
    }
  }

  async function fetchHistory(otherUserId: number) {
    if (!authStore.token) return;
    try {
      const response = await axios.get(`https://localhost:7125/api/chat/history/${otherUserId}`, {
        headers: { Authorization: `Bearer ${authStore.token}` }
      });
      messages.value = response.data;
    } catch (err) {
      console.error('Fetch History Error:', err);
    }
  }

  async function fetchSessions() {
    if (!authStore.token) return;
    try {
      const response = await axios.get('https://localhost:7125/api/chat/sessions', {
        headers: { Authorization: `Bearer ${authStore.token}` }
      });
      sessions.value = response.data;
    } catch (err) {
      console.error('Fetch Sessions Error:', err);
    }
  }

  async function sendMessage(receiverId: number, content: string, type: number = 0) {
    if (connection.value && isConnected.value) {
      try {
        await connection.value.invoke('SendMessage', receiverId, content, type);
      } catch (err) {
        console.error('Send Message Error: ', err);
      }
    }
  }

  function openChatWithUser(userId: number, userName: string) {
    if (hiddenUserIds.value.has(userId)) {
      hiddenUserIds.value.delete(userId);
    }
    currentChatUser.value = { id: userId, name: userName };
    isChatOpen.value = true;
    messages.value = []; // 先清空，等待 fetchHistory
    fetchHistory(userId);
  }

  function hideSession(userId: number) {
    hiddenUserIds.value.add(userId);
  }

  function closeChat() {
    isChatOpen.value = false;
  }

  function stopConnection() {
    if (connection.value) {
      connection.value.stop();
      connection.value = null;
      isConnected.value = false;
    }
  }

  return {
    currentChatUser,
    isChatOpen,
    hiddenUserIds,
    messages,
    sessions,
    isConnected,
    totalUnread,
    initConnection,
    fetchHistory,
    fetchSessions,
    sendMessage,
    openChatWithUser,
    hideSession,
    closeChat,
    stopConnection
  };
});
