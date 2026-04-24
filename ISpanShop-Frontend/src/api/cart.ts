import axios from './axios';
import type { CartItem } from '../stores/cart';

/**
 * 獲取用戶購物車
 */
export const getCartApi = () => {
  return axios.get<any[]>('/api/front/cart');
};

/**
 * 加入購物車
 */
export const addToCartApi = (data: { productId: number; variantId: number | null; quantity: number }) => {
  return axios.post('/api/front/cart/add', data);
};

/**
 * 更新購物車品項數量
 */
export const updateCartItemApi = (data: { productId: number; variantId: number | null; quantity: number }) => {
  return axios.post('/api/front/cart/update', data);
};

/**
 * 移除購物車品項
 */
export const removeCartItemApi = (productId: number, variantId: number | null) => {
  const url = variantId 
    ? `/api/front/cart/remove/${productId}/${variantId}` 
    : `/api/front/cart/remove/${productId}`;
  return axios.delete(url);
};

/**
 * 同步本地購物車到伺服器
 */
export const syncCartApi = (localItems: any[]) => {
  return axios.post<any[]>('/api/front/cart/sync', localItems);
};

/**
 * 清空購物車
 */
export const clearCartApi = () => {
  return axios.delete('/api/front/cart/clear');
};
