using System;
using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.DTOs.Members
{
    /// <summary>
    /// 前台會員自行更新個人資料專用 DTO
    /// </summary>
    public class UpdateMemberProfileDto
    {
        public int Id { get; set; }

        [Required]
        public string Account { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email 格式不正確")]
        public string Email { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public byte? Gender { get; set; }

        public DateOnly? Birthday { get; set; }

        public string AvatarUrl { get; set; }
    }
}
