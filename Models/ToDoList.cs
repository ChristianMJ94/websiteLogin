using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace websiteLogin.Models
{
    public class ToDoList
    {
        [ForeignKey("UserId")]
        public int UserId { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TodoId { get; set; }

        [Required]
        public string Beskrivelse { get; set; }
    }
}