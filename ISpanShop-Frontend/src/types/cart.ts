/**
 * 購物車項目介面 (對應後端 CartItems 結構)
 */
export interface CartItem {
  /** 購物車項目 ID (CartItems.Id) */
  id: number;
  /** 商品 ID (CartItems.ProductId) */
  productId: number;
  /** 規格 ID (CartItems.VariantId) */
  variantId: number;
  /** 商品名稱 (Products.Name) */
  productName: string;
  /** 規格名稱 (ProductVariants.VariantName) */
  variantName: string;
  /** 單價 (CartItems.UnitPrice 或 ProductVariants.Price) */
  price: number;
  /** 商品圖片路徑 (ProductImages.ImageUrl) */
  imageUrl: string;
  /** 購買數量 (CartItems.Quantity) */
  quantity: number;
  /** 庫存數量 (ProductVariants.Stock) */
  stock: number;
  /** 前端選取狀態 (預設為 true) */
  selected: boolean;
}

/**
 * 購物車全域狀態介面
 */
export interface CartState {
  /** 購物車內的商品清單 */
  items: CartItem[];
}
