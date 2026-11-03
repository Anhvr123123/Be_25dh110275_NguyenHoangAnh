using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Be_25dh110275_NguyenHoangAnh.Models.ViewModel
{
    public class CategoryMetadata
    {
        [HiddenInput]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)] // Đã sửa stringLength thành StringLength
        public string CategoryName { get; set; }
    }

    public class UserMetadata
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 5)] // Đã sửa MinnimumLength thành MinimumLength
        [RegularExpression(@"^[a-zA-Z0-9]([._-](?![._-])|[a-zA-Z0-9]){3,18}[a-zA-Z0-9]$")] // Đã sửa RegularExpression sạch sẽ
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string UserRole { get; set; }
    }
}