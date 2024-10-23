namespace SeriesMvc.Models
{
    public class MovieCategory
    {
        public int MovieCategoryId { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }   

        public int CategoryId { get; set; } 
        public Category Category { get; set; }
    }
}
