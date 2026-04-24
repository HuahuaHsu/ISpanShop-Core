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
    isBlacklisted: boolean;
  }>(storage.getUser() || {
    memberId: null,
    email: null,
    account: null,
    memberName: null,
    levelName: null,
    pointBalance: null,
    avatarUrl: null,
    isSeller: false,
    isBlacklisted: false
  });

  // Getters
  const isLoggedIn = computed(() => !!token.value);
  const isBlacklisted = computed(() => memberInfo.value.isBlacklisted);

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
        isSeller: data.isSeller,
        isBlacklisted: data.isBlacklisted
      };

      // 2. 持久化到 localStorage
      storage.setToken(data.token);
      storage.setUser(memberInfo.value);

      try {
        const profileRes = await getMemberProfile()
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
      isSeller: false,
      isBlacklisted: false
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

  /** 更新等級名稱並同步持久化到 localStorage */
  function updateLevel(levelName: string) {
    if (memberInfo.value) {
      memberInfo.value.levelName = levelName;
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

  /** 獲取最新會員資料並同步到 store 與 localStorage */
  async function fetchUserInfo() {
    try {
      const response = await getMemberProfile();
      const { data } = response;
      memberInfo.value = {
        ...memberInfo.value,
        memberId: data.id,
        email: data.email,
        account: data.account,
        memberName: data.fullName,
        levelName: data.levelName || '一般會員',
        pointBalance: data.pointBalance ?? memberInfo.value.pointBalance,
        avatarUrl: data.avatarUrl || null,
        isBlacklisted: data.isBlacklisted ?? data.IsBlacklisted ?? false // 同步封鎖狀態
      };
      storage.setUser(memberInfo.value);
    } catch (error) {
      console.error('獲取會員資料失敗:', error);
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
    isBlacklisted,
    login,
    logout,
    updatePoints,
    updateLevel,
    updateSellerStatus,
    fetchUserInfo,
    openLoginDialog,
    closeLoginDialog
  };
});
