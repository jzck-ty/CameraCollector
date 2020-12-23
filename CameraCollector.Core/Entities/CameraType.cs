using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CameraCollector.Core.Entities
{
    public class CameraType
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(128)]
        public string DefaultUsername { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(128)]
        public string DefaultPassword { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(1024)]
        public string StreamUrl { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(128)]
        public string SearchTerm { get; set; }
    }
}
