using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultGeneratorV2._0
{
    public class Movie
    {
        const int fMovieId = 0;
        const int fMovieTitleYear = 1;
        const int fMovieGenres = 2;
        const int sRatingMin = 1;
        const int sRatingMax = 2;
        const int sRatingMean = 3;
        const int sRatingMedian = 4;

        public string MovieId { get; set; }
        public string Title { get; set; }
        public int MovieYear { get; set; }
        public List<string> Genres { get; set; }
        
        public double RatingMean { get; set; }

        public Dictionary<User, double> Ratings { get; set; }
        public HashSet<string> Tags { get; set; }

        public Dictionary<string, double> SimMovies { get; set; }

        public Movie(string[] splittedLine)
        {
            this.MovieId = splittedLine[fMovieId];
            this.Title = splittedLine[fMovieTitleYear].Split('(')[0].Trim();
            this.MovieYear = int.Parse(splittedLine[fMovieTitleYear].Substring((splittedLine[fMovieTitleYear].LastIndexOf('(') + 1), 4));
            this.Genres = splittedLine[fMovieGenres].Split('|').ToList();

            this.Ratings = new Dictionary<User, double>();
            this.Tags = new HashSet<string>();

            this.SimMovies = new Dictionary<string, double>();
        }

        public double GetMovieSim(Movie movie, int minYear, int maxYear)
        {
            double thisYearNorm;
            double movieYearNorm;
            if (maxYear - minYear == 0)
            {
                thisYearNorm = 1;
                movieYearNorm = 1;
            }
            else
            {
                thisYearNorm = (double)(this.MovieYear - minYear) / (maxYear - minYear);
                movieYearNorm = (double)(movie.MovieYear - minYear) / (maxYear - minYear);
            }

            //int maxTag = this.Tags.Union(movie.Tags).Distinct().Count();
            //int nbSameTag = this.Tags.Where(t => movie.Tags.Contains(t)).Union(movie.Tags.Where(t => this.Tags.Contains(t))).Distinct().Count();
            //double simTag;
            //if (maxTag > 0)
            //    simTag = (double)(maxTag - nbSameTag) / maxTag;
            //else
            //    simTag = 1;

            double simBase = Math.Abs(thisYearNorm - movieYearNorm)/* + simTag*/;
            int fieldCount = 1;
            //int fieldCount = 2;
            foreach (string genre in this.Genres.Union(movie.Genres).Distinct().ToList())
            {
                if (!this.Genres.Contains(genre) || !movie.Genres.Contains(genre))
                    simBase += 1;
                fieldCount++;
            }
            double sim = simBase / fieldCount;

            return sim;
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

        public double PredictRatingByUsers(User user, double allUserMean)
        {
            if (this.Ratings.Count > 0)
            {
                int minAge = Math.Min(user.AgeGroup, this.Ratings.Keys.Min(m => m.AgeGroup));
                int maxAge = Math.Max(user.AgeGroup, this.Ratings.Keys.Max(m => m.AgeGroup));

                double sumRatingBySim = 0;
                double sumSim = 0;
                foreach (KeyValuePair<User, double> userRating in this.Ratings)
                {
                    double userSim = user.GetUserSim(userRating.Key, minAge, maxAge);
                    sumRatingBySim += userRating.Value * userSim;
                    sumSim += userSim;
                }

                return sumSim != 0 ? sumRatingBySim / sumSim : allUserMean;
            }
            else
                return allUserMean;
        }
    }
}
