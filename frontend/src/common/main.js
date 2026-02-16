import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap'; // 這是 Bootstrap 的 JS 功能
import './site.scss';// 引用你建立的 site.scss
import $ from 'jquery';//搬入 jQuery 並設定為變數 $

//如果需要在瀏覽器 Console 或是傳統標籤裡直接用 $，可以把它掛在 window 上
window.$ = window.jQuery = $;

console.log("MVC 基礎 CSS/JS 打包成功！");

$(function () {
    console.log("Bootstrap JS 測試啟動");

    // 建立一個簡單的測試：點擊按鈕彈出警告
    $('.btn-danger').on('click', function () {
        alert('Bootstrap + jQuery 聯手測試成功！');
    });
});