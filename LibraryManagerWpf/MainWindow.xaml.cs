using System;
using System.Windows;
using System.Windows.Media;


namespace LibraryManagerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Library library;
        private bool isEditingBook;
        private Book selectedBook;
        private bool isFilteredView;
        public MainWindow()
        {
            InitializeComponent();
            library = new Library();
            datagridLibrary.ItemsSource = library.GetBookList();

            // disables invent books if no api key is found
            if (!library.connectorAPI.ApiKeyFound()) DisableInventButton();
        }

        /// <summary>
        /// checks if any text field is empty
        /// </summary>
        /// <returns>true if a field is empty</returns>
        private bool FieldsAreEmpty()
        {
            string fullAuthorName = textboxAuthor.Text;
            string isbn = textboxISBN.Text;
            string title = textboxTitle.Text;

            return (string.IsNullOrEmpty(fullAuthorName) || string.IsNullOrEmpty(isbn) || string.IsNullOrEmpty(title));
        }

        /// <summary>
        /// checks if the author text field contains at least 2 parts
        /// </summary>
        /// <returns></returns>
        private bool IsAuthorFieldValid()
        {
            string fullAuthorName = textboxAuthor.Text;
            string[] authorParts = fullAuthorName.Split();
            return (authorParts.Length >= 2);
        }

        /// <summary>
        /// creates book, author from the text fields
        /// </summary>
        /// <returns>Book</returns>
        private Book CreateBookFromTextFields()
        {
            // retrieve field values
            string fullAuthorName = textboxAuthor.Text;
            string isbn = textboxISBN.Text;
            string title = textboxTitle.Text;
            string description = textboxDescription.Text;

            string[] authorParts = fullAuthorName.Split();

            Author newAuthor = new Author(authorParts[0], authorParts[1]);
            Book newBook = new Book(title, description,isbn, newAuthor);

            return newBook;
        }
        
        /// <summary>
        /// sets selected book data into the textfields
        /// </summary>
        private void SetSelectedBookIntoTextFields()
        {
            textboxISBN.Text = selectedBook.isbn;
            textboxAuthor.Text = selectedBook.author.name + " " + selectedBook.author.lastname;
            textboxTitle.Text = selectedBook.title;
            textboxDescription.Text = selectedBook.description;
            isEditingBook = true;
            buttonEdit.Content = "Cancel";
            textboxDescription.IsReadOnly = false;
        }

        /// <summary>
        /// clears all textfields
        /// </summary>
        private void ClearTextBox()
        {
            textboxISBN.Text = null;
            textboxAuthor.Text = null;
            textboxTitle.Text = null;
            textboxDescription.Text = null;
        }

        /// <summary>
        /// enables invent button and slider
        /// </summary>
        private void EnableInventButton()
        {
            sliderBookQuantity.IsEnabled = true;
            buttonInventBooks.IsEnabled = true;
            buttonInventBooks.Foreground = Brushes.White;
        }

        /// <summary>
        /// diable invent button and slider
        /// </summary>
        private void DisableInventButton()
        {
            buttonInventBooks.IsEnabled = false;
            buttonInventBooks.Foreground = Brushes.DarkGray;
            sliderBookQuantity.IsEnabled = false;
        }

        /// <summary>
        /// sets itemsource back to the normal list,
        /// deactivate reset button
        /// </summary>
        private void ResetFilter()
        {
            datagridLibrary.ItemsSource = library.GetBookList();
            textboxSearch.Text = string.Empty;
            isFilteredView = false;
            buttonResetSearch.IsEnabled = false;
            buttonResetSearch.Foreground = Brushes.DarkGray;
        }

        /// <summary>
        /// uses library.Borrowbook on the selected book
        /// </summary>
        private void ButtonLend_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook == null) { MessageBox.Show("No book selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            library.BorrowBook(selectedBook);
            datagridLibrary.Items.Refresh();
        }

        /// <summary>
        /// deletes selected book from library
        /// </summary>
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook == null) { MessageBox.Show("No book selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            library.DeleteBookFromLibrary(selectedBook);
            datagridLibrary.Items.Refresh();
        }

        /// <summary>
        /// sets selected book data into the text fields, and sets isEditingBook to true
        /// </summary>
        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook == null) { MessageBox.Show("No book selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            if (!isEditingBook) SetSelectedBookIntoTextFields();

            // reset button and empty fields
            else
            {
                buttonEdit.Content = "Edit";
                isEditingBook = false;
                ClearTextBox();
            }
        }

        /// <summary>
        /// changes the ItemSource to the filtered view based on the search string
        /// </summary>
        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchText = textboxSearch.Text;
            if (searchText.Length < 1) return;

            buttonResetSearch.IsEnabled = true;
            buttonResetSearch.Foreground = Brushes.White;

            var filteredItems = library.FilterBooks(searchText);
            datagridLibrary.ItemsSource = filteredItems;
            datagridLibrary.Items.Refresh();
            isFilteredView = true;
        }

        /// <summary>
        /// sets itemsource back to the normal list
        /// </summary>
        private void ButtonResetSearch_Click(object sender, RoutedEventArgs e)
        {
            ResetFilter();
        }

        /// <summary>
        /// inserts description of selected book into the textbox
        /// </summary>
        private void ButtonShowDescription_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook == null) { MessageBox.Show("No book selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }

            textboxDescription.Text = selectedBook.description;
        }

        /// <summary>
        /// saves/updates book to the library
        /// </summary>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (FieldsAreEmpty()) { MessageBox.Show("Please fill a fields."); return; };

            if (!IsAuthorFieldValid()) { MessageBox.Show("Please enter a full Author Name."); return; };

            Book newBook = CreateBookFromTextFields();

            if (isEditingBook)
            {
                library.UpdateBook(selectedBook, newBook);
                buttonEdit.Content = "Change";
                isEditingBook = false;
            }
            else
            {
                library.AddBookToLibrary(newBook);
            }
            if (isFilteredView) ResetFilter();
            // clean fields and refresh grid
            datagridLibrary.Items.Refresh();
            ClearTextBox();
        }

        /// <summary>
        /// Updates text of Inventbutton based on the slider value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderBookQuantity_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (sliderBookQuantity == null || buttonInventBooks == null) return;

            int sliderValue = (int)sliderBookQuantity.Value;

            if (sliderValue == 1) buttonInventBooks.Content = $"Invent {sliderValue} book";

            else buttonInventBooks.Content = $"Invent {sliderValue} books";
        }

        /// <summary>
        /// uses library.GenerateBooks to generate books based on the selected slider quantity
        /// </summary>
        private async void ButtonInventBooks_Click(object sender, RoutedEventArgs e)
        {
            string buttonTextBefore = buttonInventBooks.Content.ToString();
            // deactivate button while generating
            buttonInventBooks.Content = "Inventing...";
            DisableInventButton();

            int bookCount = Convert.ToInt32(sliderBookQuantity.Value);
            await library.GenerateBooks(bookCount);
            datagridLibrary.Items.Refresh();

            // reactivate button
            buttonInventBooks.Content = buttonTextBefore;
            EnableInventButton();
        }

        /// <summary>
        /// retrieves selected row and sets it as a Book to the selectedBook
        /// </summary>
        /// 
        /// <summary>
        /// updates the InventBooks button text
        /// </summary>
        private void DatagridLibrary_SelectedCellsChanged(object sender, EventArgs e)
        {
            selectedBook = datagridLibrary.SelectedItem as Book;
            textboxDescription.Text = string.Empty;
        }
    }
}
