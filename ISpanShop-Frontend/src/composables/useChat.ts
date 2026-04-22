import { ref, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';
import axios from 'axios';
import { useAuthStore } from '../stores/auth';
import { useChatStore } from '../stores/chat';
import type { ChatMessage } from '../types/chat';

export function useChat() {
  const authStore = useAuthStore();
  const chatStore = useChatStore(); // 確保可以使用 chatStore
  const connection = ref<signalR.HubConnection | null>(null);
  const messages = ref<ChatMessage[]>([]);
  const sessions = ref<any[]>([]);
  const isConnected = ref(false);

  // 初始化 SignalR 連線
  const initConnection = async () => {
    if (!authStore.token) return;

    const hubUrl = 'https://localhost:7125/chatHub';

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => authStore.token!,
      })
      .withAutomaticReconnect()
      .build();

    // 監聽接收訊息
    connection.value.on('ReceiveMessage', (senderId: number, content: string, type: number) => {
      console.log('SignalR ReceiveMessage:', { senderId, content });

      // --- 新增：收到新訊息時解除隱藏 ---
      if (chatStore.hiddenUserIds.has(senderId)) {
        chatStore.hiddenUserIds.delete(senderId);
      }

      messages.value.push({
        senderId,
        receiverId: authStore.memberInfo.memberId, 
        content,
        type,
        isRead: false, // 初始化已讀狀態
        sentAt: new Date().toISOString()
      } as any);
      fetchSessions();
    });

    try {
      await connection.value.start();
      isConnected.value = true;
      console.log('SignalR Connected to 7125.');
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
    }
  };

  const fetchHistory = async (otherUserId: number) => {
    messages.value = []; // --- 新增：切換時立即清空舊訊息 ---
    try {
      const response = await axios.get(`https://localhost:7125/api/chat/history/${otherUserId}`, {
        headers: { Authorization: `Bearer ${authStore.token}` }
      });
      messages.value = response.data;
    } catch (err) {
      console.error('Fetch History Error:', err);
    }
  };

  const fetchSessions = async () => {
    try {
      const response = await axios.get('https://localhost:7125/api/chat/sessions', {
        headers: { Authorization: `Bearer ${authStore.token}` }
      });
      console.log('Chat Sessions Data:', response.data);
      sessions.value = response.data;
    } catch (err) {
      console.error('Fetch Sessions Error:', err);
    }
  };

  const sendMessage = async (receiverId: number, content: string, type: number = 0) => {
    console.log('Attempting to send message:', { receiverId, content, type });
    if (connection.value && isConnected.value) {
      try {
        await connection.value.invoke('SendMessage', receiverId, content, type);
        console.log('Message sent successfully via SignalR');
      } catch (err) {
        console.error('Send Message Error: ', err);
      }
    }
  };

  onMounted(() => {
    if (authStore.isLoggedIn) {
      initConnection();
    }
  });

  onUnmounted(() => {
    if (connection.value) {
      connection.value.stop();
    }
  });

  return {
    messages,
    sessions,
    isConnected,
    fetchHistory,
    fetchSessions,
    sendMessage
  };
}
