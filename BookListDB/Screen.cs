using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BookListDB
{
    public enum FieldType
    {
        fNone, fAll, fAuthor, fIndex, fTag, fTitle, fUser, fBookType, fRead
    };
    public enum OrderType
    {
        Ascending, Descending
    };
    public class Screen
    {
        public int topScreenId;
        public int bottomScreenId;
        public List<int> screenIds;
        public const int MAX_BOOKS_PER_SCREEN = 10;
        public List<Book> books;
        public Command command;
        public BookListContext _context = null;
        const string JHOutputBookListKindleFile = "JH Book List File Kindle Output.txt";
        const string JHOutputBookListKoboFile = "JH Book List File Kobo Output.txt";
        const string JHOutputBookListGoodReadsFile = "JH Book List File Good Reads Output.txt";
        const string JHOutputBookListWishListFile = "JH Book List File Wish List Output.txt";
        public int inputFileLength = 0;
        public int startCmdNoBooks = 0;

        public Screen()
        {
            topScreenId = bottomScreenId = -1;
            screenIds = new List<int>();
            books = new List<Book>();
            command = null;
        }
        private string FixedLength(string input, int length)
        {
            if (input.Length > length)
                return input.Substring(0, length);
            else
                return input.PadRight(length, ' ');
        }

        public int JumpTo(int jumpIndex)
        {
            topScreenId = jumpIndex;
            bottomScreenId = Math.Min(topScreenId + MAX_BOOKS_PER_SCREEN - 1, screenIds.Count - 1);
            return (jumpIndex);
        }

        public bool EmptyScreen()
        {
            bool empty;

            empty = screenIds == null;

            if (!empty)
                empty = screenIds.Count == 0;

            if (!empty)
                empty = topScreenId < 0;

            if (!empty)
                empty = bottomScreenId < 0;

            return (empty);
        }
        public void RefreshBookIdsDelete(int delIndex)
        {
            Book book = books.FirstOrDefault(b => b.BookId == delIndex);
            if (book == null)
            {
                Logger.OutputError("Book {0} does not exist on Inner Delete", delIndex.ToString());
                return;
            }

            books.RemoveAll(b => b.BookId == delIndex);
        }
        public void RefreshBookIdsCreate(int crIndex)
        {
            Book book = _context.Books.FirstOrDefault(b => b.BookId == crIndex);
            if (book == null)
            {
                Logger.OutputError("Book {0} does not exist on Create", crIndex.ToString());
                return;
            }
            books.Add(book);
        }
        public void RefreshScreenIds()
        {
            screenIds = new List<int>();
            if (books == null)
            {
                Logger.OutputError("Please login first");
            }
            else
            {
                foreach (Book book in books)
                {
                    screenIds.Add(book.BookId);
                }
            }
        }

        public void SetContext(BookListContext context)
        {
            _context = context;
        }
        public void ReadScreenIds()
        {
            RefreshScreenIds();
            topScreenId = 0;
            bottomScreenId = Math.Min(topScreenId + MAX_BOOKS_PER_SCREEN - 1, screenIds.Count - 1);
        }
        public void JumpToBottom(int newIndex)
        {
            bottomScreenId = newIndex;
            if (bottomScreenId < screenIds.Count)
                bottomScreenId = Math.Min(screenIds.Count - 1, MAX_BOOKS_PER_SCREEN - 1);
            topScreenId = Math.Max(bottomScreenId - MAX_BOOKS_PER_SCREEN + 1, 0);
        }
        public void DownOne()
        {
            if (bottomScreenId != screenIds.Count - 1)
            {
                topScreenId = Math.Min(topScreenId + 1, screenIds.Count - 1);
                bottomScreenId = Math.Min(topScreenId + MAX_BOOKS_PER_SCREEN - 1, screenIds.Count - 1);
            }
        }
        public void DownPage()
        {
            if (bottomScreenId != screenIds.Count - 1)
            {
                topScreenId = Math.Min(bottomScreenId + 1, screenIds.Count - 1);
                bottomScreenId = Math.Min(topScreenId + MAX_BOOKS_PER_SCREEN - 1, screenIds.Count - 1);
                if (bottomScreenId - topScreenId + 1 < MAX_BOOKS_PER_SCREEN)
                {
                    bottomScreenId = screenIds.Count - 1;
                    topScreenId = Math.Max(bottomScreenId - MAX_BOOKS_PER_SCREEN + 1, 0);
                }
            }
        }
        public void UpOne()
        {
            topScreenId = Math.Max(topScreenId - 1, 0);
            bottomScreenId = Math.Min(topScreenId + MAX_BOOKS_PER_SCREEN - 1, screenIds.Count - 1);
        }
        public void UpPage()
        {
            topScreenId = Math.Max(topScreenId - MAX_BOOKS_PER_SCREEN, 0);
            bottomScreenId = Math.Min(topScreenId + MAX_BOOKS_PER_SCREEN - 1, screenIds.Count - 1);
        }
        public void Home()
        {
            topScreenId = 0;
            bottomScreenId = Math.Min(topScreenId + MAX_BOOKS_PER_SCREEN - 1, screenIds.Count - 1);
        }
        public void End()
        {
            bottomScreenId = screenIds.Count - 1;
            topScreenId = Math.Max(bottomScreenId - MAX_BOOKS_PER_SCREEN + 1, 0);
        }
        public void Initialise(BookListContext context)
        {
            books = context.Books.Where(b => b.UserId == Login.currentUserId).ToList();
            _context = context;
        }
        public void PopulateBooks(List<Book> oldBooks, BookListContext oldContext)
        {
            books = new List<Book>();
            foreach (Book book in oldBooks)
            {
                books.Add(book);
            }
            _context = oldContext;
        }
        public void Order(FieldType field, OrderType order, int topBookId, int bottomBookId)
        {
            Book book;
            List<Book> initialBooks = new List<Book>(books);
            int bookRowNo;

            switch (field)
            {
                case FieldType.fAuthor:
                    List<BookAuthorValue> bookAuthors;

                    bookAuthors = GetAuthorValues();

                    if (order == OrderType.Ascending)
                        bookAuthors = bookAuthors.OrderBy(ba => ba.AuthorNames).ToList();
                    else
                        bookAuthors = bookAuthors.OrderByDescending(ba => ba.AuthorNames).ToList();

                    books = new List<Book>();
                    foreach (BookAuthorValue bkAuth in bookAuthors)
                    {
                        book = initialBooks.Where(b => b.UserId == Login.currentUserId).FirstOrDefault(b => b.BookId == bkAuth.BookId);
                        if (book != null)
                            books.Add(book);
                    }
                    break;

                case FieldType.fTag:
                    List<BookTag> bookTags;

                    bookTags = GetTagValues();

                    if (order == OrderType.Ascending)
                        bookTags = bookTags.OrderBy(bt => bt.TagsValue).ToList();
                    else
                        bookTags = bookTags.OrderByDescending(bt => bt.TagsValue).ToList();

                    books = new List<Book>();
                    foreach (BookTag bkTag in bookTags)
                    {
                        book = initialBooks.Where(b => b.UserId == Login.currentUserId).FirstOrDefault(b => b.BookId == bkTag.BookId);
                        if (book != null)
                            books.Add(book);
                    }
                    break;

                case FieldType.fTitle:
                    if (order == OrderType.Ascending)
                        books = books.OrderBy(b => b.Title)
                                     .Where(b => b.UserId == Login.currentUserId).ToList();
                    else
                        books = books.OrderByDescending(b => b.Title)
                                     .Where(b => b.UserId == Login.currentUserId).ToList();
                    break;

                case FieldType.fIndex:
                    if (order == OrderType.Ascending)
                        books = books.OrderBy(b => b.BookId)
                                     .Where(b => b.UserId == Login.currentUserId).ToList();
                    else
                        books = books.OrderByDescending(b => b.BookId)
                                     .Where(b => b.UserId == Login.currentUserId).ToList();
                    break;

                case FieldType.fBookType:
                    List<BookTypeEntry> bookTypeEntryList = FillBookEntryList();
                    if (order == OrderType.Ascending)
                        bookTypeEntryList = bookTypeEntryList
                            .Where(b => b.userId == Login.currentUserId)
                            .OrderBy(b => b.BK_TYPE).ToList();
                    else
                        bookTypeEntryList = bookTypeEntryList
                            .Where(b => b.userId == Login.currentUserId)
                            .OrderByDescending(b => b.BK_TYPE).ToList();
                    SendBookTypeEntryListToBooks(bookTypeEntryList);
                    break;

                case FieldType.fRead:
                    if (order == OrderType.Ascending)
                        books = books.OrderBy(b => b.Read)
                                     .Where(b => b.UserId == Login.currentUserId).ToList();
                    else
                        books = books.OrderByDescending(b => b.Read)
                                     .Where(b => b.UserId == Login.currentUserId).ToList();
                    break;
            }

            ReadScreenIds();
            if (order == OrderType.Ascending)
            {
                bookRowNo = GetRowNumber(topBookId);
                JumpTo(bookRowNo);
            }
            else
            {
                bookRowNo = GetRowNumber(bottomBookId);
                JumpToBottom(bookRowNo);
            }
        }

        public int GetRowNumber(int bookId)
        {
            int i;

            for (i = 0; i < books.Count; ++i)
            {
                if (books[i].BookId == bookId)
                    return (i);
            }
            return (0);
        }
        public class BookAuthorValue
        {
            public int BookId;
            public string AuthorNames;
        }
        public List<BookAuthorValue> GetAuthorValues()
        {
            List<BookAuthorValue> bookAuthorValues = new List<BookAuthorValue>();
            BookAuthorValue bookAuthorNamesValue;
            List<Author> relevantAuthors = new List<Author>();

            foreach (Author author in _context.Authors)
            {
                if (books.FirstOrDefault(b => b.BookId == author.BookId) != null)
                {
                    relevantAuthors.Add(author);
                }
            }

            foreach (Author author in relevantAuthors)
            {
                bookAuthorNamesValue = bookAuthorValues.FirstOrDefault(a => a.BookId == author.BookId);
                if (bookAuthorNamesValue == null)
                {
                    bookAuthorNamesValue = new BookAuthorValue
                    {
                        BookId = author.BookId,
                        AuthorNames = author.Name
                    };
                    bookAuthorValues.Add(bookAuthorNamesValue);
                }
                else
                {
                    bookAuthorNamesValue.AuthorNames += ";" + author.Name;
                }
            }

            return bookAuthorValues;
        }
        public class BookTag
        {
            public int BookId;
            public string TagsValue;
        }
        public List<BookTag> GetTagValues()
        {
            List<BookTag> bookTags = new List<BookTag>();
            BookTag bookTag;
            List<Tag> relevantTags = new List<Tag>();

            foreach (Tag tag in _context.Tags)
            {
                if (books.FirstOrDefault(b => b.BookId == tag.BookId) != null)
                {
                    relevantTags.Add(tag);
                }
            }

            foreach (Tag tag in relevantTags)
            {
                bookTag = bookTags.FirstOrDefault(bt => bt.BookId == tag.BookId);
                if (bookTag == null)
                {
                    bookTag = new BookTag
                    {
                        BookId = tag.BookId,
                        TagsValue = tag.Value
                    };
                    bookTags.Add(bookTag);

                }
                else
                {
                    bookTag.TagsValue += ";" + tag.Value;
                }
            }

            return bookTags;
        }
        public class BookTypeEntry
        {
            public int bookTypeId;
            public int bookId;
            public string BK_TYPE;
            public int userId;
        }
        public List<BookTypeEntry> FillBookEntryList()
        {
            List<BookTypeEntry> bookTypeEntryList = new List<BookTypeEntry>();
            foreach (Book book in books)
            {
                BookTypeEntry btEntry = new BookTypeEntry
                {
                    bookId = book.BookId,
                    bookTypeId = book.BookTypeId,
                    userId = book.UserId
                };
                BookType bookType;

                bookType = _context.BookTypes.FirstOrDefault(bt => bt.BookTypeId == book.BookTypeId);
                btEntry.BK_TYPE = bookType.BK_TYPE;
                bookTypeEntryList.Add(btEntry);
            }

            return (bookTypeEntryList);
        }
        Book GetBookById(List<Book> initialBooks, int bookId)
        {
            return (initialBooks.FirstOrDefault(b => b.BookId == bookId));
        }
        public void SendBookTypeEntryListToBooks(List<BookTypeEntry> bookTypeEntryList)
        {
            List<Book> initialBooks = new List<Book>(books);

            books = new List<Book>();
            foreach (BookTypeEntry bookTypeEntry in bookTypeEntryList)
            {
                Book book = GetBookById(initialBooks, bookTypeEntry.bookId);
                books.Add(book);
            }
        }
        public void Filter(FieldType field, string query)
        {
            List<Book> initialBooks;
            Book book;

            if (EmptyScreen())
                return;

            switch (field)
            {
                case FieldType.fTitle:
                    books = books.Where(b => b.UserId == Login.currentUserId).Where(bt => bt.Title.ToUpper().Contains(query.ToUpper())).ToList();
                    break;

                case FieldType.fAuthor:
                    List<BookAuthorValue> bookAuthors;

                    bookAuthors = GetAuthorValues();

                    bookAuthors = bookAuthors.FindAll(ba => ba.AuthorNames.ToUpper().Contains(query.ToUpper())).ToList();

                    initialBooks = new List<Book>(books);

                    books = new List<Book>();
                    foreach (BookAuthorValue bkAuth in bookAuthors)
                    {
                        book = initialBooks.Where(b => b.UserId == Login.currentUserId).FirstOrDefault(b => b.BookId == bkAuth.BookId);
                        if (book != null)
                            books.Add(book);
                    }
                    break;

                case FieldType.fTag:
                    List<BookTag> bookTags;

                    bookTags = GetTagValues();

                    bookTags = bookTags.FindAll(t => t.TagsValue.ToUpper().Contains(query.ToUpper())).ToList();

                    initialBooks = new List<Book>(books);
                    books = new List<Book>();
                    foreach (BookTag bkTag in bookTags)
                    {
                        book = initialBooks.Where(b => b.UserId == Login.currentUserId).FirstOrDefault(b => b.BookId == bkTag.BookId);
                        if (book != null)
                            books.Add(book);
                    }
                    break;

                case FieldType.fBookType:
                    List<BookTypeEntry> bookTypeEntryList = FillBookEntryList();
                    bookTypeEntryList = bookTypeEntryList.Where(b => b.userId == Login.currentUserId)
                                                         .Where(b => b.BK_TYPE.ToUpper().Contains(query.ToUpper())).ToList();
                    SendBookTypeEntryListToBooks(bookTypeEntryList);
                    break;

                case FieldType.fRead:
                    bool readValue;

                    readValue = query.Length > 0 && query.ToUpper()[0] == 'Y';
                    books = books.Where(b => b.UserId == Login.currentUserId).Where(bt => bt.Read == readValue).ToList();
                    break;

                default:
                    // ignore
                    break;

            }

            ReadScreenIds();
        }
        public void Convert(string inputFile, string bookType)
        {
            if (bookType == "BT_KINDLE")
                ConvertKindle(inputFile);
            else if (bookType == "BT_KOBO")
                ConvertKobo(inputFile);
            else if (bookType == "BT_GOOD_READS")
                ConvertGoodReads(inputFile);
            else if (bookType == "BT_WISH_LIST")
                ConvertWishList(inputFile);
        }
        public void ConvertKindle(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Logger.OutputError("Web Page File {0} does not exist", inputFile);
            }
            else
            {
                int i, j;
                bool delString;
                List<string> linesList;
                string[] outputLines;

                var lines = File.ReadAllLines(inputFile);
                linesList = lines.ToList();

                // Delete all before "AuthorDateCollections"
                delString = false;
                for (i = linesList.Count - 1; i >= 0; --i)
                {
                    if (linesList[i] == "AuthorDateCollections")
                        delString = true;
                    if (delString)
                        linesList.RemoveAt(i);
                }

                // Delete all after "Your recently viewed items and featured recommendations"
                delString = false;
                for (i = 0; i < linesList.Count; ++i)
                {
                    if ((linesList[i] == "Show More") ||
                        (linesList[i] == "Your recently viewed items and featured recommendations"))
                        delString = true;
                    if (delString)
                    {
                        for (j = linesList.Count - 1; j >= i; --j)
                            linesList.RemoveAt(j);
                    }
                }

                linesList.RemoveAll(l => l.Equals("..."));
                linesList.RemoveAll(l => l.Equals("0"));
                linesList.RemoveAll(l => l.Equals("Update Available"));
                linesList.RemoveAll(l => l.Equals(""));

                List<int> samplePos = new List<int>();
                for (i = 0; i < linesList.Count; ++i)
                    if (linesList[i].Equals("Sample"))
                        samplePos.Add(i);
                samplePos.Reverse();
                foreach (int sample in samplePos)
                {
                    linesList.RemoveAt(sample + 1);
                    linesList.RemoveAt(sample);
                    linesList.RemoveAt(sample - 1);
                }

                for (i = 0; i < linesList.Count; ++i)
                {
                    linesList[i] = Regex.Replace(linesList[i], @"\d+\s[A-Z][a-z]+\s\d{4}$", "");
                    linesList[i] = linesList[i].Replace(",", ";");
                }

                const string readTrueString = ",jerhogan,BT_KINDLE,true,American;Fantasy";
                const string readFalseString = ",jerhogan,BT_KINDLE,false,American;Fantasy";
                string readString, newString;
                // handle READ
                // concat strings
                // Add extra parameters (read user from currentUserId)
                i = linesList.Count - 1;
                while (i >= 0)
                {
                    if (linesList[i - 1] == "READ")
                    {
                        newString = linesList[i - 2] + linesList[i];
                        readString = readTrueString;
                        linesList.RemoveRange(i - 1, 2);
                        i -= 2;
                    }
                    else
                    {
                        newString = linesList[i - 1] + linesList[i];
                        readString = readFalseString;
                        linesList.RemoveRange(i - 1, 1);
                        --i;
                    }
                    linesList[i] = newString + readString;
                    --i;
                }

                outputLines = linesList.ToArray();
                File.WriteAllLines(JHOutputBookListKindleFile, outputLines);
            }
        }
        public void ConvertKobo(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Logger.OutputError("Web Page File {0} does not exist", inputFile);
            }
            else
            {
                int i, j;
                bool delString;
                List<string> linesList;
                List<string> outputLinesList;
                string[] outputLines;
                List<int> startNumber = new List<int>();
                List<int> endNumber = new List<int>();
                bool read;

                var lines = File.ReadAllLines(inputFile);
                linesList = lines.ToList();

                // Delete all before " Grid List SHOW: 24"
                delString = false;
                for (i = linesList.Count - 1; i >= 0; --i)
                {
                    if (linesList[i] == " Grid List SHOW: 24")
                        delString = true;
                    if (delString)
                        linesList.RemoveAt(i);
                }

                // Delete all after "All about Rakuten Kobo"
                delString = false;
                for (i = 0; i < linesList.Count; ++i)
                {
                    if (linesList[i] == "All about Rakuten Kobo")
                        delString = true;
                    if (delString)
                    {
                        for (j = linesList.Count - 1; j >= i; --j)
                            linesList.RemoveAt(j);
                    }
                }

                linesList.RemoveAll(l => l.Equals(""));

                // Go through linesList and determine the start and ends of each output line
                for (i = 1; i < linesList.Count; ++i)
                {
                    if (linesList[i] == linesList[i - 1])
                    {
                        if (i - 2 >= 0)
                            endNumber.Add(i - 2);
                        startNumber.Add(i - 1);
                    }
                }
                endNumber.Add(i - 1);

                outputLinesList = new List<string>();
                for (i = 0; i < startNumber.Count; ++i)
                {
                    string outputLine;

                    outputLine = linesList[startNumber[i]];
                    for (j = startNumber[i] + 2; j < endNumber[i]; ++j)
                        outputLine += " " + linesList[j];

                    read = false; // default value
                    int subIndex, finishedSubIndex;
                    subIndex = linesList[endNumber[i]].Length - 6;
                    finishedSubIndex = subIndex - 2;
                    if ((subIndex >= 0) &&
                        (linesList[endNumber[i]].Substring(subIndex).Equals("UNREAD")))
                        read = false;
                    else if ((subIndex >= 0) &&
                            linesList[endNumber[i]].Substring(subIndex).Equals(@"% READ"))
                    {
                        if (subIndex - 2 < 0)
                            read = false;
                        else if ((linesList[endNumber[i]][subIndex - 2] >= '7') &&
                                 (linesList[endNumber[i]][subIndex - 2] <= '9'))
                            read = true;
                        else
                            read = false;
                    }
                    else if ((finishedSubIndex >= 0) &&
                             (linesList[endNumber[i]].Substring(finishedSubIndex).Equals("FINISHED")))
                        read = true;

                    // get username
                    string userName;
                    User user = _context.Users.FirstOrDefault(u => u.UserId == Login.currentUserId);
                    if (user == null)
                        userName = "jerhogan";
                    else
                        userName = user.UserName;

                    string addString;
                    addString = "," + userName + ",BT_KOBO," + read.ToString() + ",American;Fantasy";

                    outputLine = outputLine.Replace(",", ";");
                    outputLine += addString;
                    outputLinesList.Add(outputLine);
                }

                outputLines = outputLinesList.ToArray();
                File.WriteAllLines(JHOutputBookListKoboFile, outputLines);
            }
        }
        public void ConvertGoodReads(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Logger.OutputError("Web Page File {0} does not exist", inputFile);
            }
            else
            {
                int i, j;
                bool delString;
                List<string> linesList;
                List<string> outputLinesList;
                string[] outputLines;
                List<int> startNumber = new List<int>();
                List<int> endNumber = new List<int>();
                bool read;

                var lines = File.ReadAllLines(inputFile);
                linesList = lines.ToList();

                // Delete all before "cover	title	author	avg rating	rating	shelves	date read	date added Down arrow	"
                delString = false;
                for (i = linesList.Count - 1; i >= 0; --i)
                {
                    if (linesList[i] == "cover	title	author	avg rating	rating	shelves	date read	date added Down arrow	")
                        delString = true;
                    if (delString)
                        linesList.RemoveAt(i);
                }

                // Delete all after "per page "
                delString = false;
                for (i = 0; i < linesList.Count; ++i)
                {
                    if (linesList[i] == "per page ")
                        delString = true;
                    if (delString)
                    {
                        for (j = linesList.Count - 1; j >= i; --j)
                            linesList.RemoveAt(j);
                    }
                }

                linesList.RemoveAll(l => l.Equals(""));

                // Remove trailing " *" if there 
                for (i = 1; i < linesList.Count; ++i)
                    if (linesList[i].Substring(linesList[i].Length - 2) == " *")
                        linesList[i] = linesList[i].Substring(0, linesList[i].Length - 2);

                // Go through linesList and determine the start and ends of each output line
                for (i = 1; i < linesList.Count; ++i)
                {
                    if (linesList[i].Replace(" ", string.Empty) ==
                        linesList[i - 1].Replace(" ", string.Empty))
                    {
                        if (i - 2 >= 0)
                            endNumber.Add(i - 2);
                        startNumber.Add(i - 1);
                    }
                }
                endNumber.Add(i - 1);

                outputLinesList = new List<string>();
                for (i = 0; i < startNumber.Count; ++i)
                {
                    string outputLine;

                    outputLine = linesList[startNumber[i]] + " " + linesList[startNumber[i] + 2];

                    read = false; // default value
                    for (j = startNumber[i] + 3; j < endNumber[i]; ++j)
                        if (linesList[j] == "read")
                            read = true;

                    // get username
                    string userName;
                    User user = _context.Users.FirstOrDefault(u => u.UserId == Login.currentUserId);
                    if (user == null)
                        userName = "jerhogan";
                    else
                        userName = user.UserName;

                    string addString;
                    addString = "," + userName + ",BT_GOOD_READS," + read.ToString() + ",<tag-place-holder>";

                    outputLine = outputLine.Replace(",", "");
                    outputLine += addString;
                    outputLinesList.Add(outputLine);
                }

                outputLines = outputLinesList.ToArray();
                File.WriteAllLines(JHOutputBookListGoodReadsFile, outputLines);
            }
        }
        public void ConvertWishList(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Logger.OutputError("Web Page File {0} does not exist", inputFile);
            }
            else
            {
                int i, j;
                bool delString;
                List<string> linesList;
                List<string> outputLinesList;
                string[] outputLines;
                List<int> startNumber = new List<int>();
                List<int> endNumber = new List<int>();
                bool read;

                var lines = File.ReadAllLines(inputFile);
                linesList = lines.ToList();

                // Delete all before "Filter & Sort"
                delString = false;
                for (i = linesList.Count - 1; i >= 0; --i)
                {
                    if (linesList[i] == "Filter & Sort")
                        delString = true;
                    if (delString)
                        linesList.RemoveAt(i);
                }

                // Delete all after "End of list"
                delString = false;
                for (i = 0; i < linesList.Count; ++i)
                {
                    if (linesList[i] == "End of list")
                        delString = true;
                    if (delString)
                    {
                        for (j = linesList.Count - 1; j >= i; --j)
                            linesList.RemoveAt(j);
                    }
                }

                linesList.RemoveAll(l => l.Equals(""));

                // Go through linesList and determine the start and ends of each output line
                for (i = 1; i < linesList.Count; ++i)
                {
                    // if  cosecutive lines the same ignoring white space
                    if (Regex.Replace(linesList[i], @"\s+", "") ==
                        Regex.Replace(linesList[i - 1], @"\s+", ""))
                    {
                        if (i - 2 >= 0)
                            endNumber.Add(i - 2);
                        startNumber.Add(i - 1);
                    }
                }
                endNumber.Add(i - 1);

                Boolean isAudioCD;

                outputLinesList = new List<string>();
                for (i = 0; i < startNumber.Count; ++i)
                {
                    string outputLine;

                    outputLine = linesList[startNumber[i]];
                    isAudioCD = outputLine.IndexOf("(Audio CD)") != -1;
                    outputLine = ConvertWishListTitle(outputLine);
                    outputLine += " " + ConvertWishListAuthor (linesList[startNumber[i] + 2]);

                    read = false; // default value

                    // get username
                    string userName;
                    User user = _context.Users.FirstOrDefault(u => u.UserId == Login.currentUserId);
                    if (user == null)
                        userName = "jerhogan";
                    else
                        userName = user.UserName;

                    string addString;
                    addString = "," + userName + ",BT_WISH_LIST," + read.ToString() + ",<tag-place-holder>";

                    outputLine = outputLine.Replace(",", ";");
                    outputLine += addString;
                    if ((outputLine.IndexOf ("[DVD]") == -1) && !isAudioCD) // If it's not a DVD or a CD then add
                        outputLinesList.Add(outputLine);
                }

                outputLines = outputLinesList.ToArray();
                File.WriteAllLines(JHOutputBookListWishListFile, outputLines);
            }
        }
        private string ConvertWishListTitle(string originalTitle)
        {
            string processedTitle;

            processedTitle = RemoveParenthesisBlock (originalTitle);
            processedTitle.Replace(",", ";");
            return (processedTitle.Trim());
        }
        private string RemoveParenthesisBlock(string originalString)
        {
            string processed;
            int lParPos, rParPos;

            processed = originalString;
            lParPos = processed.IndexOf("(");
            rParPos = processed.LastIndexOf(")");
            if ((lParPos != -1) && (rParPos != -1))
            {
                string newString = "";

                if (lParPos != 0)
                    newString = processed.Substring(0, lParPos);

                if (rParPos != processed.Length - 1)
                    newString += processed.Substring(rParPos + 1);

                processed = newString;
            }

            return (processed.Trim());
        }
        private string ConvertWishListAuthor(string originalAuthor)
        {
            string processed;

            processed = RemoveParenthesisBlock(originalAuthor);
            processed.Replace(",", ";");
            processed = RemoveByWord(processed);
            return (processed);
        }
        private string RemoveByWord(string originalString)
        {
            string processed;

            processed = originalString;

            if (processed.IndexOf("by ") == 0)
                processed = processed.Substring(3);

            return (processed);
        }
        public void Import(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Logger.OutputError("Import File {0} does not exist", inputFile);
            }
            else
            {
                startCmdNoBooks = _context.Books.Max(b => b.BookId);
                var lines = File.ReadAllLines(inputFile);
                inputFileLength = lines.Length;
                for (var i = 1; i < lines.Length; i += 1)
                {
                    BookListContextExtensions.AddBook(_context, lines[i]);
                }
                ReadScreenIds();
            }
        }
        void AddXMLParameter(XElement DATAElement, string XMLName, string XMLValue)
        {
            DATAElement.Add(new XElement(XMLName));
            XElement ParElement = DATAElement.Element(XMLName);
            ParElement.Value = XMLValue;
        }
        void AddXMLParameter(XElement DATAElement, string XMLName, int XMLValue)
        {
            AddXMLParameter(DATAElement, XMLName, XMLValue.ToString());
        }
        void AddXMLParameter(XElement DATAElement, string XMLName, bool XMLValue)
        {
            AddXMLParameter(DATAElement, XMLName, XMLValue.ToString());
        }
        public void XmlLoad(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Logger.OutputError("XML File {0} does not exist", inputFile);
                return;
            }
            if ((_context != null) &&
                ((_context.Books.Count() > 0) || (_context.Authors.Count() > 0) || (_context.Users.Count() > 0) ||
                 (_context.BookTypes.Count() > 0) || (_context.Tags.Count() > 0)))
            {
                Logger.OutputError("The Database already exists.");
                var query = Login.GetConsoleString("Overwrite ? (Y/N)");
                if (query.ToUpper()[0] != 'Y')
                    return;
            }

            InitialiseDatabase();

            XDocument xdoc = XDocument.Load(inputFile);

            var userEl = xdoc.Descendants("Users")
                             .FirstOrDefault();
            Console.Write("Add XML Users.");
            foreach (var dataEl in userEl.Elements("DATA"))
            {
                User user = new User();

                Int32.TryParse(dataEl.Element("UserId").Value, out int intPar);
                user.UserId = intPar;
                user.FirstName = dataEl.Element("FirstName").Value;
                user.MiddleName = dataEl.Element("MiddleName").Value;
                user.Surname = dataEl.Element("Surname").Value;
                user.Password = dataEl.Element("Password").Value;
                user.UserName = dataEl.Element("UserName").Value;
                user.Email = dataEl.Element("Email").Value;

                Console.Write(".");
                _context.Users.Add(user);
                _context.SaveChanges();
            }

            var bookTypeEl = xdoc.Descendants("BookTypes")
                                 .FirstOrDefault();
            Console.Write("Add XML Book Types.");
            foreach (var dataEl in bookTypeEl.Elements("DATA"))
            {
                BookType bookType = new BookType();

                Int32.TryParse(dataEl.Element("BookTypeId").Value, out int intPar);
                bookType.BookTypeId = intPar;
                bookType.Description = dataEl.Element("Description").Value;
                bookType.BK_TYPE = dataEl.Element("BKTYPE").Value;
                Int32.TryParse(dataEl.Element("ShoppingListNo").Value, out intPar);
                bookType.ShoppingListNo = intPar;

                Console.Write(".");
                _context.BookTypes.Add(bookType);
                _context.SaveChanges();
            }


            var bookEl = xdoc.Descendants("Books")
                            .FirstOrDefault();
            Console.Write("Add XML Books.");
            foreach (var dataEl in bookEl.Elements("DATA"))
            {
                Book book = new Book();

                Int32.TryParse(dataEl.Element("BookId").Value, out int intPar);
                book.BookId = intPar;
                book.GoogleId = dataEl.Element("GoogleId").Value;
                book.Title = dataEl.Element("Title").Value;
                book.SubTitle = dataEl.Element("SubTitle").Value;
                book.Description = dataEl.Element("Description").Value;
                Int32.TryParse(dataEl.Element("PageCount").Value, out intPar);
                book.PageCount = intPar;
                book.PrintType = dataEl.Element("PrintType").Value;
                book.PublishedDate = dataEl.Element("PublishedDate").Value;
                book.Publisher = dataEl.Element("Publisher").Value;
                book.SmallThumbNail = dataEl.Element("SmallThumbNail").Value;
                book.ThumbNail = dataEl.Element("ThumbNail").Value;
                Int32.TryParse(dataEl.Element("UserId").Value, out intPar);
                book.UserId = intPar;
                Int32.TryParse(dataEl.Element("BookTypeId").Value, out intPar);
                book.BookTypeId = intPar;
                Boolean.TryParse(dataEl.Element("Read").Value, out bool boolPar);
                book.Read = boolPar;

                Console.Write(".");
                _context.Books.Add(book);
                _context.SaveChanges();
                RefreshBookIdsCreate(book.BookId);
                ReadScreenIds();
            }

            var authorEl = xdoc.Descendants("Authors")
                               .FirstOrDefault();
            Console.Write("Add XML Authors.");
            foreach (var dataEl in authorEl.Elements("DATA"))
            {
                Author author = new Author();

                Int32.TryParse(dataEl.Element("AuthorId").Value, out int intPar);
                author.AuthorId = intPar;
                Int32.TryParse(dataEl.Element("BookId").Value, out intPar);
                author.BookId = intPar;
                author.Name = dataEl.Element("Name").Value;

                Console.Write(".");
                _context.Authors.Add(author);
                _context.SaveChanges();
            }

            var tagEl = xdoc.Descendants("Tags")
                            .FirstOrDefault();
            Console.Write("Add XML Tags.");
            foreach (var dataEl in tagEl.Elements("DATA"))
            {
                Tag tag = new Tag();

                Int32.TryParse(dataEl.Element("TagId").Value, out int intPar);
                tag.TagId = intPar;
                Int32.TryParse(dataEl.Element("BookId").Value, out intPar);
                tag.BookId = intPar;
                tag.Value = dataEl.Element("Value").Value;

                Console.Write(".");
                _context.Tags.Add(tag);
                _context.SaveChanges();
            }
            ReadScreenIds();
        }

        private void InitialiseDatabase()
        {
            if (_context == null)
            {
                Logger.OutputInformation("Initialise Database called but _context is null");
                return;
            }
            List<Book> bookList;
            bookList = _context.Books.ToList();
            bookList.Reverse();
            Console.Write("Remove Books.");
            foreach (Book book in bookList)
            {
                Console.Write(".");
                _context.Books.Remove(book);
                _context.SaveChanges();
                // RefreshBookIdsDelete(book.BookId);
                RefreshScreenIds();
            }

            List<Author> authorList;
            authorList = _context.Authors.ToList();
            authorList.Reverse();
            Console.Write("Remove Authors.");
            foreach (Author author in authorList)
            {
                Console.Write(".");
                _context.Authors.Remove(author);
                _context.SaveChanges();
            }

            List<User> userList;
            userList = _context.Users.ToList();
            userList.Reverse();
            Console.Write("Remove Users.");
            foreach (User user in userList)
            {
                Console.Write(".");
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            List<BookType> bookTypeList;
            bookTypeList = _context.BookTypes.ToList();
            bookTypeList.Reverse();
            Console.Write("Remove Book Types.");
            foreach (BookType bookType in bookTypeList)
            {
                Console.Write(".");
                _context.BookTypes.Remove(bookType);
                _context.SaveChanges();
            }

            List<Tag> tagList;
            tagList = _context.Tags.ToList();
            tagList.Reverse();
            Console.Write("Remove Tags.");
            foreach (Tag tag in tagList)
            {
                Console.Write(".");
                _context.Tags.Remove(tag);
                _context.SaveChanges();
            }
        }

        public void XmlSave(string outputFile)
        {
            if (File.Exists(outputFile))
            {
                Logger.OutputError("XML File {0} already exists", outputFile);
                string overWrite = Login.GetConsoleString("Overwrite (Yes/No)");
                if (overWrite.ToUpper()[0] != 'Y')
                    return;

            }

            XDocument xDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("JH BooksList XML Save Database"),
                new XElement("JHDBFile", new XElement("Database", new XElement("JH"))));

            List<XElement> xElements = new List<XElement>();
            XElement el = new XElement("Books");
            xElements.Add(el);
            el = new XElement("Authors");
            xElements.Add(el);
            el = new XElement("Users");
            xElements.Add(el);
            el = new XElement("BookTypes");
            xElements.Add(el);
            el = new XElement("Tags");
            xElements.Add(el);

            XElement JHElement = xDocument.Descendants("JH").FirstOrDefault();
            JHElement.Add(xElements);

            XElement BooksElement = xDocument.Descendants("Books").FirstOrDefault();
            foreach (Book book in _context.Books)
            {
                BooksElement.Add(new XElement("DATA"));
                XElement DATAElement = null;
                foreach (XElement dataEl in BooksElement.Elements("DATA"))
                    DATAElement = dataEl;
                AddXMLParameter(DATAElement, "BookId", book.BookId);
                AddXMLParameter(DATAElement, "GoogleId", book.GoogleId);
                AddXMLParameter(DATAElement, "Title", book.Title);
                AddXMLParameter(DATAElement, "SubTitle", book.SubTitle);
                AddXMLParameter(DATAElement, "Description", book.Description);
                AddXMLParameter(DATAElement, "PageCount", book.PageCount);
                AddXMLParameter(DATAElement, "PrintType", book.PrintType);
                AddXMLParameter(DATAElement, "PublishedDate", book.PublishedDate);
                AddXMLParameter(DATAElement, "Publisher", book.Publisher);
                AddXMLParameter(DATAElement, "SmallThumbNail", book.SmallThumbNail);
                AddXMLParameter(DATAElement, "ThumbNail", book.ThumbNail);
                AddXMLParameter(DATAElement, "UserId", book.UserId);
                AddXMLParameter(DATAElement, "BookTypeId", book.BookTypeId);
                AddXMLParameter(DATAElement, "Read", book.Read);
            }

            XElement AuthorsElement = xDocument.Descendants("Authors").FirstOrDefault();
            foreach (Author author in _context.Authors)
            {
                AuthorsElement.Add(new XElement("DATA"));
                XElement DATAElement = null;
                foreach (XElement dataEl in AuthorsElement.Elements("DATA"))
                    DATAElement = dataEl;
                AddXMLParameter(DATAElement, "AuthorId", author.AuthorId);
                AddXMLParameter(DATAElement, "BookId", author.BookId);
                AddXMLParameter(DATAElement, "Name", author.Name);
            }

            XElement UsersElement = xDocument.Descendants("Users").FirstOrDefault();
            foreach (User user in _context.Users)
            {
                UsersElement.Add(new XElement("DATA"));
                XElement DATAElement = null;
                foreach (XElement dataEl in UsersElement.Elements("DATA"))
                    DATAElement = dataEl;
                AddXMLParameter(DATAElement, "UserId", user.UserId);
                AddXMLParameter(DATAElement, "FirstName", user.FirstName);
                AddXMLParameter(DATAElement, "MiddleName", user.MiddleName);
                AddXMLParameter(DATAElement, "Surname", user.Surname);
                AddXMLParameter(DATAElement, "Password", user.Password);
                AddXMLParameter(DATAElement, "UserName", user.UserName);
                AddXMLParameter(DATAElement, "Email", user.Email);
            }

            XElement BookTypesElement = xDocument.Descendants("BookTypes").FirstOrDefault();
            foreach (BookType bookType in _context.BookTypes)
            {
                BookTypesElement.Add(new XElement("DATA"));
                XElement DATAElement = null;
                foreach (XElement dataEl in BookTypesElement.Elements("DATA"))
                    DATAElement = dataEl;
                AddXMLParameter(DATAElement, "BookTypeId", bookType.BookTypeId);
                AddXMLParameter(DATAElement, "Description", bookType.Description);
                AddXMLParameter(DATAElement, "BKTYPE", bookType.BK_TYPE);
                AddXMLParameter(DATAElement, "ShoppingListNo", bookType.ShoppingListNo);
            }

            XElement TagsElement = xDocument.Descendants("Tags").FirstOrDefault();
            foreach (Tag tag in _context.Tags)
            {
                TagsElement.Add(new XElement("DATA"));
                XElement DATAElement = null;
                foreach (XElement dataEl in TagsElement.Elements("DATA"))
                    DATAElement = dataEl;
                AddXMLParameter(DATAElement, "TagId", tag.TagId);
                AddXMLParameter(DATAElement, "BookId", tag.BookId);
                AddXMLParameter(DATAElement, "Value", tag.Value);
            }

            try
            {
                xDocument.Save(outputFile);
            }
            catch (Exception ex)
            {
                Logger.OutputError("Error saving XML file \"{0}\".", outputFile);
                Logger.OutputError(ex.Message);
            }
        }
        public void Delete(FieldType fieldType, int bookRowNo, string delValue)
        {
            Logger.OutputInformation("Inital value is:");
            DisplayBook(bookRowNo);

            switch (fieldType)
            {
                case FieldType.fAll:
                    DeleteBook(bookRowNo);
                    break;
                case FieldType.fAuthor:
                    DeleteBookAuthor(bookRowNo, delValue);
                    break;
                case FieldType.fTag:
                    DeleteBookTag(bookRowNo, delValue);
                    break;
            }
            RefreshScreenIds();

            Logger.OutputInformation("New value is:");
            DisplayBook(bookRowNo);
        }
        public void DeleteBook(int delIndex)
        {
            Book book;

            book = _context.Books.FirstOrDefault(b => b.BookId == delIndex);
            if (book == null)
            {
                Logger.OutputError("Given index does not exist for any book.");
                return;
            }
            var authorList = _context.Authors.ToList();
            authorList.Reverse();
            authorList = authorList.Where(a => a.BookId == delIndex).ToList();
            foreach (Author author in authorList)
            {
                _context.Authors.Remove(author);
                _context.SaveChanges();
            }
            var tagList = _context.Tags.ToList();
            tagList.Reverse();
            tagList = tagList.Where(t => t.BookId == delIndex).ToList();
            foreach (Tag tag in tagList)
            {
                _context.Tags.Remove(tag);
                _context.SaveChanges();
            }
            _context.Books.Remove(book);
            _context.SaveChanges();
            RefreshBookIdsDelete(book.BookId);
            RefreshScreenIds();


            books = _context.Books.Where(b => b.UserId == Login.currentUserId).ToList();
            ReadScreenIds();
        }
        public void DeleteBookAuthor(int updIndex, string authorName)
        {
            Book book;

            book = _context.Books.FirstOrDefault(b => b.BookId == updIndex);
            if (book == null)
            {
                Logger.OutputError("Given index does not exist for any book.");
                return;
            }

            Author author;

            author = _context.Authors.FirstOrDefault(a => a.BookId == updIndex && a.Name == authorName);
            if (author == null)
            {
                Logger.OutputError("This author does not exist.");
                return;
            }

            _context.Authors.Remove(author);
            _context.SaveChanges();
        }
        public void DeleteBookTag(int updIndex, string tagValues)
        {
            Book book;
            string[] tagValue;

            book = _context.Books.FirstOrDefault(b => b.BookId == updIndex);
            if (book == null)
            {
                Logger.OutputError("Given index does not exist for any book.");
                return;
            }

            tagValue = tagValues.Split(';');
            foreach (string tv in tagValue)
            {
                Tag tag;

                tag = _context.Tags.FirstOrDefault(t => t.BookId == updIndex && t.Value == tv);
                if (tag == null)
                {
                    Logger.OutputError("This tag {0} does not exist.", tv);
                    continue;
                }

                _context.Tags.Remove(tag);
                _context.SaveChanges();

            }
        }
        public void Update(FieldType field, int updIndex, string newValue)
        {
            Logger.OutputInformation("Inital value is:");
            DisplayBook(updIndex);

            switch (field)
            {
                case FieldType.fTitle:
                    UpdateBookTitle(updIndex, newValue);
                    break;

                case FieldType.fAuthor:
                    UpdateBookAuthor(updIndex, newValue);
                    break;

                case FieldType.fTag:
                    UpdateBookTag(updIndex, newValue);
                    break;

                default:
                    // ignore
                    break;
            }
            Logger.OutputInformation("New value is:");
            DisplayBook(updIndex);
        }
        public void UpdateBookTitle(int updIndex, string title)
        {
            Book book;

            book = _context.Books.FirstOrDefault(b => b.BookId == updIndex);
            if (book == null)
            {
                Logger.OutputError("Given index does not exist for any book.");
                return;
            }

            book.Title = title;
            _context.SaveChanges();
        }
        public void UpdateBookAuthor(int updIndex, string authorName)
        {
            Book book;

            book = _context.Books.FirstOrDefault(b => b.BookId == updIndex);
            if (book == null)
            {
                Logger.OutputError("Given index does not exist for any book.");
                return;
            }

            Author author;

            author = _context.Authors.FirstOrDefault(a => a.BookId == updIndex && a.Name == authorName);
            if (author != null)
            {
                Logger.OutputError("This author already exists.");
                return;
            }

            int authorId;
            authorId = (_context.Authors == null || _context.Authors.Count() == 0) ? 1 : _context.Authors.Max(a => a.AuthorId) + 1;

            author = new Author
            {
                AuthorId = authorId,
                BookId = updIndex,
                Name = authorName
            };
            _context.Authors.Add(author);
            _context.SaveChanges();
        }
        public void UpdateBookTag(int updIndex, string tagValues)
        {
            Book book;
            string[] tagValue;

            book = _context.Books.FirstOrDefault(b => b.BookId == updIndex);
            if (book == null)
            {
                Logger.OutputError("Given index does not exist for any book.");
                return;
            }

            tagValue = tagValues.Split(';');

            foreach (string tv in tagValue)
            {
                Tag tag;

                tag = _context.Tags.FirstOrDefault(t => t.BookId == updIndex && t.Value == tv);
                if (tag != null)
                {
                    Logger.OutputError("This tag {0} already exists.", tv);
                    continue;
                }

                int tagId;
                tagId = (_context.Tags == null || _context.Tags.Count() == 0) ? 1 : _context.Tags.Max(t => t.TagId) + 1;

                tag = new Tag
                {
                    TagId = tagId,
                    BookId = updIndex,
                    Value = tv
                };
                _context.Tags.Add(tag);
                _context.SaveChanges();
            }
        }
        public int CreateBook(string query, string user, int bookTypeRowNo, bool read, string tagValues)
        {
            int bookRowNo;

            bookRowNo = DBCreateBook(query, user, bookTypeRowNo, read, tagValues);

            Logger.OutputInformation("New value is:");
            DisplayBook(bookRowNo);

            return (bookRowNo);
        }
        public int DBCreateBook(string query, string user, int bookTypeRowNo, bool read, string tagValues)
        {
            string[] allParArray = new string[5];
            string allPars;
            int newId;
            BookType bookType;

            bookType = _context.BookTypes.FirstOrDefault(bt => bt.BookTypeId == bookTypeRowNo);
            if (bookType == null)
            {
                Logger.OutputError("There is no entry for Book Type {0}.",
                                   bookTypeRowNo.ToString());
                return (-1);
            }
            allParArray[0] = query;
            allParArray[1] = user;
            allParArray[2] = bookType.BK_TYPE;
            allParArray[3] = read.ToString();
            allParArray[4] = tagValues;
            allPars = string.Join(",", allParArray);
            newId = BookListContextExtensions.AddBook(_context, allPars);
            RefreshBookIdsCreate(newId);
            RefreshScreenIds();

            return (newId);
        }
        public void DisplayBooksScreen()
        {
            int i;

            DisplayBookHeader();
            if ((screenIds.Count > 0) && (topScreenId != -1) &&
                (bottomScreenId != -1))
            {
                for (i = topScreenId; i <= bottomScreenId; ++i)
                {
                    DisplayBook(screenIds[i]);
                }
            }
        }

        private const string bookPadding = " | ";
        private void DisplayBookHeader()
        {
            Logger.OutputInformation(FixedLength("Book#", 5) + bookPadding +
                              FixedLength("Title", 20) + bookPadding +
                              FixedLength("Authors", 30) + bookPadding +
                              FixedLength("Tags", 30) + bookPadding);

        }

        public void DisplayBook(int bookId)
        {
            string authorsString;
            List<string> authorsStringList;
            string tagsString;
            List<string> tagsStringList;

            Book book = _context.Books.Where(b => b.UserId == Login.currentUserId).FirstOrDefault(b => b.BookId == bookId);
            if (book != null)
            {
                List<Author> authors = _context.Authors.Where(a => a.BookId == bookId).ToList();
                authorsStringList = new List<string>();
                foreach (Author a in authors)
                    authorsStringList.Add(a.Name);
                authorsString = String.Join(";", authorsStringList.ToArray());

                List<Tag> tags = _context.Tags.Where(t => t.BookId == bookId).ToList();
                tagsStringList = new List<string>();
                foreach (Tag t in tags)
                    tagsStringList.Add(t.Value);
                tagsString = String.Join(";", tagsStringList.ToArray());

                Logger.OutputInformation(FixedLength(bookId.ToString(), 5) + bookPadding +
                                  FixedLength(book.Title, 20) + bookPadding +
                                  FixedLength(authorsString, 30) + bookPadding +
                                  FixedLength(tagsString, 30) + bookPadding);
            }
            else
            {
                if ((bookId >= _context.Books.Min(b => b.BookId)) && (bookId <= _context.Books.Max(b => b.BookId)))
                {
                    Logger.OutputError("Book " + bookId + " has been deleted.");
                }
                else
                {
                    Logger.OutputError("Book " + bookId + " does not exist - range is " +
                         _context.Books.Min(b => b.BookId) + " to " + _context.Books.Max(b => b.BookId));
                }
            }
        }
        public void RegisterNewUser(string FirstName,
           string MiddleName, string Surname, string Encrypted,
           string UserName, string Email)
        {
            User user = _context.Users.FirstOrDefault(u => u.UserName == UserName);
            if (user != null)
            {
                Logger.OutputError("User name  " + UserName + " already exists.");
            }
            else
            {
                BookListContextExtensions.AddUser(_context, FirstName, MiddleName, Surname, Encrypted, UserName, Email);
            }
        }
        public bool LoginUser(string UserName, string Encrypted)
        {
            User user;
            bool loggedInSuccessfully;

            user = _context.Users.FirstOrDefault(u => u.UserName == UserName && u.Password == Encrypted);
            loggedInSuccessfully = user != null;
            if (loggedInSuccessfully)
            {
                Login.currentUserId = user.UserId;
                string welcome;

                welcome = "Welcome " + user.FirstName + " !";
                Logger.OutputInformation(welcome);

                Initialise(_context);

                ReadScreenIds();
            }
            return (loggedInSuccessfully);
        }
        public static string GetHtml(User user, string newPassword)
        {
            string messageBody;

            try
            {
                messageBody = "<h1>Changing password for user - " +
                              user.UserName + "</h1><h2> Hi " + user.FirstName +
                              "</h2><h2>Your new password is " + newPassword + "</h2>";
            }
            catch (Exception ex)
            {
                Logger.OutputError(ex.Message);
                return null;
            }

            return (messageBody);
        }
        public static void Email(string htmlString, string toMailAddress)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress("jerhogan@outlook.ie");
                message.To.Add(new MailAddress(toMailAddress));
                message.Subject = "JHConsoleBookList Password has been changed Successfully";
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = htmlString;
                smtp.Port = 587;
                smtp.Host = "smtp.live.com"; //for hotmail host  
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("jerhogan@outlook.ie",
                                                         Encode_Decode.Decrypt("8a7gafvWeNEfrSzjSKThH/ZRaRzGyD26zYNxcgXmEMU="));
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Logger.OutputError("Email error " + ex.Message);
            }
        }
        public static string Get8CharacterRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path.Substring(0, 8);  // Return 8 character string
        }
        public bool EmailUserPassword(string UserName)
        {
            string newPassword;
            string newEncrypted;
            string toMailAddress;

            newPassword = Get8CharacterRandomString();
            newEncrypted = Encode_Decode.Encrypt(newPassword);

            User user = _context.Users.FirstOrDefault(u => u.UserName == UserName);
            if (user == null)
            {
                Logger.OutputError("User name " + UserName + " does not exist.");
            }
            else
            {
                user.Password = newEncrypted;
                _context.SaveChanges();
                toMailAddress = user.Email;

                string htmlString = GetHtml(user, newPassword);
                Email(htmlString, toMailAddress);
            }
            return (true);
        }
        public bool ChangeUserPassword(string userName, string encryptedPassword,
                    string newEncryptedPassword, string repeatEncryptedPassword)
        {
            User user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                Logger.OutputError("Must be an active user.");
                return (false);
            }
            if (encryptedPassword != user.Password)
            {
                Logger.OutputError("Old password entered incorrectly.");
                return (false);
            }
            if (newEncryptedPassword != repeatEncryptedPassword)
            {
                Logger.OutputError("New password does not match repeat password.");
                return (false);
            }
            // set new password
            user.Password = newEncryptedPassword;
            _context.SaveChanges();
            return (true);
        }
        public void FillInEmptyTags()
        {
            string tagValue;
            string[] tagValues;
            List<int> emptyBookList = new List<int>();
            Book book;

            foreach (Book emptyBook in _context.Books)
            {
                if (EmptyTag(emptyBook.BookId))
                    emptyBookList.Add(emptyBook.BookId);
            }

            foreach (int bookId in emptyBookList)
            {
                Logger.OutputInformation("Old value is:");
                DisplayBook(bookId);

                book = _context.Books.FirstOrDefault(b => b.BookId == bookId);
                if (book == null)
                {
                    Logger.OutputError("Given index {0} does not exist for any book.",
                                       bookId.ToString());
                    continue;
                }

                tagValue = GetTagFromUser(book);
                tagValues = tagValue.Split(';');
                foreach (string tv in tagValues)
                {
                    Tag tag;

                    tag = _context.Tags.FirstOrDefault(t => t.BookId == book.BookId && t.Value == tv);
                    if (tag != null)
                    {
                        Logger.OutputError("This tag {0} already exists.", tv);
                        continue;
                    }

                    int tagId;
                    tagId = (_context.Tags == null || _context.Tags.Count() == 0) ? 1 : _context.Tags.Max(t => t.TagId) + 1;

                    tag = new Tag
                    {
                        TagId = tagId,
                        BookId = book.BookId,
                        Value = tv
                    };
                    _context.Tags.Add(tag);
                    _context.SaveChanges();

                    Logger.OutputInformation("New value is:");
                    DisplayBook(bookId);
                }
            }
        }
        public Boolean EmptyTag(int bookId)
        {
            return _context.Tags.Where(t => t.BookId == bookId).Count() == 0;
        }
        public string GetTagFromUser(Book book)
        {
            string tagValues;

            tagValues = Login.GetConsoleString("new tag");

            return (tagValues);
        }
        public void DisplayCounts(FieldType fieldType1, FieldType fieldType2)
        {
            if (fieldType2 == FieldType.fNone)
            {
                switch (fieldType1)
                {
                    case FieldType.fAuthor:
                        DisplayCountsAuthors();
                        break;
                    case FieldType.fTag:
                        DisplayCountsTags();
                        break;
                    case FieldType.fBookType:
                        DisplayCountsBookTypes();
                        break;
                    case FieldType.fRead:
                        DisplayCountsReads();
                        break;
                    case FieldType.fUser:
                        DisplayCountsUsers();
                        break;
                    case FieldType.fAll:
                        DisplayCountsAuthors();
                        DisplayCountsTags();
                        DisplayCountsBookTypes();
                        DisplayCountsReads();
                        DisplayCountsUsers();
                        break;
                }
            }
            else if (fieldType2 == FieldType.fBookType)
            {
                switch (fieldType1)
                {
                    case FieldType.fAuthor:
                        DisplayCountsAuthorsBookTypes();
                        break;
                    case FieldType.fTag:
                        DisplayCountsTagsBookTypes();
                        break;
                    case FieldType.fRead:
                        DisplayCountsReadsBookTypes();
                        break;
                    case FieldType.fUser:
                        DisplayCountsUsersBookTypes();
                        break;
                    case FieldType.fAll:
                        DisplayCountsAuthorsBookTypes();
                        DisplayCountsTagsBookTypes();
                        DisplayCountsReadsBookTypes();
                        DisplayCountsUsersBookTypes();
                        break;
                }
            }
            else if (fieldType2 == FieldType.fRead)
            {
                switch (fieldType1)
                {
                    case FieldType.fAuthor:
                        DisplayCountsAuthorsReads();
                        break;
                    case FieldType.fTag:
                        DisplayCountsTagsReads();
                        break;
                    case FieldType.fBookType:
                        DisplayCountsBookTypesReads();
                        break;
                    case FieldType.fUser:
                        DisplayCountsUsersReads();
                        break;
                    case FieldType.fAll:
                        DisplayCountsAuthorsReads();
                        DisplayCountsTagsReads();
                        DisplayCountsBookTypesReads();
                        DisplayCountsUsersReads();
                        break;
                }
            }

            RefreshScreenIds();
        }
        void DisplayCountsAuthors()
        {
            List<Author> authorList = new List<Author>();

            foreach (Author author in _context.Authors)
            {
                Book book = _context.Books.FirstOrDefault(b => b.BookId ==
                    author.BookId);
                if (book.UserId == Login.currentUserId)
                    authorList.Add(author);
            }
            var groups = authorList.GroupBy(a => a.Name, (k, c) => new Result()
            {
                AuthorName = k,
                Count = c.Count()
            });
            var orderedGroups = groups.OrderByDescending(g => g.Count);
            foreach (var authGroup in orderedGroups)
            {
                if (authGroup.Count > 1)
                {
                    Logger.OutputInformation("Author " + authGroup.AuthorName + "(" +
                        authGroup.Count + ")");
                }
            }
        }
        public struct AuthorBookType
        {
            public string authorName;
            public string bookTypeName;
        }
        void DisplayCountsAuthorsBookTypes()
        {
            List<AuthorBookType> authorBookTypeList = new List<AuthorBookType>();

            foreach (Author author in _context.Authors)
            {
                Book book = _context.Books.FirstOrDefault(b => b.BookId ==
                    author.BookId);
                if (book.UserId == Login.currentUserId)
                {
                    AuthorBookType abt = new AuthorBookType();

                    abt.authorName = author.Name;
                    BookType bookType = _context.BookTypes.FirstOrDefault(b => b.BookTypeId ==
                    book.BookTypeId);
                    abt.bookTypeName = bookType.BK_TYPE;
                    authorBookTypeList.Add(abt);
                }
            }
            var groups = authorBookTypeList
                .GroupBy(ab => new { ab.authorName, ab.bookTypeName })
                .Select(g => new
                {
                    AuthorName = g.Key.authorName,
                    BookTypeName = g.Key.bookTypeName,
                    Count = g.Count()
                });
            var orderedGroups = groups.OrderByDescending(ab => ab.Count);
            foreach (var authGroup in orderedGroups)
            {
                Logger.OutputInformation("Book type " + authGroup.BookTypeName + " Author " + authGroup.AuthorName + "(" +
                    authGroup.Count + ")");
            }
        }
        public struct AuthorRead
        {
            public string authorName;
            public Boolean read;
        }
        void DisplayCountsAuthorsReads()
        {
            List<AuthorRead> authorReadList = new List<AuthorRead>();

            foreach (Author author in _context.Authors)
            {
                Book book = _context.Books.FirstOrDefault(b => b.BookId ==
                    author.BookId);
                if (book.UserId == Login.currentUserId)
                {
                    AuthorRead ar = new AuthorRead();

                    ar.authorName = author.Name;
                    ar.read = book.Read;
                    authorReadList.Add(ar);
                }
            }
            var groups = authorReadList
                .GroupBy(ar => new { ar.authorName, ar.read })
                .Select(g => new
                {
                    AuthorName = g.Key.authorName,
                    Read = g.Key.read,
                    Count = g.Count()
                });
            var orderedGroups = groups.OrderByDescending(ar => ar.Count);
            foreach (var authGroup in orderedGroups)
            {
                Logger.OutputInformation("Read " + authGroup.Read.ToString() + " Author " + authGroup.AuthorName + "(" +
                    authGroup.Count + ")");
            }
        }
        void DisplayCountsTags()
        {
            List<Tag> tagList = new List<Tag>();

            foreach (Tag tag in _context.Tags)
            {
                Book book = _context.Books.FirstOrDefault(b => b.BookId ==
                    tag.BookId);
                if (book.UserId == Login.currentUserId)
                    tagList.Add(tag);
            }
            var groups = tagList.GroupBy(t => t.Value, (k, c) => new TagResult()
            {
                TagName = k,
                Count = c.Count()
            });
            var orderedGroups = groups.OrderByDescending(g => g.Count);
            foreach (var tagGroup in orderedGroups)
            {
                Logger.OutputInformation("Tag " + tagGroup.TagName + "(" +
                    tagGroup.Count + ")");
            }
        }
        public struct TagBookType
        {
            public string tagValue;
            public string bookTypeName;
        }
        void DisplayCountsTagsBookTypes()
        {
            List<TagBookType> tagBookTypeList = new List<TagBookType>();

            foreach (Tag tag in _context.Tags)
            {
                Book book = _context.Books.FirstOrDefault(b => b.BookId ==
                    tag.BookId);
                if (book.UserId == Login.currentUserId)
                {
                    TagBookType tbt = new TagBookType();

                    tbt.tagValue = tag.Value;
                    BookType bookType = _context.BookTypes.FirstOrDefault(b => b.BookTypeId ==
                    book.BookTypeId);
                    tbt.bookTypeName = bookType.BK_TYPE;
                    tagBookTypeList.Add(tbt);
                }
            }
            var groups = tagBookTypeList
                .GroupBy(tb => new { tb.tagValue, tb.bookTypeName })
                .Select(g => new
                {
                    TagValue = g.Key.tagValue,
                    BookTypeName = g.Key.bookTypeName,
                    Count = g.Count()
                });
            var orderedGroups = groups.OrderByDescending(ab => ab.Count);
            foreach (var authGroup in orderedGroups)
            {
                Logger.OutputInformation("Book type " + authGroup.BookTypeName + " Tag " + authGroup.TagValue + "(" +
                    authGroup.Count + ")");
            }
        }
        public struct TagRead
        {
            public string tagValue;
            public bool read;
        }
        void DisplayCountsTagsReads()
        {
            List<TagRead> tagReadList = new List<TagRead>();

            foreach (Tag tag in _context.Tags)
            {
                Book book = _context.Books.FirstOrDefault(b => b.BookId ==
                    tag.BookId);
                if (book.UserId == Login.currentUserId)
                {
                    TagRead tr = new TagRead();

                    tr.tagValue = tag.Value;
                    tr.read = book.Read;
                    tagReadList.Add(tr);
                }
            }
            var groups = tagReadList
                .GroupBy(tr => new { tr.tagValue, tr.read })
                .Select(g => new
                {
                    TagValue = g.Key.tagValue,
                    Read = g.Key.read,
                    Count = g.Count()
                });
            var orderedGroups = groups.OrderByDescending(tr => tr.Count);
            foreach (var tagGroup in orderedGroups)
            {
                Logger.OutputInformation("Book type " + tagGroup.Read.ToString () + " Tag " + tagGroup.TagValue + "(" +
                    tagGroup.Count + ")");
            }
        }
        void DisplayCountsBookTypes()
        {
            List<Book> bookList = new List<Book>();

            foreach (Book book in _context.Books)
            {
                if (book.UserId == Login.currentUserId)
                    bookList.Add(book);
            }
            var groups = bookList
                  .Join(_context.BookTypes,
                        bk => bk.BookTypeId,
                        bt => bt.BookTypeId,
                        (bl, bt) => new { Book = bl, Type = bt });

            var countGroups = groups.GroupBy(b => b.Type.BK_TYPE, (k, c) => new BookTypeResult()
            {
                BookTypeName = k,
                Count = c.Count()
            });
            var orderedGroups = countGroups.OrderByDescending(g => g.Count);
            foreach (var btGroup in orderedGroups)
            {
                Logger.OutputInformation("Book Type " + btGroup.BookTypeName + "(" +
                    btGroup.Count + ")");
            }
        }
        void DisplayCountsReads()
        {
            List<Book> bookList = new List<Book>();

            foreach (Book book in _context.Books)
            {
                if (book.UserId == Login.currentUserId)
                    bookList.Add(book);
            }
            var countGroups = bookList.GroupBy(b => b.Read, (k, c) => new ReadResult()
            {
                ReadValue = k.ToString(),
                Count = c.Count()
            });
            var orderedGroups = countGroups.OrderByDescending(g => g.Count);
            foreach (var btGroup in orderedGroups)
            {
                Logger.OutputInformation("Book Read ? " + btGroup.ReadValue + "(" +
                    btGroup.Count + ")");
            }
        }
        public struct ReadBookType
        {
            public Boolean read;
            public string bookTypeName;
        }
        void DisplayCountsReadsBookTypes()
        {
            List<ReadBookType> readBookTypeList = new List<ReadBookType>();

            foreach (Book book in _context.Books)
            {
                if (book.UserId == Login.currentUserId)
                {
                    ReadBookType rbt = new ReadBookType();

                    rbt.read = book.Read;
                    BookType bookType = _context.BookTypes.FirstOrDefault(b => b.BookTypeId ==
                    book.BookTypeId);
                    rbt.bookTypeName = bookType.BK_TYPE;
                    readBookTypeList.Add(rbt);
                }
            }
            var groups = readBookTypeList
                .GroupBy(rb => new { rb.read, rb.bookTypeName })
                .Select(g => new
                {
                    Read = g.Key.read,
                    BookTypeName = g.Key.bookTypeName,
                    Count = g.Count()
                });
            var orderedGroups = groups.OrderByDescending(rb => rb.Count);
            foreach (var readGroup in orderedGroups)
            {
                Logger.OutputInformation("Book type " + readGroup.BookTypeName + " Read " + 
                    readGroup.Read.ToString () + "(" + readGroup.Count + ")");
            }
        }
        void DisplayCountsUsers()
        {
            var groups = _context.Books
                  .Join(_context.Users,
                        b => b.UserId,
                        u => u.UserId,
                        (b, u) => new { Book = b, User = u });

            var countGroups = groups.GroupBy(b => b.User.UserName, (k, c) => new UserResult()
            {
                UserName = k,
                Count = c.Count()
            });
            var orderedGroups = countGroups.OrderByDescending(g => g.Count);
            foreach (var uGroup in orderedGroups)
            {
                Logger.OutputInformation("User " + uGroup.UserName + "(" +
                    uGroup.Count + ")");
            }
        }
        public struct UserBookType
        {
            public string userName;
            public string bookTypeName;
        }
        void DisplayCountsUsersBookTypes()
        {
            List<UserBookType> userBookTypeList = new List<UserBookType>();

            foreach (Book book in _context.Books)
            {
                UserBookType ubt = new UserBookType();

                User user = _context.Users.FirstOrDefault(b => b.UserId ==
                book.UserId);
                ubt.userName = user.UserName;
                BookType bookType = _context.BookTypes.FirstOrDefault(b => b.BookTypeId ==
                book.BookTypeId);
                ubt.bookTypeName = bookType.BK_TYPE;
                userBookTypeList.Add(ubt);
            }
            var groups = userBookTypeList
                .GroupBy(ub => new { ub.userName, ub.bookTypeName })
                .Select(g => new
                {
                    UserName = g.Key.userName,
                    BookTypeName = g.Key.bookTypeName,
                    Count = g.Count()
                });
            var orderedGroups = groups.OrderByDescending(ub => ub.Count);
            foreach (var userGroup in orderedGroups)
            {
                Logger.OutputInformation("Book type " + userGroup.BookTypeName + " User " +
                    userGroup.UserName + "(" + userGroup.Count + ")");
            }
        }
        public struct UserRead
        {
            public string userName;
            public bool read;
        }
        void DisplayCountsUsersReads()
        {
            List<UserRead> userReadList = new List<UserRead>();

            foreach (Book book in _context.Books)
            {
                UserRead ur = new UserRead();

                User user = _context.Users.FirstOrDefault(b => b.UserId ==
                book.UserId);
                ur.userName = user.UserName;
                ur.read = book.Read;
                userReadList.Add(ur);
            }
            var groups = userReadList
                .GroupBy(ur => new { ur.userName, ur.read })
                .Select(g => new
                {
                    UserName = g.Key.userName,
                    Read = g.Key.read,
                    Count = g.Count()
                });
            var orderedGroups = groups.OrderByDescending(ur => ur.Count);
            foreach (var userGroup in orderedGroups)
            {
                Logger.OutputInformation("Read " + userGroup.Read.ToString() + " User " +
                    userGroup.UserName + "(" + userGroup.Count + ")");
            }
        }
        public struct BookTypeRead
        {
            public string bookType;
            public bool read;
        }
        void DisplayCountsBookTypesReads()
        {
            List<BookTypeRead> bookTypeReadList = new List<BookTypeRead>();

            foreach (Book book in _context.Books)
            {
                BookTypeRead btr = new BookTypeRead();

                User user = _context.Users.FirstOrDefault(b => b.UserId ==
                book.UserId);
                if (book.UserId == Login.currentUserId)
                {
                    BookType bookType = _context.BookTypes.FirstOrDefault(bt => bt.BookTypeId ==
                    book.BookTypeId);
                    btr.bookType = bookType.BK_TYPE;
                    btr.read = book.Read;
                    bookTypeReadList.Add(btr);
                }
            }
            var groups = bookTypeReadList
                .GroupBy(btr => new { btr.bookType, btr.read })
                .Select(g => new
                {
                    BookType = g.Key.bookType,
                    Read = g.Key.read,
                    Count = g.Count()
                });
            var orderedGroups = groups.OrderByDescending(btr => btr.Count);
            foreach (var btGroup in orderedGroups)
            {
                Logger.OutputInformation("Read " + btGroup.Read.ToString() + " Book Type " +
                    btGroup.BookType + "(" + btGroup.Count + ")");
            }
        }
        public void DisplayVersion()
        {
            Console.WriteLine("Software Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
            RefreshScreenIds();
        }
    }
    class Result
    {
        public string AuthorName;
        public int Count;
    }
    class TagResult
    {
        public string TagName;
        public int Count;
    }
    class BookTypeResult
    {
        public string BookTypeName;
        public int Count;
    }
    class ReadResult
    {
        public string ReadValue;
        public int Count;
    }
    class UserResult
    {
        public string UserName;
        public int Count;
    }


    public class Encode_Decode
    {
        public static class Global
        {
            // set password
            public const string strPassword = "LetMeIn99$";

            // set permutations
            public const String strPermutation = "oputedgytrd";
            public const Int32 bytePermutation1 = 0x34;
            public const Int32 bytePermutation2 = 0x78;
            public const Int32 bytePermutation3 = 0x3A;
            public const Int32 bytePermutation4 = 0x06;
        }
        /*            // set permutations
                    public const String strPermutation = "ouiveyxaqtd";
                    public const Int32 bytePermutation1 = 0x19;
                    public const Int32 bytePermutation2 = 0x59;
                    public const Int32 bytePermutation3 = 0x17;
                    public const Int32 bytePermutation4 = 0x41;*/



        // encoding
        public static string Encrypt(string strData)
        {

            return System.Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(strData)));
            // reference https://msdn.microsoft.com/en-us/library/ds4kkd55(v=vs.110).aspx

        }


        // decoding
        public static string Decrypt(string strData)
        {
            return Encoding.UTF8.GetString(Decrypt(System.Convert.FromBase64String(strData)));
            // reference https://msdn.microsoft.com/en-us/library/system.convert.frombase64string(v=vs.110).aspx

        }

        // encrypt
        public static byte[] Encrypt(byte[] strData)
        {
            PasswordDeriveBytes passbytes =
            new PasswordDeriveBytes(Global.strPermutation,
            new byte[] { Global.bytePermutation1,
                         Global.bytePermutation2,
                         Global.bytePermutation3,
                         Global.bytePermutation4
            });

            MemoryStream memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(memstream,
            aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();
            return memstream.ToArray();
        }

        // decrypt
        public static byte[] Decrypt(byte[] strData)
        {
            PasswordDeriveBytes passbytes =
            new PasswordDeriveBytes(Global.strPermutation,
            new byte[] { Global.bytePermutation1,
                         Global.bytePermutation2,
                         Global.bytePermutation3,
                         Global.bytePermutation4
            });

            MemoryStream memstream = new MemoryStream();
            Aes aes = new AesManaged();
            aes.Key = passbytes.GetBytes(aes.KeySize / 8);
            aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

            CryptoStream cryptostream = new CryptoStream(memstream,
            aes.CreateDecryptor(), CryptoStreamMode.Write);
            cryptostream.Write(strData, 0, strData.Length);
            cryptostream.Close();
            return memstream.ToArray();
        }
        // reference
        // https://msdn.microsoft.com/en-us/library/system.security.cryptography(v=vs.110).aspx
        // https://msdn.microsoft.com/en-us/library/system.security.cryptography.cryptostream%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        // https://msdn.microsoft.com/en-us/library/system.security.cryptography.rfc2898derivebytes(v=vs.110).aspx
        // https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
    }
    /*
using System.IO;
using System.Text;
 
namespace SecurePassword
    {
        class Encode_Decode
        {
            public static class Global
            {
                // set password
                public const string strPassword = "LetMeIn99$";

                // set permutations
                public const String strPermutation = "ouiveyxaqtd";
                public const Int32 bytePermutation1 = 0x19;
                public const Int32 bytePermutation2 = 0x59;
                public const Int32 bytePermutation3 = 0x17;
                public const Int32 bytePermutation4 = 0x41;
            }


            // The console window
            public static void Main(String[] args)
            {

                Console.Title = "Secure Password v2";
                Console.WriteLine("Output---");
                Console.WriteLine("");

                Console.WriteLine("Password:  " + Global.strPassword);

                string strEncrypted = (Encrypt(Global.strPassword));
                Console.WriteLine("Encrypted: " + strEncrypted);

                string strDecrypted = (Decrypt(strEncrypted));
                Console.WriteLine("Decrypted: " + strDecrypted);

                Console.ReadKey();
            }




            // encoding
            public static string Encrypt(string strData)
            {

                return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(strData)));
                // reference https://msdn.microsoft.com/en-us/library/ds4kkd55(v=vs.110).aspx

            }


            // decoding
            public static string Decrypt(string strData)
            {
                return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(strData)));
                // reference https://msdn.microsoft.com/en-us/library/system.convert.frombase64string(v=vs.110).aspx

            }

            // encrypt
            public static byte[] Encrypt(byte[] strData)
            {
                PasswordDeriveBytes passbytes =
                new PasswordDeriveBytes(Global.strPermutation,
                new byte[] { Global.bytePermutation1,
                         Global.bytePermutation2,
                         Global.bytePermutation3,
                         Global.bytePermutation4
                });

                MemoryStream memstream = new MemoryStream();
                Aes aes = new AesManaged();
                aes.Key = passbytes.GetBytes(aes.KeySize / 8);
                aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

                CryptoStream cryptostream = new CryptoStream(memstream,
                aes.CreateEncryptor(), CryptoStreamMode.Write);
                cryptostream.Write(strData, 0, strData.Length);
                cryptostream.Close();
                return memstream.ToArray();
            }

            // decrypt
            public static byte[] Decrypt(byte[] strData)
            {
                PasswordDeriveBytes passbytes =
                new PasswordDeriveBytes(Global.strPermutation,
                new byte[] { Global.bytePermutation1,
                         Global.bytePermutation2,
                         Global.bytePermutation3,
                         Global.bytePermutation4
                });

                MemoryStream memstream = new MemoryStream();
                Aes aes = new AesManaged();
                aes.Key = passbytes.GetBytes(aes.KeySize / 8);
                aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

                CryptoStream cryptostream = new CryptoStream(memstream,
                aes.CreateDecryptor(), CryptoStreamMode.Write);
                cryptostream.Write(strData, 0, strData.Length);
                cryptostream.Close();
                return memstream.ToArray();
            }
            // reference
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography(v=vs.110).aspx
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.cryptostream%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.rfc2898derivebytes(v=vs.110).aspx
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        }
    }*/
}