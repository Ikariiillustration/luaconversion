using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace project2.Models
{
    public class Entry
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Channel { get; set; }
        [Required]
        public string Directory { get; set; } //iterate through array, and compile each entry into a single string seperated with breaks
    }
}