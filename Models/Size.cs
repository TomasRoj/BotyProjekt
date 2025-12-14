namespace BotyProjekt.Models
{
    public class Size
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ProductVariant> Variants { get; set; }
    }
}
