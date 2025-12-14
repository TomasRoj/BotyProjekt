using Org.BouncyCastle.Tls;
using System.ComponentModel.DataAnnotations.Schema;

namespace BotyProjekt.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        [ForeignKey("OrderId")]
        public int OrderId { get; set; }
        [ForeignKey("ProductVariatnId")]
        public int ProductVariantId { get; set; }

        public int Quantity { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
    }
}
