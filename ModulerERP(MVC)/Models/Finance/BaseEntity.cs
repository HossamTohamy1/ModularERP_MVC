namespace ModularERP.Common.Models
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? UpdatedById { get; set; }
    }
}
