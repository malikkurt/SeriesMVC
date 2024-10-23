namespace SeriesMvc.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }

        public ICollection<MovieCategory> MovieCategories { get; set; }
    }
}
