using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultGeneratorV2._0
{
    public class Rating
    {
        public string UserId { get; set; }
        public string MovieId { get; set; }
        public double GivenRating { get; set; }

        public Rating(string userId, string movieId, double rating)
        {
            this.UserId = userId;
            this.MovieId = movieId;
            this.GivenRating = rating;
        }

        public override bool Equals(object obj)
        {
            return this.UserId == (obj as Rating).UserId && this.MovieId == (obj as Rating).MovieId;
        }

        public override int GetHashCode()
        {
            return (this.UserId + this.MovieId).GetHashCode();
        }
    }
}
