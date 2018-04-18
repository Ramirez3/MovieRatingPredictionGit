using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultGeneratorV2._0
{
    public class User
    {
        const int fIdUser = 0;
        const int fGender = 1;
        const int fAge = 2;
        const int fOccupation = 3;
        const int sRatingMin = 1;
        const int sRatingMax = 2;
        const int sRatingMean = 3;
        const int sRatingMedian = 4;

        public string UserId { get; set; }
        public bool IsMale { get; set; }
        public int AgeGroup { get; set; }
        public string Occupation { get; set; }
        
        public double RatingMean { get; set; }

        public Dictionary<Movie, double> Ratings { get; set; }

        public Dictionary<string, double> SimUsers { get; set; }

        public User(string[] splittedLine)
        {
            this.UserId = splittedLine[fIdUser];
            this.IsMale = splittedLine[fGender] == "M";
            this.AgeGroup = int.Parse(splittedLine[fAge]);
            this.Occupation = splittedLine[fOccupation];

            this.Ratings = new Dictionary<Movie, double>();

            this.SimUsers = new Dictionary<string, double>();
        }

        public double GetUserSim(User user, int minAge, int maxAge)
        {
            double thisAgeNorm;
            double userAgeNorm;
            if (maxAge-minAge==0)
            {
                thisAgeNorm = 1;
                userAgeNorm = 1;
            }
            else
            {
                thisAgeNorm = (double)(this.AgeGroup - minAge) / (maxAge - minAge);
                userAgeNorm = (double)(user.AgeGroup - minAge) / (maxAge - minAge);
            }

            return (double)(Math.Abs(thisAgeNorm - userAgeNorm) + Math.Abs(Convert.ToInt32(this.IsMale) - Convert.ToInt32(user.IsMale)) + this.Occupation == user.Occupation ? 0 : 1) / 3;
        }

        public void SetRatingMean()
        {
            if (this.Ratings.Count > 0)
            {
                this.RatingMean = this.Ratings.Values.Sum() / this.Ratings.Count();
            }
            else
                this.RatingMean = 3.5;
        }

        public double PredictRatingByMovies(Movie movie, double allMovieMean)
        {
            if (this.Ratings.Count > 0)
            {
                int minYear = Math.Min(movie.MovieYear, this.Ratings.Keys.Min(m => m.MovieYear));
                int maxYear = Math.Max(movie.MovieYear, this.Ratings.Keys.Max(m => m.MovieYear));

                double sumRatingBySim = 0;
                double sumSim = 0;
                foreach (KeyValuePair<Movie, double> movieRating in this.Ratings)
                {
                    double movieSim = movie.GetMovieSim(movieRating.Key, minYear, maxYear);
                    sumRatingBySim += movieRating.Value * movieSim;
                    sumSim += movieSim;
                }

                return sumSim != 0 ? sumRatingBySim / sumSim : allMovieMean;
            }
            else
                return allMovieMean;
        }
    }
}
