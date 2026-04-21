import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { loginApi } from '../api/auth';
import { getMemberProfile } from '../api/member'
import type { LoginRequest } from '../types/auth';
import { storage } from '../utils/storage';

export const useAuthStore = defineStore('auth', () => {
  // State
  const token = ref<string | null>(storage.getToken());
  const isLoginDialogOpen = ref(false); // 控制彈窗顯示
  const memberInfo = ref<{
    memberId: number | null;
    email: string | null;
    account: string | null;
    memberName: string | null;
    levelName: string | null;
    pointBalance: number | null;
    avatarUrl: string | null;
    isSeller: boolean;
  }>(storage.getUser() || {
    memberId: null,
    email: null,
    account: null,
    memberName: null,
    levelName: null,
    pointBalance: null,
    avatarUrl: null,
    isSeller: false
  });

  // Getters
  const isLoggedIn = computed(() => !!token.value);

  // Actions
  async function login(loginData: LoginRequest) {
    try {
      // 登入前先強制登出，確保舊 Token (例如 admin) 被清除
      logout();

      const response = await loginApi(loginData);
      const { data } = response;

      // 1. 存入 Token 與 使用者資訊 (由後端 DTO 回傳)
      token.value = data.token;
      memberInfo.value = {
        memberId: data.memberId,
        email: data.email,
        account: data.account,
        memberName: data.memberName,
        levelName: data.levelName,
        pointBalance: data.pointBalance,
        avatarUrl: data.avatarUrl || null,
        isSeller: data.isSeller
      };

      // 2. 持久化到 localStorage
        storage.setToken(data.token);
        storage.setUser(memberInfo.value);;
    try {
        const profileRes = await getMemberProfile(data.memberId)
        memberInfo.value.avatarUrl = profileRes.data.avatarUrl ?? null
        storage.setUser(memberInfo.value)
      } catch {
        // 拿不到也沒關係，不影響登入
      }

      return true;

    } catch (error) {
      console.error('登入失敗:', error);
      throw error;
    }
  }

  function logout() {
    // 1. 重置 State
    token.value = null;
    memberInfo.value = {
      memberId: null,
      email: null,
      account: null,
      memberName: null,
      levelName: null,
      pointBalance: null,
      avatarUrl: null,
      isSeller: false
    };

    // 2. 清除 localStorage
    storage.clearAll();
  }

  /** 更新點數並同步持久化到 localStorage */
  function updatePoints(newBalance: number) {
    if (memberInfo.value) {
      memberInfo.value.pointBalance = newBalance;
      storage.setUser(memberInfo.value);
    }
  }

  /** 更新賣家身分並同步持久化到 localStorage */
  function updateSellerStatus(isSeller: boolean) {
    if (memberInfo.value) {
      memberInfo.value.isSeller = isSeller;
      storage.setUser(memberInfo.value);
    }
  }

  function openLoginDialog() {
    isLoginDialogOpen.value = true;
  }

  function closeLoginDialog() {
    isLoginDialogOpen.value = false;
  }

  return {
    token,
    isLoginDialogOpen,
    memberInfo,
    isLoggedIn,
    login,
    logout,
    updatePoints,
    updateSellerStatus,
    openLoginDialog,
    closeLoginDialog
  };
});
