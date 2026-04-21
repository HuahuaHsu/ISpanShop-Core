import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useChatStore = defineStore('chat', () => {
  // 當前對話的對象資訊
  const currentChatUser = ref<{
    id: number | null;
    name: string;
    avatar?: string;
  }>({
    id: null,
    name: ''
  });

  // 控制聊天視窗是否開啟
  const isChatOpen = ref(false);

  /** 
   * 開啟與特定對象的聊天
   * @param userId 對象 ID
   * @param userName 對象名稱
   */
  function openChatWithUser(userId: number, userName: string) {
    currentChatUser.value = { id: userId, name: userName };
    isChatOpen.value = true;
  }

  function closeChat() {
    isChatOpen.value = false;
  }

  return {
    currentChatUser,
    isChatOpen,
    openChatWithUser,
    closeChat
  };
});
