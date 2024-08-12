using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryManagerWpf
{
    internal class Author
    {
        public Guid authorId {  get; set; } = Guid.NewGuid();
        public string name {  get; set; }
        public string lastname { get; set; }

        public Author() { }

        public Author(string name, string lastname)
        {
            this.name = name;
            this.lastname = lastname;
        }
    }
}
