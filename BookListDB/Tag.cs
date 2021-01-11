using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    public class Tag
    {
/*        public Tag(Tag tag)
        {
            TagId = tag.TagId;
            BookId = tag.BookId;
            Value = tag.Value;
        }
        public Tag()
        {

        }
*/

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TagId { get; set; }

        [Required]
        public int BookId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Value { get; set; }

    }
}
