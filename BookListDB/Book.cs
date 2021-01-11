using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookListDB
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int BookId { get; set; }

        [Required]
        [MaxLength(50)]
        public string GoogleId { get; set; }
        [Required]
        [MaxLength(300)]
        public string Title { get; set; }
        [Required]
        [MaxLength(200)]
        public string SubTitle { get; set; }
        [Required]
        [MaxLength(10000)]
        public string Description { get; set; }
        public int PageCount { get; set; }
        [Required]
        [MaxLength(50)]
        public string PrintType { get; set; }
        [Required]
        [MaxLength(50)]
        public string PublishedDate { get; set; }
        [Required]
        [MaxLength(50)]
        public string Publisher { get; set; }
        [Required]
        [MaxLength(200)]
        public string SmallThumbNail { get; set; }
        [Required]
        [MaxLength(200)]
        public string ThumbNail { get; set; }
        public int UserId { get; set; }
        public int BookTypeId { get; set; }
        [Required]
        public bool Read { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("BookTypeId")]
        public virtual BookType BookType { get; set; }
        public virtual List<Tag> Tags { get; set; }
    }
}
