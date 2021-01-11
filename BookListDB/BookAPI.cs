/*
 * using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleBooksAPIList
{
    class BookAPI
    {
    }
}
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Books.v1;
using Google.Apis.Services;

namespace BookListDB
{
    public class BookApi
    {
        private readonly BooksService _booksService;
        public BookApi(string apiKey)
        {
            _booksService = new BooksService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });
        }

        private string TruncateString(string str, int length)
        {
           if (str != null)
            {
                if (length < str.Length)
                {
                    Logger.OutputInformation("String \'" + str + "\' Length " + length + " < string length " + str.Length);
                }
            }
            return (str == null ? "" : str.Substring(0, Math.Min(length, str.Length)));
        }

        public static int maxTitleLength = 0;
        public static int maxSubTitleLength = 0;
        public static int maxDescriptionLength = 0;
        public static int maxThumbnailLength = 0;
        public static int maxSmallThumbnailLength = 0;
        public Tuple<int?, List<Book>, List<List<Author>>> Search(string query, int offset, int count)
        {
            var listquery = _booksService.Volumes.List(query);
            listquery.MaxResults = count;
            listquery.StartIndex = offset;
            var res = listquery.Execute();

            if (res.Items == null)
            {
                Logger.OutputError("Query '" + query + "' is unsuccessful in Google Books API");

                return new Tuple<int?, List<Book>, List<List<Author>>>(null, null, null);
            }

            maxTitleLength = Math.Max(maxTitleLength, 
                             res.Items.Max(b => b.VolumeInfo.Title == null ? 0 : b.VolumeInfo.Title.Length));
            maxSubTitleLength = Math.Max(maxSubTitleLength, 
                                res.Items.Max(b => b.VolumeInfo.Subtitle == null ? 0: b.VolumeInfo.Subtitle.Length));
            maxDescriptionLength = Math.Max(maxDescriptionLength, res.Items.Max(b => b.VolumeInfo.Description == null ? 0 : b.VolumeInfo.Description.Length));
            maxThumbnailLength = Math.Max(maxThumbnailLength,
                             res.Items.Max(b => b.VolumeInfo.ImageLinks == null ? 0 : 
                                                b.VolumeInfo.ImageLinks.Thumbnail.Length));
            maxSmallThumbnailLength = Math.Max(maxThumbnailLength,
                             res.Items.Max(b => b.VolumeInfo.ImageLinks == null ? 0 :
                                                b.VolumeInfo.ImageLinks.Thumbnail.Length));
            var books = res.Items.Select(b => new Book
            {
                GoogleId = b.Id,
                Title = TruncateString(b.VolumeInfo.Title, 300),
//                Authors = b.VolumeInfo.Authors == null ? "" : b.VolumeInfo.Authors[0],
                SubTitle = TruncateString(b.VolumeInfo.Subtitle, 200),
                Description = TruncateString(b.VolumeInfo.Description, 10000),
                PageCount = b.VolumeInfo.PageCount == null ? 0 : (int)b.VolumeInfo.PageCount,
                PrintType = b.VolumeInfo.PrintType,
                PublishedDate = b.VolumeInfo.PublishedDate ?? "",
                Publisher = b.VolumeInfo.Publisher ?? "",
                SmallThumbNail = (b.VolumeInfo.ImageLinks == null) ? "" : TruncateString(b.VolumeInfo.ImageLinks.SmallThumbnail, 200),
                ThumbNail = (b.VolumeInfo.ImageLinks == null) ? "" : TruncateString(b.VolumeInfo.ImageLinks.Thumbnail, 200)
            }).ToList();
            var listAuthors = new List<List<Author>>();
            for (var i = 0; i < books.Count; ++i)
            {
                var authorList = new List<Author>();
                if (res.Items[i].VolumeInfo.Authors != null)
                {
                    for (var j = 0; j < res.Items[i].VolumeInfo.Authors.Count; j++)
                    {
                        Author author = new Author()
                        {
                            Name = res.Items[i].VolumeInfo.Authors[j]
                        };
                        authorList.Add(author);
                    }
                }
                listAuthors.Add(authorList);
            }

            return new Tuple<int?, List<Book>, List<List<Author>>> (res.TotalItems, books, listAuthors);
        }
    }
}