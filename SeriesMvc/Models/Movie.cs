﻿namespace SeriesMvc.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; }
        public ICollection<MovieCategory> MovieCategories { get; set; }
    }
}