namespace AppointmentSystem.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("sales_managers")]
    public class SalesManager
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column(name: "languages", TypeName = "varchar[]")]
        public List<string> Languages { get; set; } = new();

        [Column(name: "products", TypeName = "varchar[]")]
        public List<string> Products { get; set; } = new();

        [Column(name: "customer_ratings", TypeName = "varchar[]")]
        public List<string> CustomerRatings { get; set; } = new();

        // Navigation Property (EF Core automatically loads slots)
        public virtual List<Slot> Slots { get; set; } = new();

        // Auto-updating HashSets
        [NotMapped]
        public HashSet<string> LanguagesHashSet => new(Languages);

        [NotMapped]
        public HashSet<string> ProductsHashSet => new(Products);

        [NotMapped]
        public HashSet<string> CustomerRatingsHashSet => new(CustomerRatings);
    }
}
