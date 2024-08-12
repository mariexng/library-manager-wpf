using OpenAI_API;
using OpenAI_API.Completions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LibraryManagerWpf
{
    internal class ChatGptApiConnector
    {
        private string ApiKey;
        private OpenAIAPI openAI;

        public ChatGptApiConnector()
        {
            GetApiKeyFromTextFile();
            openAI = new OpenAIAPI(ApiKey);
        }

        /// <summary>
        /// retrieves the api key from an txt file
        /// </summary>
        private void GetApiKeyFromTextFile()
        {
            string path = @"..\..\apikey.txt";
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    ApiKey = reader.ReadLine();
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("No file 'apikey.txt' found.");
            }
        }

        /// <summary>
        /// checks if an apikey was found and inserted into the field
        /// </summary>
        /// <returns>bool</returns>
        public bool ApiKeyFound()
        {
            return !string.IsNullOrEmpty(ApiKey);
        }

        /// <summary>
        /// Generates multiple books based on the given quantity
        /// </summary>
        /// <returns>List of type Book</returns>
        public async Task<List<Book>> GenerateMultipleBooksAsync(int quantity)
        {
            List<Book> generatedBooks = new List<Book>();
            bool isValidBook;

            for (int i = 0; i < quantity; i++)
            {
                isValidBook = false;
                while (!isValidBook)
                {
                    // generates book with chatgpt api
                    string bookString = await ChatGptApiRequest("Generate a Book. In exactly this Format(no \" and no Title: ..): title, author, xxx-x-xxxxx-x", 30);

                    //checks if it has at least 3 bookParts, (author, title, isbn)
                    string[] bookParts = bookString.Split(',');
                    if (bookParts.Length < 3) { Console.WriteLine($"Unaccepted book format: {bookString}"); continue; }
                    
                    // checks if name and lastname was generated
                    string[] authorParts = bookParts[1].Trim().Split();
                    if (authorParts.Length < 2) continue;
                        
                    Author autor = new Author()
                    {
                        authorId = Guid.NewGuid(),
                        name = authorParts[0],
                        lastname = authorParts[1],
                    };

                    Book buch = new Book()
                    {
                        bookId = Guid.NewGuid(),
                        title = bookParts[0].Trim().Trim('"'),
                        author = autor,
                        isbn = bookParts[2].Trim()
                    };

                    // creates description and adds book to list
                    buch.description = await ChatGptApiRequest($"Generate a description of the book {buch.title} from {buch.author.name} {buch.author.lastname}.1 short sentance, no special signs", 50);
                    generatedBooks.Add(buch);
                    isValidBook = true;
                }
            }
            Console.WriteLine($"{generatedBooks.Count} books generated.");
            return generatedBooks;
        }

        /// <summary>
        /// CompletionRequest based on the given prompt and max tokens
        /// </summary>
        /// <returns>completion request answere string</returns>
        private async Task<string> ChatGptApiRequest(string prompt, int maxTokens)
        {
            var completionsRequest = new CompletionRequest
            {
                Model = "gpt-3.5-turbo-instruct",
                MaxTokens = maxTokens,
                Prompt = prompt
            };
            try
            {
                var response = await openAI.Completions.CreateCompletionAsync(completionsRequest);

                if (response != null) return response.Completions[0].Text.Trim();

                return "no completion received.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
