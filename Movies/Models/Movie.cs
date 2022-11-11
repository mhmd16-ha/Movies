using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.Models
{
    public class Movie
    {
        public int Id { get; set; }
        [Required, MaxLength(250)]
        public string Title { get; set; }
        public int Year { get; set; }
        public Double Rate { get; set; }
        [Required, MaxLength(2500)]
        public string StoryLine { get; set; }
        [Required]
        public byte[] poster { get; set; }
        public byte GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}
