/**
 * api-helpers.js
 * 統一的 Fetch API 工具函式，供後台各頁面共用。
 *
 * 使用範例：
 *   const data = await apiGet('/Admin/Products/GetSubCategories?parentId=1');
 *   const result = await apiPost('/Admin/Products/ApproveProduct', { id: 5 });
 */

/**
 * 發送 GET 請求並回傳解析後的 JSON。
 * @param {string} url 請求網址
 * @returns {Promise<any>} 解析後的 JSON 物件
 * @throws {Error} HTTP 錯誤或網路錯誤時拋出
 */
async function apiGet(url) {
    const res = await fetch(url);
    if (!res.ok) throw new Error(`GET ${url} 失敗 (HTTP ${res.status})`);
    return res.json();
}

/**
 * 發送 POST 請求（JSON body）並回傳解析後的 JSON。
 * @param {string} url 請求網址
 * @param {object} [data={}] 要送出的資料物件，會序列化為 JSON
 * @returns {Promise<any>} 解析後的 JSON 物件
 * @throws {Error} HTTP 錯誤或網路錯誤時拋出
 */
async function apiPost(url, data = {}) {
    const res = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    });
    if (!res.ok) throw new Error(`POST ${url} 失敗 (HTTP ${res.status})`);
    return res.json();
}
