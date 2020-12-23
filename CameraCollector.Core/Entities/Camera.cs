using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CameraCollector.Core.Entities
{
    public class Camera
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("CameraTypeId")]
        public virtual CameraType CameraType { get; set; }
        public Guid CameraTypeId { get; set; }

        [Required]
        [ForeignKey("HostId")]
        public virtual Host Host { get; set; }
        public Guid HostId { get; set; }

        [Required]
        public int Port { get; set; }

        [DataType(DataType.Text)]
        [StringLength(128)]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(2048)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Password { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime FoundOn { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastPinged { get; set; }

        [Required]
        public bool Active { get; set; }
    }
}
