using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Models.Finance.DTOs
{
    // ⭐ CurrencyDto - للعرض فقط
    public class CurrencyDto
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [Display(Name = "Currency Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Currency Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        [Display(Name = "Symbol")]
        public string Symbol { get; set; } = string.Empty;

        [Range(0, 4)]
        [Display(Name = "Decimal Places")]
        public int Decimals { get; set; } = 2;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // ⭐ Audit fields (للعرض فقط)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // ⭐ CreateCurrencyDto - للإضافة
    public class CreateCurrencyDto
    {
        [Required(ErrorMessage = "Currency code is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
        [Display(Name = "Currency Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency name is required")]
        [StringLength(100, ErrorMessage = "Currency name cannot exceed 100 characters")]
        [Display(Name = "Currency Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency symbol is required")]
        [StringLength(5, ErrorMessage = "Currency symbol cannot exceed 5 characters")]
        [Display(Name = "Symbol")]
        public string Symbol { get; set; } = string.Empty;

        [Range(0, 4, ErrorMessage = "Decimals must be between 0 and 4")]
        [Display(Name = "Decimal Places")]
        public int Decimals { get; set; } = 2;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    // ⭐ UpdateCurrencyDto - للتحديث
    public class UpdateCurrencyDto
    {
        // الـ Code للتعريف فقط (read-only في الـ form)
        [Required]
        [StringLength(3, MinimumLength = 3)]
        [Display(Name = "Currency Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency name is required")]
        [StringLength(100, ErrorMessage = "Currency name cannot exceed 100 characters")]
        [Display(Name = "Currency Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Currency symbol is required")]
        [StringLength(5, ErrorMessage = "Currency symbol cannot exceed 5 characters")]
        [Display(Name = "Symbol")]
        public string Symbol { get; set; } = string.Empty;

        [Range(0, 4, ErrorMessage = "Decimals must be between 0 and 4")]
        [Display(Name = "Decimal Places")]
        public int Decimals { get; set; } = 2;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}