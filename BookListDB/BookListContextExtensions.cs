using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    public static class BookListContextExtensions
    {
        private const string BookListPath = "JH Books List.txt";
        static BookApi _bookAPI;
        static Screens _screens;

        public static void AddScreens(Screens screens)
        {
            _screens = screens;
        }
        public static void AddUser(BookListContext context, string firstName, string middleName, string surname,
                                   string password, string userName, string eMail)
        {
            int userId;
            userId = (context.Users == null || context.Users.Count() == 0) ? 1 : context.Users.Max(u => u.UserId) + 1;
            User user = new User
            {
                UserId = userId,
                FirstName = firstName,
                MiddleName = middleName,
                Surname = surname,
                Password = password,
                UserName = userName,
                Email = eMail
            };
            context.Users.Add(user);
            context.SaveChanges();
        }

        private static void AddBookType(BookListContext context, string description, string bk_type, int shopping_list_no)
        {
            int bookTypeId;
            bookTypeId = (context.BookTypes == null || context.BookTypes.Count() == 0) ? 1 : context.BookTypes.Max(bt => bt.BookTypeId) + 1;
            BookType bookType = new BookType
            {
                BookTypeId = bookTypeId,
                Description = description,
                BK_TYPE = bk_type,
                ShoppingListNo = shopping_list_no
            };
            context.BookTypes.Add(bookType);

            context.SaveChanges();
        }

        private static int AddBook(BookListContext context, string search_string,
                                   string user_string, string book_type_string, bool read)
        {
            int user_id, book_type_id;
            string processed_search_string;

            Tuple<int?, List<Book>, List<List<Author>>> searchResult;

            _bookAPI = new BookApi("AIzaSyDwn5O8bqUr5peb9duTYNVOd8hznN47XR0");

            processed_search_string = RemoveInParentheses(search_string);
            searchResult = _bookAPI.Search(processed_search_string, 0, 1);

            /*            Logger.OutputError("Book 0 - Title \'" + searchResult.Item2[0].Title + "\'");
                        Logger.OutputError("Book 1 - Title \'" + searchResult.Item2[1].Title + "\'");
                        Console.ReadKey();*/


            if (searchResult.Item1 == null)
            {
                // Error already printed
                return (-1);
            }

            var users = context.Users.Where(u => u.UserName == user_string);
            if (users == null)
            {
                Logger.OutputError("User {0} does not exist.", user_string);
                return (-1);
            }
            user_id = users.FirstOrDefault().UserId;

            if (book_type_string.Contains("#"))
            {
                // is a BT_SHOPPING_LIST

                var book_type_parts = book_type_string.Split('#');

                if (book_type_parts[0] != "BT_SHOPPING_LIST")
                {
                    Logger.OutputError("Book Type {0} has # but isn't BT_SHOPPING_LIST.", book_type_string);
                    return (-1);
                }

                bool success;
                success = Int32.TryParse(book_type_parts[1], out int shoppingListNo);
                if (!success)
                {
                    Logger.OutputError("Book Type {0} has # but non-numeric after it.", book_type_string);
                    return (-1);
                }

                var bookTypes = context.BookTypes.Where(bt => (bt.BK_TYPE == book_type_parts[0] &&
                                                               bt.ShoppingListNo == shoppingListNo));
                if (bookTypes == null || bookTypes.Count(bt => bt.BookTypeId != 0) == 0)
                {
                    AddBookType(context, "Amazon Shopping List " + shoppingListNo.ToString(), "BT_SHOPPING_LIST", shoppingListNo);
                    bookTypes = context.BookTypes.Where(bt => (bt.BK_TYPE == book_type_parts[0]) &&
                                                              (bt.ShoppingListNo == shoppingListNo));
                }
                book_type_id = bookTypes.FirstOrDefault().BookTypeId;
            }
            else
            {
                var bookTypes = context.BookTypes.Where(bt => bt.BK_TYPE == book_type_string);
                if (bookTypes == null)
                {
                    Logger.OutputError("Book Type {0} does not exist.", book_type_string);
                    return (-1);
                }
                book_type_id = bookTypes.FirstOrDefault().BookTypeId;
            }

            int bookId;
            bookId = (context.Books == null || context.Books.Count() == 0) ? 1 : context.Books.Max(b => b.BookId) + 1;
            Book book = new Book
            {
                BookId = bookId,
                GoogleId = searchResult.Item2[0].GoogleId,
                Title = searchResult.Item2[0].Title,
                //                Authors = searchResult.Item2[0].Authors,
                SubTitle = searchResult.Item2[0].SubTitle,
                Description = searchResult.Item2[0].Description,
                PageCount = searchResult.Item2[0].PageCount,
                PrintType = searchResult.Item2[0].PrintType,
                PublishedDate = searchResult.Item2[0].PublishedDate,
                Publisher = searchResult.Item2[0].Publisher,
                SmallThumbNail = searchResult.Item2[0].SmallThumbNail,
                ThumbNail = searchResult.Item2[0].ThumbNail,
                Read = read,
                UserId = user_id,
                BookTypeId = book_type_id
            };
            if (context.Books.Where(b => b.GoogleId.Equals(book.GoogleId) &&
                                         b.BookTypeId == book.BookTypeId &&
                                         b.UserId == book.UserId).Count() != 0)
            {
                Logger.OutputError("Book Title {0} already exists for this user and book type.", book.Title);
                foreach (Author authorName in searchResult.Item3[0])
                    Logger.OutputError("Book Author {0}", authorName.Name);
                return (-1);
            }

            context.Books.Add(book);
            context.SaveChanges();

            _screens.RefreshScreenIds();

            foreach (Author authorName in searchResult.Item3[0])
                AddAuthor(context, book.BookId, authorName.Name);

            Console.Write(".({0} of {1})", book.BookId, _screens.StartCmdNoBooks + _screens.InputFileLength);
            return (book.BookId);
        }
        private static string RemoveInParentheses(string aString)
        {
            string retString;
            int rParPos, lParPos;

            retString = aString;
            lParPos = aString.IndexOf('(');
            rParPos = aString.LastIndexOf(')');

            if ((lParPos == -1) || (lParPos == -1))
                return (retString);

            if (lParPos > rParPos)
                return (retString);

            if (lParPos == 0)
                retString = "";
            else
                retString = aString.Substring(0, lParPos - 1);

            if (rParPos != aString.Length - 1)
                retString += aString.Substring(rParPos + 1);

            return (retString);
        }
        private static void AddAuthor(BookListContext context, int bookId, string authorName)
        {

            int authorId;
            authorId = (context.Authors == null || context.Authors.Count() == 0) ? 1 : context.Authors.Max(a => a.AuthorId) + 1;

            Author author = new Author
            {
                AuthorId = authorId,
                BookId = bookId,
                Name = authorName
            };

            context.Authors.Add(author);
            context.SaveChanges();

        }
        private static int AddBook(BookListContext context, string search_string,
                                   string user_string, string book_type_string, bool read, string tagValue)
        {
            int bookId;
            string[] tagValues;
            int tagBookId;

            bookId = AddBook(context, search_string, user_string, book_type_string, read);
            if (bookId != -1)
            {
                if (tagValue == "<tag-place-holder>")
                {
                    List<string> AuthorNames = GetAuthors(context, bookId);
                    if (AuthorNames.Count > 0)
                    {
                        tagBookId = GetFirstBookForAuthor(context, AuthorNames[0]);

                        List<string> AuthorTags = GetAuthorTags(context, tagBookId);

                        foreach (string tv in AuthorTags)
                        {
                            if (tv.Length > 0)
                            {
                                AddTag(context, bookId, tv);
                            }
                        }
                    }
                }
                else
                {
                    tagValues = tagValue.Split(';');
                    if (tagValues.Length > 0)
                    {
                        foreach (string tv in tagValues)
                        {
                            if (tv.Length > 0)
                            {
                                AddTag(context, bookId, tv);
                            }
                        }

                    }

                }
            }
            return (bookId);
        }
        public static int AddTag(BookListContext context, int bookId, string tagValue)
        {
            int tagId;
            tagId = (context.Tags == null || context.Tags.Count() == 0) ? 1 : context.Tags.Max(t => t.TagId) + 1;
            Tag tag = new Tag
            {
                TagId = tagId,
                BookId = bookId,
                Value = tagValue
            };

            context.Tags.Add(tag);
            context.SaveChanges();

            return (tagId);
        }
        public static List<string> GetAuthorTags(BookListContext context, int tagBookId)
        {
            List<string> AuthorTags = new List<string>();

            var tags = context.Tags.Where(t => t.BookId == tagBookId);
            foreach (Tag tag in tags)
            {
                AuthorTags.Add(tag.Value);
            }

            return (AuthorTags);
        }
        public static int GetFirstBookForAuthor(BookListContext context, string AuthorName)
        {
            return (context.Authors.FirstOrDefault(a => a.Name == AuthorName).BookId);
        }
        public static List<string> GetAuthors(BookListContext context, int bookId)
        {
            List<string> AuthorNames = new List<string>();

            foreach (Author author in context.Authors.Where(a => a.BookId == bookId))
            {
                AuthorNames.Add(author.Name);
            }

            return (AuthorNames);
        }
        public static int AddBook(BookListContext context, string csvPars)
        {
            string[] splitPars;
            bool success;
            int newId = -1;

            splitPars = csvPars.Split(',');
            success = bool.TryParse(splitPars[3], out bool read);
            if (!success)
            {
                Logger.OutputError("Boolean value read not valid.");
                return(newId);
            }
            try
            {
                newId = AddBook(context, splitPars[0], splitPars[1], splitPars[2], read, splitPars[4]);
            }
            catch(Exception ex)
            {
                Logger.OutputError("Error adding a book.");
                Logger.OutputError(ex.Message);
                Logger.OutputError(csvPars);
                return (newId);
            }
            return (newId);
        }
        public static void EnsureSeedDataForContext(this BookListContext context)
        {
            if (context.Books.Any())
            {
                return;
            }

            AddUser(context, "Jer", "David", "Hogan", @"UQzA4uH/FRIMX9PkXDXI3Q==", "jerhogan", "jerhogan@hotmail.com");
            AddUser(context, "Peter", "Francis", "Hogan", @"UQzA4uH/FRIMX9PkXDXI3Q==", "peterhogan", "jerhogan@live.ie");
            AddUser(context, "Mary", "Ita", "Hogan", @"UQzA4uH/FRIMX9PkXDXI3Q==", "maryhogan", "jerhogan@live.ie");
            AddUser(context, "Julie", "Catherine", "Hogan", @"UQzA4uH/FRIMX9PkXDXI3Q==", "juliehogan", "jerhogan@live.ie");
            AddUser(context, "Katy", "Mary", "Hogan", @"UQzA4uH/FRIMX9PkXDXI3Q==", "katyhogan", "jerhogan@live.ie");
            AddUser(context, "Con", "Peter", "Hogan", @"UQzA4uH/FRIMX9PkXDXI3Q==", "conhogan", "jerhogan@live.ie");
            AddUser(context, "Anne", "Christine Grania", "Hogan", @"UQzA4uH/FRIMX9PkXDXI3Q==", "annehogan", "jerhogan@live.ie");
            AddUser(context, "Lucy", "Blaithin", "Hogan", @"UQzA4uH/FRIMX9PkXDXI3Q==", "lucyhogan", "jerhogan@live.ie");

            AddBookType(context, "Google Books", "BT_GOOGLE", 0);
            AddBookType(context, "Amazon Kindle", "BT_KINDLE", 0);
            AddBookType(context, "Rakuten Kobo", "BT_KOBO", 0);
            AddBookType(context, "Adobe Digital Editions", "BT_ADOBE", 0);
            AddBookType(context, "Amazon Wish List", "BT_WISH_LIST", 0);
            AddBookType(context, "Paperback Hard Copy", "BT_PAPER", 0);
            AddBookType(context, "Hardback Hard Copy", "BT_HARD", 0);

            if (File.Exists(BookListPath))
            {
                var lines = File.ReadAllLines(BookListPath);
                _screens.InputFileLength = lines.Length;
                for (var i = 1; i < lines.Length; i += 1)
                {
                    AddBook(context, lines[i]);
                }
            }

            Logger.OutputInformation("Max Title Length = " + BookApi.maxTitleLength);
            Logger.OutputInformation("Max Sub Title Length = " + BookApi.maxSubTitleLength);
            Logger.OutputInformation("Max Description Length = " + BookApi.maxDescriptionLength);
            Logger.OutputInformation("Max Thumbnail Length = " + BookApi.maxThumbnailLength);
            Logger.OutputInformation("Max Small Thumbnail Length = " + BookApi.maxSmallThumbnailLength);

        }
        /*
        Department department = new Department
        {
            DepartmentName = "Technology",
            Employees = new List<Employee>
            {
                new Employee() {EmployeeName = "Jack"},
                new Employee() {EmployeeName = "Kim"},
                new Employee() {EmployeeName = "Shen"}
            }
        };

        context.Departments.Add(department);

        Employee employee = new Employee
        {
            EmployeeName = "Akhil Mittal",
            DepartmentId = 1
        };

        context.Employees.Add(employee);
        context.SaveChanges();

        */
    }
}
