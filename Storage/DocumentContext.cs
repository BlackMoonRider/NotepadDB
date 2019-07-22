using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Storage
{
    public class DocumentContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        
        public DocumentContext() : base("DocumentDBConnection") { }
    }
}
