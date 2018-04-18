using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultGeneratorV2._0
{
    class Program
    {
        static void Main(string[] args)
        {
            const int rUserIdCol = 0;
            const int rMovieIdCol = 1;
            const int rRatingCol = 2;

            bool fr = ConfigurationManager.AppSettings["language"] == "fr";
            string inputFile = ConfigurationManager.AppSettings["inputFile"];
            string userFile = ConfigurationManager.AppSettings["userFile"];
            string movieFile = ConfigurationManager.AppSettings["movieFile"];
            string movieFile2 = ConfigurationManager.AppSettings["movieFile2"];
            string ratingFile = ConfigurationManager.AppSettings["ratingFile"];
            string movieTagFile = ConfigurationManager.AppSettings["movieTagFile"];
            string outputFile = ConfigurationManager.AppSettings["outputFile"];

            Dictionary<string, Movie> movieList = new Dictionary<string, Movie>();
            Dictionary<string, User> userList = new Dictionary<string, User>();

            Console.WriteLine("Reading all users...");
            int userCount = 0;
            using (StreamReader userStream = new StreamReader(userFile))
            {
                string userLine;
                while ((userLine = userStream.ReadLine()) != null)
                {
                    string[] splittedUserLine = userLine.Split(new string[] { "::" }, StringSplitOptions.None);
                    
                    userList.Add(splittedUserLine[0], new User(splittedUserLine));
                    userCount++;
                }
            }
            Console.WriteLine(string.Format("{0} users read.", userCount));

            Console.WriteLine("Reading all movies...");
            bool firstLine = true;
            int movieCount = 0;
            using (StreamReader movieStream = new StreamReader(movieFile))
            {
                string movieLine;
                while ((movieLine = movieStream.ReadLine()) != null)
                {
                    if (firstLine)
                        firstLine = false;
                    else
                    {
                        movieLine = movieLine.Replace(", ", " ");
                        movieLine = movieLine.Replace(",000", ".000");
                        string[] splittedMovieLine = movieLine.Split(',');

                        movieList.Add(splittedMovieLine[0], new Movie(splittedMovieLine));
                        movieCount++;
                    }
                }
            }

            using (StreamReader movieStream = new StreamReader(movieFile2))
            {
                string movieLine;
                while ((movieLine = movieStream.ReadLine()) != null)
                {
                    string[] splittedMovieLine = movieLine.Split(new string[] { "::" }, StringSplitOptions.None);
                    if (!movieList.ContainsKey(splittedMovieLine[0]))
                    {
                        movieList.Add(splittedMovieLine[0], new Movie(splittedMovieLine));
                        movieCount++;
                    }
                }
            }
            Console.WriteLine(string.Format("{0} movies read.", movieCount));
            
            Console.WriteLine("Reading all ratings of read users and movies...");
            int ratingCount = 0;
            firstLine = true;
            using (StreamReader stream = new StreamReader(ratingFile))
            {
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    if (firstLine)
                        firstLine = false;
                    else
                    {
                        string[] splittedLine = line.Split(',');

                        string userId = splittedLine[rUserIdCol];
                        string movieId = splittedLine[rMovieIdCol];

                        if (userList.ContainsKey(userId) && movieList.ContainsKey(movieId))
                        {
                            double rating = fr ? double.Parse(splittedLine[rRatingCol].Replace('.', ',')) : double.Parse(splittedLine[rRatingCol]);
                            userList[userId].Ratings.Add(movieList[movieId], rating);
                            movieList[movieId].Ratings.Add(userList[userId], rating);
                            ratingCount++;
                        }
                    }
                }
            }
            Console.WriteLine(string.Format("{0} ratings read.", ratingCount));
            
            Console.WriteLine("Calculating users rating mean and movies rating mean...");
            foreach (User user in userList.Values)
            {
                user.SetRatingMean();
            }
            double allUserRatingMean = userList.Values.Sum(u => u.RatingMean) / userList.Count();

            foreach(Movie movie in movieList.Values)
            {
                movie.SetRatingMean();
            }
            double allMovieRatingMean = movieList.Values.Sum(m => m.RatingMean) / movieList.Count();

            Console.WriteLine("Reading all tags for each movies...");
            firstLine = true;
            using (StreamReader stream = new StreamReader(movieTagFile))
            {
                string tagLine;
                while ((tagLine = stream.ReadLine()) != null)
                {
                    if (firstLine)
                        firstLine = false;
                    else
                    {
                        string[] splittedTagLine = tagLine.Split(',');
                        
                        string movieId = splittedTagLine[0];
                        string tagId = splittedTagLine[1];
                        double relevance = fr ? double.Parse(splittedTagLine[2].Replace('.', ',')) : double.Parse(splittedTagLine[2]);

                        if (movieList.ContainsKey(movieId) && relevance > 0.5)
                        {
                            movieList[movieId].Tags.Add(tagId);
                        }
                    }
                }
            }

            //Console.WriteLine("Reading all tag scores for each users...");
            //using (StreamReader stream = new StreamReader(tagScoresFile))
            //{
            //    string tagLine;
            //    while ((tagLine = stream.ReadLine()) != null)
            //    {
            //        if (firstLine)
            //            firstLine = false;
            //        else
            //        {
            //            string[] splittedTagLine = tagLine.Split(',');

            //            string userId = splittedTagLine[0];
            //            string tagId = splittedTagLine[1];
            //            double score = fr ? double.Parse(splittedTagLine[2].Replace('.', ',')) : double.Parse(splittedTagLine[2]);

            //            if (userList.ContainsKey(userId))
            //            {
            //                userList[userId].Tags.Add(tagId, score);
            //            }
            //        }
            //    }
            //}

            Console.WriteLine("Predicting ratings and writing results");
            int ignoredLineCount = 0;
            int predictedRatingCount = 0;
            using (StreamWriter outputFileStream = new StreamWriter(outputFile))
            {
                outputFileStream.WriteLine("user,rating,id");
                firstLine = true;
                using (StreamReader stream = new StreamReader(inputFile))
                {
                    string line;

                    while ((line = stream.ReadLine()) != null)
                    {
                        if (firstLine)
                            firstLine = false;
                        else
                        {
                            string[] splittedLine = line.Split(',');

                            string userId = splittedLine[0];
                            //string movieId = splittedLine[1];
                            string movieId = splittedLine[2].Split('_')[1];

                            if (userList.ContainsKey(userId) && movieList.ContainsKey(movieId))
                            {
                                double predictedRatingByMovies = userList[userId].PredictRatingByMovies(movieList[movieId], allMovieRatingMean);
                                double predictedRatingByUsers = movieList[movieId].PredictRatingByUsers(userList[userId], allUserRatingMean);

                                double predictedRating = (predictedRatingByMovies + predictedRatingByUsers) / 2;
                                predictedRatingCount++;

                                outputFileStream.WriteLine(string.Format("{0},{1},{2}", userId, /*movieId, */Math.Round(predictedRating,1), userId + "_" + movieId));
                            }
                            else
                                ignoredLineCount++;
                        }
                    }
                }
            }
            Console.WriteLine(string.Format("{0} rating predicted, {1} rating ignored", predictedRatingCount, ignoredLineCount));
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
