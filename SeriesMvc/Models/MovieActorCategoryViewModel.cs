namespace SeriesMvc.Models
{
    public class MovieActorCategoryViewModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public List<string> Actors { get; set; } = new List<string>();
        public List<string> Categories { get; set; } = new List<string>();
    }

}
