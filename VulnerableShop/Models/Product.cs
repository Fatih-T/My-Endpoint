namespace VulnerableShop.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
