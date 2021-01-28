using System.Collections.Generic;

namespace BookListDB
{
    public class Screens
    {
        public List<Screen> screen = new List<Screen>();
        public int currentScreensId = -1;
        public int InputFileLength
        {
            get { return currentScreensId == -1 ? 0 : screen[currentScreensId].inputFileLength; }
            set { if (currentScreensId != -1)  screen[currentScreensId].inputFileLength = value; }
        }
        public int StartCmdNoBooks
        {
            get { return screen[currentScreensId].startCmdNoBooks; }
            set { screen[currentScreensId].startCmdNoBooks = value; }
        }
        public int JumpTo(int jumpIndex)
        {
            return (screen[currentScreensId].JumpTo(jumpIndex));
        }

        public bool EmptyScreen()
        {
            return screen[currentScreensId].EmptyScreen();
        }

        public void AddScreen(Command cmd)
        {
            Screen oldScreen = screen[currentScreensId];
            Screen newScreen = new Screen();
            newScreen.PopulateBooks(oldScreen.books, oldScreen._context);
            newScreen.ReadScreenIds();
            newScreen.command = cmd;
            screen.Add(newScreen);
            currentScreensId = screen.Count - 1;
        }
        public void Initialise(BookListContext context)
        {
            // create an intial screen
            Screen newScreen = new Screen();
            newScreen.Initialise(context);
            newScreen.ReadScreenIds();
            screen = new List<Screen>
            {
                newScreen
            };
            currentScreensId = 0;
        }

        public void ReadScreenIds()
        {
            screen[currentScreensId].ReadScreenIds();
        }
        public void RefreshScreenIds()
        {
            screen[currentScreensId].RefreshScreenIds();
        }
        public void DownPage()
        {
            screen[currentScreensId].DownPage();
        }
        public void DownOne()
        {
            screen[currentScreensId].DownOne();
        }
        public void UpPage()
        {
            screen[currentScreensId].UpPage();
        }
        public void UpOne()
        {
            screen[currentScreensId].UpOne();
        }
        public void Home()
        {
            screen[currentScreensId].Home();
        }
        public void End()
        {
            screen[currentScreensId].End();
        }
        public void DisplayBooksScreen()
        {
            screen[currentScreensId].DisplayBooksScreen();
        }

        public void Order(Command cmd, FieldType field, OrderType order)
        {
            int prevTopBookId;
            int prevBottomBookId;

            var prevTopIndex = screen[currentScreensId].topScreenId;
            prevTopBookId = screen[currentScreensId].books[prevTopIndex].BookId;
            var prevBottomIndex = screen[currentScreensId].bottomScreenId;
            prevBottomBookId = screen[currentScreensId].books[prevBottomIndex].BookId;

            AddScreen(cmd);
            screen[currentScreensId].Order(field, order, prevTopBookId, prevBottomBookId);
        }
        public void Filter(Command cmd, FieldType field, string query)
        {
            AddScreen(cmd);
            screen[currentScreensId].Filter(field, query);
        }
        public void Import(string inputFile)
        {
            screen[currentScreensId].Import(inputFile);
        }
        public void XmlSave(string outputFile)
        {
            screen[currentScreensId].XmlSave(outputFile);
        }
        public void XmlLoad(string inputFile)
        {
            screen[currentScreensId].XmlLoad(inputFile);
        }
        public void DisplayBook(int bookId)
        {
            screen[currentScreensId].DisplayBook(bookId);
        }
        public int CreateBook(string query, string user, int bookTypeRowNo, bool read, string tagValues)
        {
            return (screen[currentScreensId].CreateBook(query, user, bookTypeRowNo, read, tagValues));
        }
        public void DeleteBook(int bookId)
        {
            screen[currentScreensId].DeleteBook(bookId);
        }
        public void DeleteBookAuthor(int bookId, string authorName)
        {
            screen[currentScreensId].DeleteBookAuthor(bookId, authorName);
        }
        public void DeleteBookTag(int bookId, string tagValue)
        {
            screen[currentScreensId].DeleteBookTag(bookId, tagValue);
        }
        public void UpdateBookTitle(int bookId, string title)
        {
            screen[currentScreensId].UpdateBookTitle(bookId, title);
        }
        public void UpdateBookAuthor(int bookId, string authorName)
        {
            screen[currentScreensId].UpdateBookTitle(bookId, authorName);
        }
        public void UpdateBookTag(int bookId, string tagValue)
        {
            screen[currentScreensId].UpdateBookTitle(bookId, tagValue);
        }
        public void Convert(string inputFile, string bookType)
        {
            screen[currentScreensId].Convert(inputFile, bookType);
        }
        public void Update(FieldType field, int updIndex, string newValue)
        {
            screen[currentScreensId].Update(field, updIndex, newValue);
        }
        public void RegisterNewUser(string FirstName,
           string MiddleName, string Surname, string Encrypted,
           string UserName, string Email)
        {
            screen[currentScreensId].RegisterNewUser(FirstName, MiddleName,
                Surname, Encrypted, UserName, Email);
        }
        public bool LoginUser(string UserName, string Encrypted)
        {
            return (screen[currentScreensId].LoginUser(UserName, Encrypted));
        }
        public bool EmailUserPassword(string UserName)
        {
            return (screen[currentScreensId].EmailUserPassword(UserName));
        }
        public bool ChangeUserPassword(string userName, string encryptedPassword,
                    string newEncryptedPassword, string repeatEncryptedPassword)
        {
            return (screen[currentScreensId].ChangeUserPassword(userName,
                    encryptedPassword, newEncryptedPassword, repeatEncryptedPassword));
        }
        public void Delete(FieldType field, int updIndex, string delValue)
        {
            screen[currentScreensId].Delete(field, updIndex, delValue);
        }
        public void FillInEmptyTags()
        {
            screen[currentScreensId].FillInEmptyTags();
        }
        public void DisplayCounts(FieldType field)
        {
            screen[currentScreensId].DisplayCounts(field);
        }
        public void DisplayVersion()
        {
            screen[currentScreensId].DisplayVersion();
        }
    }
}