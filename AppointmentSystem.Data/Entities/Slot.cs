using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentSystem.Data.Entities
{
    [Table("slots")]
    public class Slot
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("booked")]
        public bool Booked { get; set; } = false;

        [Required]
        [ForeignKey(nameof(SalesManager))]
        [Column("sales_manager_id")]
        public int SalesManagerId { get; set; }

        public virtual SalesManager SalesManager { get; set; } = null!;
    }
}
