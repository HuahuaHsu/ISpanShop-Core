export interface SupportTicket {
  id?: number;
  userId: number;
  userName?: string;
  orderId?: number | null;
  category: number; // 0:訂單問題, 1:帳號問題, 2:檢舉
  subject: string;
  description: string;
  attachmentUrl?: string | null;
  status: number; // 0:待處理, 1:處理中, 2:已結案
  adminReply?: string | null;
  createdAt?: string;
  resolvedAt?: string | null;
}

export const SUPPORT_CATEGORIES = [
  { label: '訂單問題', value: 0 },
  { label: '帳號問題', value: 1 },
  { label: '檢舉與其他', value: 2 }
];

export const TICKET_STATUS = {
  0: { label: '待處理', type: 'warning' },
  1: { label: '處理中', type: 'primary' },
  2: { label: '已結案', type: 'success' }
};
