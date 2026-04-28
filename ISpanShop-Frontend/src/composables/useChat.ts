import { storeToRefs } from 'pinia';
import { useChatStore } from '../stores/chat';

export function useChat() {
  const chatStore = useChatStore();
  const { messages, sessions, isConnected } = storeToRefs(chatStore);

  return {
    messages,
    sessions,
    isConnected,
    fetchHistory: chatStore.fetchHistory,
    fetchSessions: chatStore.fetchSessions,
    sendMessage: chatStore.sendMessage
  };
}
