using ModularERP.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace ModulerERP_MVC_.Models.Finance
{
    public class VoucherAttachment : BaseEntity
    {

        public Guid VoucherId { get; set; }

        [Required, MaxLength(255)]
        public string Filename { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string MimeType { get; set; } = string.Empty;

        public int FileSize { get; set; }

        [MaxLength(64)]
        public string? Checksum { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Guid UploadedBy { get; set; }

        // Navigation properties
        public virtual Voucher Voucher { get; set; } = null!;
        public virtual ApplicationUser UploadedByUser { get; set; } = null!;
    }
}
