using System;

namespace LibraryManagerWpf
{
    internal class Book
    {
        public Guid bookId { get; set; } = Guid.NewGuid();
        public string title {get;set;}
        public string description {get;set;}
        public string isbn {get; set;}
        public bool available { get; set; } = true;
        public Author author {get;set;}

        public Book() { }
        public Book(string title, string description, string isbn, Author author)
        {
            this.title = title;
            this.isbn = isbn;
            this.description = description;
            this.author = author;
        }
    }
}
