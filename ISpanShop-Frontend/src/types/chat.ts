export interface ChatMessage {
  id?: number;
  senderId: number;
  receiverId: number;
  content: string;
  type: number; // 0: 文字, 1: 圖片, etc.
  isRead?: boolean;
  sentAt?: string;
}

export interface ChatSession {
  sessionId: string;
  otherUser: {
    id: number;
    name: string;
    avatar?: string;
  };
  lastMessage?: string;
  lastSentAt?: string;
  unreadCount: number;
}
