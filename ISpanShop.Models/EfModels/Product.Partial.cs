using System;
using System.Collections.Generic;

namespace ISpanShop.Models.EfModels;

public partial class Product
{
    /// <summary>
    /// 動態屬性 JSON (OptionId 與 CustomValue 的集合)
    /// </summary>
    public string? AttributesJson { get; set; }
}
