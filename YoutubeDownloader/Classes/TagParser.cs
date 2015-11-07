using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeDownloader
{
    public struct SuggestedTagsPackage
    {
        public List<string> titles;
        public List<string> authors;
        public string suggestedTitle;
        public string suggestedAuthor;
        public bool foundStuffInDetails;
    }

    

    class TagParser
    {
        public static readonly char[] separators = { ':', ';','-','|','=' };
        // From https://gist.github.com/martindale/6174408
        public static readonly string[] illegals = { "[dubstep]", "[electro]", "[edm]", "[house music]",
                  "[glitch hop]", "[video]", "[official video]", "(official video)",
                  "(official music video)", "(lyrics)",
                  "[ official video ]", "[official music video]", "[free download]",
                  "[free dl]", "( 1080p )", "(with lyrics)", "(high res / official video)",
                  "(music video)", "[music video]", "[hd]", "(hd)", "[hq]", "(hq)",
                  "(original mix)", "[original mix]", "[lyrics]", "[free]", "[trap]",
                  "[monstercat release]", "[monstercat freebie]", "[monstercat]",
                  "[edm.com premeire]", "[edm.com exclusive]", "[enm release]",
                  "[free download!]", "[monstercat free release]" };
        public static readonly string[] greatIllegals = { "http" };


        public static SuggestedTagsPackage AttemptToParseTags(string title,string details,string tags,string vidAuthor)
        {
            title = Purify(title);
            details = Purify(details);
            //Debug.WriteLine(details);


            List<string> titles = new List<string>();
            List<string> authors = new List<string>();
            string suggTitle ="", suggArtist = "";

            // Check known rules

            //// Check keywords in details
            //bool prevSuspicious = false;
            //foreach (string word in details.Split(' '))
            //{
            //    bool? IsTitle = IsTitleWord(word);
            //    if (IsTitle == true) // Yeah it is!
            //    {

            //    }
            //    else if(IsTitle == false) // Nope , nothing like that.
            //    {

            //    }
            //    else if(IsTitle == null) // It may be , gotta be careful...
            //    {
            //        prevSuspicious = true;
            //    }
            //}
            // Do the same with title - separate strings
            foreach (char separator in separators)
            {
                var lines = title.Split(separator);
                if (lines.Length >= 2)
                    foreach (string part in lines)
                    {
                        if(IsTitlePartValid(part,vidAuthor)) titles.Add(part);
                    }
            }
            

            // Separate lines in details
            foreach (string line in details.Split("\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries))
            {
                foreach(char separator in separators)
                {
                    var lines = line.Split(separator);
                    bool foundKeywordTitle = false,foundKeywordArtist = false;
                    if (lines.Length >= 2)
                        foreach (string part in lines)
                        {
                            if (!foundKeywordTitle && !foundKeywordArtist)
                            {
                                foundKeywordArtist = IsArtistWord(part);
                                if (!foundKeywordArtist)
                                    foundKeywordTitle = (bool)IsTitleWord(part);
                            }

                            if (foundKeywordTitle && IsTitlePartValid(part,vidAuthor)) titles.Add(part);
                            if (foundKeywordArtist) authors.Add(part);
                        }
                }
            }



            
            for (int i = 0; i < titles.Count; i++)
            {
                if (titles[i].Length < 4 || string.IsNullOrWhiteSpace(titles[i]) || !ContainsLetters(titles[i]))
                    titles.RemoveAt(i);
                else
                {
                    titles[i] = titles[i].Trim();
                    if (title.Contains(titles[i]))
                        suggTitle = titles[i];

                }
            }
            for (int i = 0; i < authors.Count; i++)
            {
                authors[i] = authors[i].Trim();
                if (title.Contains(authors[i]))
                    suggArtist = authors[i];
            }
            authors.Add(vidAuthor);
            authors = authors.Distinct().ToList();
            titles = titles.Distinct().ToList(); // clear duplicates
            SuggestedTagsPackage pkg = new SuggestedTagsPackage();
            pkg.titles = titles;
            pkg.authors = authors;
            pkg.suggestedTitle = suggTitle;
            pkg.suggestedAuthor = suggArtist;

            return pkg;
        } 

        private static bool ContainsLetters(string part)
        {
            foreach (var item in part)
            {
                if(item > '9')
                    return true;
            }

            return false;
        }

        private static bool? IsTitleWord(string word)
        {
            word = word.ToLower();

            if (word.Contains("title") || word.Contains("track"))
                return true;
            else if (word.Contains("name") || word.Contains("song"))
                return null;
            return false;
        }

        private static bool IsArtistWord(string word)
        {
            word = word.ToLower();

            if (word.Contains("artist") || word.Contains("composer") || word.Contains("performer") || word.Contains("author") || word.Contains("creator")) return true;
            return false;
        }

        private static bool ContainsAuthor(string part,string author)
        {
            return part.Contains(author) || part.Contains(author.Replace(" ", ""));
        }

        private static bool IsTitlePartValid(string part,string author)
        {
            return part.Length < 50 && !ContainsAuthor(part,author) && !ContainsIllegals(part);
        }

        private static string Purify(string part)
        {
            StringBuilder details = new StringBuilder();
            foreach (string illegal in greatIllegals)
            {
                foreach (var line in part.Split("\n".ToCharArray(),StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!line.Contains(illegal))
                        details.AppendLine(line);
                }
            }

            return details.ToString();
        }

        private static bool ContainsIllegals(string part)
        {
            foreach (var item in greatIllegals)
            {
                if (part.Contains(item)) return true;
            }
            return false;
        }
    }
}
