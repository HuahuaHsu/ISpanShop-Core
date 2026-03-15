using ISpanShop.Models.DTOs.Admins;
using ISpanShop.Models.DTOs.Common;
using System.Collections.Generic;

namespace ISpanShop.MVC.Areas.Admin.Models.Admins
{
    public class AdminIndexVm
    {
        /// <summary>管理員分頁結果</summary>
        public PagedResult<AdminDto> PagedResult { get; set; } = new PagedResult<AdminDto>();

        /// <summary>身分等級列表（含預設權限）</summary>
        public List<AdminLevelDto> AdminLevels { get; set; } = new List<AdminLevelDto>();

        /// <summary>所有可用權限列表（Checkbox 來源）</summary>
        public List<PermissionDto> AllPermissions { get; set; } = new List<PermissionDto>();

        /// <summary>新增管理員表單</summary>
        public AdminCreateVm CreateForm { get; set; } = new AdminCreateVm();

        /// <summary>新增身分等級表單</summary>
        public AdminLevelCreateVm LevelCreateForm { get; set; } = new AdminLevelCreateVm();

        /// <summary>系統預產的下一個帳號（Modal 唯讀顯示用）</summary>
        public string NextAccount { get; set; } = "";

        /// <summary>操作結果訊息</summary>
        public string Message { get; set; }

        /// <summary>當前選中的 Tab（"tab1" 管理員列表，"tab2" 身分與權限）</summary>
        public string ActiveTab { get; set; } = "tab1";

        public string Keyword { get; set; }
        public string Status { get; set; }
        public int? SelectedAdminLevelId { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
    }
}