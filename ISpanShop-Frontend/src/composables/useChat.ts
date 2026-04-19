import { ref, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';
import axios from 'axios';
import { useAuthStore } from '../stores/auth';
import type { ChatMessage } from '../types/chat';

export function useChat() {
  const authStore = useAuthStore();
  const connection = ref<signalR.HubConnection | null>(null);
  const messages = ref<ChatMessage[]>([]);
  const sessions = ref<any[]>([]);
  const isConnected = ref(false);

  // 初始化 SignalR 連線
  const initConnection = async () => {
    if (!authStore.token) return;

    // 請確保此網址與您的 ASP.NET Core 後端一致
    const hubUrl = 'https://localhost:7193/chatHub';

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => authStore.token!,
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    connection.value.on('ReceiveMessage', (senderId: number, content: string, type: number) => {
      messages.value.push({
        senderId,
        receiverId: authStore.memberInfo.memberId!,
        content,
        type,
        sentAt: new Date().toISOString()
      });
      // 收到訊息時也刷新清單
      fetchSessions();
    });

    try {
      await connection.value.start();
      isConnected.value = true;
      console.log('SignalR Connected.');
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
    }
  };

  // 取得對話紀錄
  const fetchHistory = async (otherUserId: number) => {
    try {
      const response = await axios.get(`https://localhost:7193/api/chat/history/${otherUserId}`, {
        headers: { Authorization: `Bearer ${authStore.token}` }
      });
      messages.value = response.data;
    } catch (err) {
      console.error('Fetch History Error:', err);
    }
  };

  // 取得聯絡人列表
  const fetchSessions = async () => {
    try {
      const response = await axios.get('https://localhost:7193/api/chat/sessions', {
        headers: { Authorization: `Bearer ${authStore.token}` }
      });
      sessions.value = response.data;
    } catch (err) {
      console.error('Fetch Sessions Error:', err);
    }
  };

  // 發送訊息
  const sendMessage = async (receiverId: number, content: string, type: number = 0) => {
    if (connection.value && isConnected.value) {
      try {
        await connection.value.invoke('SendMessage', receiverId, content, type);
        // 本地立即增加訊息，讓介面流暢
        messages.value.push({
          senderId: authStore.memberInfo.memberId!,
          receiverId,
          content,
          type,
          sentAt: new Date().toISOString()
        });
      } catch (err) {
        console.error('Send Message Error: ', err);
      }
    }
  };

  // 組件掛載時啟動連線
  onMounted(() => {
    if (authStore.isLoggedIn) {
      initConnection();
    }
  });

  // 組件卸載時斷開連線
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
