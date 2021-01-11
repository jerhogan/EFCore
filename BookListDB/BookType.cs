using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    public class BookType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BookTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Description { get; set; }
        [Required]
        [MaxLength(50)]
        public string BK_TYPE { get; set; }
        public int ShoppingListNo { get; set; }
    }
}
