﻿using System.ComponentModel.DataAnnotations;

namespace QuanLyInAn.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
