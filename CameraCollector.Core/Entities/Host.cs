using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CameraCollector.Core.Entities
{
    public class Host
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [DataType(DataType.Text)]
        [StringLength(15)]
        public string IpAddress { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(2)]
        public string Country { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(128)]
        public string City { get; set; }

        [DataType(DataType.Text)]
        [StringLength(128)]
        public string Name { get; set; }

        [Required]
        public bool Active { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FoundOn { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastPinged { get; set; }

        public virtual ICollection<Camera> Cameras { get; set; }
    }
}
