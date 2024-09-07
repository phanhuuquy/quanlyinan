using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyInAn.Models
{
    public class ConfirmEmail
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ConfirmCode { get; set; }
        public DateTime ExpiryTime { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsConfirm { get; set; }
    }
}
