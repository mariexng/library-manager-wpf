using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LibraryManagerWpf
{
    internal class Library
    {
        private List<Book> books;
        private List<Author> authors;
        public ChatGptApiConnector connectorAPI;

        public Library()
        {
            books = new List<Book>();
            authors = new List<Author>();
            connectorAPI = new ChatGptApiConnector();
            LoadFromJSON();
        }
        
        /// <summary>
        /// sets the author id, based on his being already in the list or not
        /// if not adds author to authors list
        /// </summary>
        private void SetAuthorId(Author author)
        {
            // returns an author, if an author with same name is found
            var foundAuthor = authors.FirstOrDefault(a => a.lastname == author.lastname && a.name == author.name);

            // set id to found author
            if (foundAuthor != null) author.authorId = foundAuthor.authorId;

            // add new author
            else AddAuthorToLibrary(author);
        } 
        /// <summary>
        /// adds book to the books list,
        /// checks if author is new or already on the list
        /// </summary>
        public void AddBookToLibrary(Book book)
        {
            SetAuthorId(book.author);

            books.Add(book);
            SaveAsJson();
        }

        /// <summary>
        /// deletes a book from books list
        /// </summary>
        public void DeleteBookFromLibrary(Book book)
        {
            books.Remove(book);
            SaveAsJson();
        }

        /// <summary>
        /// changes the book.available to the opposite of his current state
        /// </summary>
        public void BorrowBook(Book book)
        {
            book.available = !book.available;
            SaveAsJson();
        }

        /// <summary>
        /// updates the values of one book
        /// checks if author is new or already on the list
        /// </summary>
        public void UpdateBook(Book originalBook, Book editedBook)
        {
            SetAuthorId(editedBook.author);

            // replaces the existing book with the edited one
            var index = books.FindIndex(b => b.bookId == originalBook.bookId);
            books[index] = editedBook;
            
            SaveAsJson();
        }

        /// <summary>
        /// add author to the authors list
        /// </summary>
        public void AddAuthorToLibrary(Author author)
        {
            authors.Add(author);
        }

        /// <summary>
        /// searches books based on the search string
        /// </summary>
        /// <returns>filtered list of type Book</returns>
        public List<Book> FilterBooks(string searchText)
        {
            var filteredItems = books.Where(b => b.title.Contains(searchText) || b.author.lastname.Contains(searchText) || b.author.name.Contains(searchText)).ToList();
            return filteredItems;
        }

        /// <summary>
        /// generates books with ChatGpt Api and adds the books to the list
        /// </summary>
        /// <returns></returns>
        public async Task GenerateBooks(int bookCount)
        {
            List<Book> generatedBookList = await connectorAPI.GenerateMultipleBooksAsync(bookCount);
            if (generatedBookList == null || generatedBookList.Count < 1) return;

            foreach (Book book in generatedBookList)
            {
                AddBookToLibrary(book);
            }
        }

        /// <summary>
        /// returns booklist
        /// </summary>
        /// <returns></returns>
        public List<Book> GetBookList()
        {
            return books;
        }

        /// <summary>
        /// saves the book list as an json file
        /// </summary>
        private void SaveAsJson()
        {
            string jsonPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\books.json");
            try
            {
                string jsonString = JsonConvert.SerializeObject(books, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(jsonPath, jsonString);
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"JSON Error: {ex.Message}");
            }
        }

        /// <summary>
        /// loads books and authors from a json file and inserts it into the books & authors list
        /// </summary>
        private void LoadFromJSON()
        {
            try
            {
                string jsonPath = System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\books.json");
                string reader = File.ReadAllText(jsonPath);
                List<Book> booklist = JsonConvert.DeserializeObject<List<Book>>(reader);

                // set retrieved values into the lists
                books = booklist;
                authors = booklist.Select(book => book.author).Distinct().ToList();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("JSON Error: File 'books.json' not found.");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Fehler: {ex.Message}");
            }
        }
    }
}
