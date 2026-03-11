using ISpanShop.Models.EfModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISpanShop.Models.EFModels
{
	public partial class SensitiveWordCategory
	{
		public SensitiveWordCategory()
		{
			SensitiveWords = new HashSet<SensitiveWord>();
		}

		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		// 導覽屬性：一個分類對應多個敏感字
		public virtual ICollection<SensitiveWord> SensitiveWords { get; set; }
	}
}